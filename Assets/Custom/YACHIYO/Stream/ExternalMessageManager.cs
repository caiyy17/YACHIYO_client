using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

namespace Yachiyo
{
    /// <summary>
    /// Bidirectional HTTP bridge between external services (gateway) and the Unity pipeline.
    ///
    /// Inbound (external → pipeline):
    ///   POST http://localhost:{port}/send — body is YYMessage JSON, injected into pipeline
    ///   GET  http://localhost:{port}/health — returns 200 OK
    ///
    /// Outbound (pipeline → external):
    ///   Receives pipeline output via OnPipelineOutput (bound to ProcessingPipeline.sendSignal)
    ///   and forwards to gateway via HTTP POST.
    /// </summary>
    public class ExternalMessageManager : MonoBehaviour
    {
        [Header("HTTP Server (Inbound)")]
        [SerializeField] private int port = 7890;

        [Header("Gateway (Outbound)")]
        [SerializeField] private string gatewayUrl = "http://localhost:8080";

        [Header("References")]
        [SerializeField] private ProcessingPipeline processingPipeline;

        private HttpListener _listener;
        private Thread _listenerThread;
        private volatile bool _running;
        private ConcurrentQueue<string> _pendingMessages = new ConcurrentQueue<string>();

        // Outbound HTTP client
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        void Start()
        {
            StartServer();
        }

        void Update()
        {
            while (_pendingMessages.TryDequeue(out string msg))
            {
                if (processingPipeline != null)
                {
                    processingPipeline.EnqueueMessage(msg);
                    Debug.Log($"[ExternalMessageManager] Injected into pipeline: {msg}");
                }
                else
                {
                    Debug.LogWarning("[ExternalMessageManager] ProcessingPipeline not assigned.");
                }
            }
        }

        void OnDestroy()
        {
            StopServer();
        }

        // ─── Pipeline Output Handler ───
        // Bind this to ProcessingPipeline.sendSignal in Inspector

        /// <summary>
        /// Called by ProcessingPipeline.sendSignal via DataRouter.
        /// Forwards the event to gateway for external processing.
        /// </summary>
        public void OnPipelineOutput(string eventName, string message)
        {
            Debug.Log($"[ExternalMessageManager] Pipeline event '{eventName}': {message}");
            _ = ForwardToGateway(eventName, message);
        }

        private async Task ForwardToGateway(string eventName, string message)
        {
            string url = $"{gatewayUrl}/api/pipeline-event";
            try
            {
                // message is the raw pipeline JSON (already contains signal, timestamp, etc.)
                string json = $"{{\"eventName\":\"{eventName}\",\"message\":{message}}}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"[ExternalMessageManager] Gateway forward failed: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ExternalMessageManager] Gateway forward error: {e.Message}");
            }
        }

        // ─── HTTP Server (Inbound) ───

        private void StartServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");

            try
            {
                _listener.Start();
                _running = true;
                _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                _listenerThread.Start();
                Debug.Log($"[ExternalMessageManager] HTTP server started on port {port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalMessageManager] Failed to start: {e.Message}");
            }
        }

        private void StopServer()
        {
            _running = false;
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        private void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // CORS
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            try
            {
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    response.Close();
                    return;
                }

                string path = request.Url.AbsolutePath.TrimEnd('/');

                if (path == "/health" && request.HttpMethod == "GET")
                {
                    SendResponse(response, 200, "{\"status\":\"ok\"}");
                }
                else if (path == "/send" && request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string body = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(body))
                        {
                            SendResponse(response, 400, "{\"error\":\"empty body\"}");
                            return;
                        }

                        _pendingMessages.Enqueue(body);
                        SendResponse(response, 200, "{\"status\":\"queued\"}");
                    }
                }
                else
                {
                    SendResponse(response, 404, "{\"error\":\"not found\"}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalMessageManager] Request error: {e.Message}");
                try { SendResponse(response, 500, "{\"error\":\"internal\"}"); } catch { }
            }
        }

        private void SendResponse(HttpListenerResponse response, int statusCode, string body)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(body);
            response.ContentLength64 = data.Length;
            response.OutputStream.Write(data, 0, data.Length);
            response.Close();
        }
    }
}
