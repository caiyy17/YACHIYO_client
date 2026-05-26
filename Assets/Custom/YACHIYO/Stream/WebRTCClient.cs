using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Audio;
using UnityEngine.UI;
using Unity.WebRTC;
using Debug = UnityEngine.Debug;

namespace Yachiyo
{
    public class WebRTCClient : MonoBehaviour
    {
        public string serverUrl = "localhost:15168"; // WebRTC signaling server
        public string mainServerUrl = "localhost:8910"; // Main REST API server
        public string clientId = "unity-client";
        public string pipelineConfig = "default";

        [SerializeField] private RawImage receiveImage;
        [SerializeField] private AudioSource receiveAudio;
        [SerializeField] private AudioMixerGroup captureMixerGroup; // Silent mixer group for mic capture

        public enum VideoSourceMode { None, Camera, Webcam }

        [Header("Video")]
        [SerializeField] private VideoSourceMode videoSource = VideoSourceMode.None;
        [SerializeField] private int videoWidth = 320;
        [SerializeField] private int videoHeight = 240;
        [SerializeField] private int videoFps = 30;

        [Header("Video Source — Camera")]
        [SerializeField] private Camera sendCamera;

        [Header("Video Source — Webcam")]
        [Tooltip("Leave empty for default device.")]
        [SerializeField] private string webcamDeviceName;

        [Header("Stats (Read Only)")]
        [SerializeField] private string sendStats = "-";
        [SerializeField] private string recvStats = "-";

        private RTCPeerConnection _pc;
        private Coroutine _statsCoroutine;
        private ulong _lastFramesSent;
        private ulong _lastFramesReceived;

        // Receive tracks
        private VideoStreamTrack receiveVideoTrack;
        private AudioStreamTrack receiveAudioTrack;

        // Send tracks
        private GameObject micAudioObject;
        private AudioSource micAudioSource;
        private AudioStreamTrack sendAudioTrack;
        private RenderTexture sendVideoTexture;
        private VideoStreamTrack sendVideoTrack;
        private WebCamTexture _webcamTex;

        // Data channels
        private RTCDataChannel clientDataChannel; // "client-signals" — created by us
        private RTCDataChannel serverDataChannel; // "server-data" — created by server

        // Single message queue for all server-data DC messages
        public Queue<string> messageQueue = new Queue<string>();

        private void Awake()
        {
            serverUrl = PlayerPrefs.GetString("webrtcUrlInput", serverUrl);
            clientId = PlayerPrefs.GetString("userId", clientId);
            mainServerUrl = PlayerPrefs.GetString("urlInput", mainServerUrl);
            pipelineConfig = PlayerPrefs.GetString("pipelineConfig", pipelineConfig);

            serverUrl = $"http://{serverUrl}";
            mainServerUrl = $"http://{mainServerUrl}";
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Start()
        {
            StartCoroutine(ConnectFlow());
            StartCoroutine(WebRTC.Update());
        }

        /// <summary>
        /// Full connection flow: register → init pipeline → WebRTC signaling
        /// </summary>
        private IEnumerator ConnectFlow()
        {
            // Register and init_pipeline are handled by GameStart (SetupPipeline)
            yield return StartCoroutine(Call());
        }

        private IEnumerator Register()
        {
            Debug.Log("Registering client...");
            string json = $"{{\"client_id\":\"{clientId}\"}}";
            byte[] postData = Encoding.UTF8.GetBytes(json);

            UnityWebRequest request = new UnityWebRequest($"{mainServerUrl}/register/", "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Registration failed: {request.error}");
            else
                Debug.Log($"Registration successful: {request.downloadHandler.text}");
        }

        private IEnumerator InitPipeline()
        {
            Debug.Log("Initializing pipeline...");
            string json = $"{{\"config\":\"{pipelineConfig}\"}}";
            byte[] postData = Encoding.UTF8.GetBytes(json);

            UnityWebRequest request = new UnityWebRequest($"{mainServerUrl}/init_pipeline/{clientId}", "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Init pipeline failed: {request.error}");
            else
                Debug.Log($"Init pipeline successful: {request.downloadHandler.text}");
        }

        private IEnumerator Call()
        {
            RTCConfiguration config = GetSelectedSdpSemantics();
            _pc = new RTCPeerConnection(ref config);
            Debug.Log("Created peer connection object");

            _pc.OnIceConnectionChange = OnIceConnectionChange;
            _pc.OnConnectionStateChange = OnConnectionStateChange;
            _pc.OnTrack = OnTrack;
            _pc.OnDataChannel = OnDataChannel;

            // Add local mic audio track → transceiver becomes SendRecv
            SetupMicAudioTrack();

            // Add local video track (if source configured) → transceiver becomes SendRecv
            SetupVideoTrack();

            // Create "client-signals" DataChannel for VAD signals
            RTCDataChannelInit dcConf = new RTCDataChannelInit { ordered = true };
            clientDataChannel = _pc.CreateDataChannel("client-signals", dcConf);
            clientDataChannel.OnOpen = () => Debug.Log("client-signals DataChannel opened");
            clientDataChannel.OnClose = () => Debug.Log("client-signals DataChannel closed");

            // Create offer
            var op = _pc.CreateOffer();
            yield return op;

            if (op.IsError)
            {
                Debug.LogError($"Failed to create offer: {op.Error.message}");
                yield break;
            }

            Debug.Log("Created offer successfully");
            RTCSessionDescription desc = op.Desc;
            var localDescOp = _pc.SetLocalDescription(ref desc);
            yield return localDescOp;

            if (localDescOp.IsError)
            {
                Debug.LogError($"Failed to set local description: {localDescOp.Error.message}");
                yield break;
            }

            Debug.Log("Set local description successfully");
            yield return StartCoroutine(SendOfferToServer(op.Desc));

            _statsCoroutine = StartCoroutine(StatsCoroutine());
        }

        [Header("Mic Sync")]
        [Tooltip("AudioSource plays this far behind mic write position (ms)")]
        [SerializeField] private float micOffsetMs = 30f;
        [Tooltip("How often to resync (seconds)")]
        [SerializeField] private float micSyncInterval = 2f;
        private float micSyncTimer;

        private void SetupMicAudioTrack()
        {
            var micManager = MicrophoneManager.Instance;
            if (micManager == null || micManager.MicrophoneClip == null)
            {
                Debug.LogWarning("MicrophoneManager not available, skipping mic track");
                return;
            }

            micAudioObject = new GameObject("MicAudioSender");
            micAudioObject.transform.SetParent(transform);

            micAudioSource = micAudioObject.AddComponent<AudioSource>();
            micAudioSource.clip = micManager.MicrophoneClip;
            micAudioSource.loop = true;
            micAudioSource.volume = 1.0f;
            if (captureMixerGroup != null)
                micAudioSource.outputAudioMixerGroup = captureMixerGroup;

            SyncMicPlayback();
            micAudioSource.Play();

            sendAudioTrack = new AudioStreamTrack(micAudioSource);
            _pc.AddTrack(sendAudioTrack);
            Debug.Log("Added local mic audio track");
        }

        private Stopwatch _debugSw = new Stopwatch();
        private int _frameCount = 0;
        private const float DEBUG_THRESHOLD_MS = 30f;

        private void Update()
        {
            _frameCount++;
            float frameStart = Time.realtimeSinceStartup;

            // Blit webcam frames to RT
            if (_webcamTex != null && _webcamTex.didUpdateThisFrame && sendVideoTexture != null)
            {
                _debugSw.Restart();
                Graphics.Blit(_webcamTex, sendVideoTexture);
                _debugSw.Stop();
                if (_debugSw.ElapsedMilliseconds > DEBUG_THRESHOLD_MS)
                    Debug.LogWarning($"[Perf] Frame {_frameCount}: Graphics.Blit took {_debugSw.ElapsedMilliseconds}ms");
            }

            // Mic sync
            if (micAudioSource == null || !micAudioSource.isPlaying) return;

            micSyncTimer += Time.deltaTime;
            if (micSyncTimer >= micSyncInterval)
            {
                micSyncTimer = 0f;
                _debugSw.Restart();
                SyncMicPlayback();
                _debugSw.Stop();
                if (_debugSw.ElapsedMilliseconds > DEBUG_THRESHOLD_MS)
                    Debug.LogWarning($"[Perf] Frame {_frameCount}: SyncMicPlayback took {_debugSw.ElapsedMilliseconds}ms");
            }

            float frameDelta = (Time.realtimeSinceStartup - frameStart) * 1000f;
            if (frameDelta > DEBUG_THRESHOLD_MS)
                Debug.LogWarning($"[Perf] Frame {_frameCount}: WebRTCClient.Update total {frameDelta:F1}ms");
        }

        private IEnumerator StatsCoroutine()
        {
            while (_pc != null)
            {
                yield return new WaitForSeconds(1f);
                if (_pc == null) yield break;

                var op = _pc.GetStats();
                yield return op;
                if (op.IsError) continue;

                foreach (var stat in op.Value.Stats.Values)
                {
                    if (stat is RTCOutboundRTPStreamStats outbound && outbound.kind == "video")
                    {
                        ulong delta = outbound.framesSent - _lastFramesSent;
                        _lastFramesSent = outbound.framesSent;
                        sendStats = $"{outbound.frameWidth}x{outbound.frameHeight} {delta}fps";
                    }
                    else if (stat is RTCInboundRTPStreamStats inbound && inbound.kind == "video")
                    {
                        ulong delta = inbound.framesReceived - _lastFramesReceived;
                        _lastFramesReceived = inbound.framesReceived;
                        recvStats = $"{inbound.frameWidth}x{inbound.frameHeight} {delta}fps";
                    }
                }
            }
        }

        private void SyncMicPlayback()
        {
            var micManager = MicrophoneManager.Instance;
            if (micManager == null || micAudioSource == null) return;

            int micPos = micManager.GetCurrentSamplePosition();
            int offsetSamples = (int)(micManager.sampleRate * micOffsetMs / 1000f);
            int bufferSize = micManager.sampleRate * 60;
            int targetPos = (micPos - offsetSamples + bufferSize) % bufferSize;

            micAudioSource.timeSamples = targetPos;
        }

        private void SetupVideoTrack()
        {
            // Create RT with platform-correct format
#if UNITY_ANDROID && !UNITY_EDITOR
            sendVideoTexture = new RenderTexture(videoWidth, videoHeight, 0, RenderTextureFormat.ARGB32);
#else
            sendVideoTexture = new RenderTexture(videoWidth, videoHeight, 0, RenderTextureFormat.BGRA32);
#endif
            sendVideoTexture.Create();

            // Bind source to RT
            switch (videoSource)
            {
                case VideoSourceMode.Camera:
                    if (sendCamera != null)
                    {
                        sendCamera.targetTexture = sendVideoTexture;
                        Debug.Log($"Video source: Camera '{sendCamera.name}' → RT ({videoWidth}x{videoHeight})");
                    }
                    else
                    {
                        Debug.LogWarning("videoSource=Camera but sendCamera not assigned, sending blank");
                    }
                    break;

                case VideoSourceMode.Webcam:
                    if (WebCamTexture.devices.Length == 0)
                    {
                        Debug.LogWarning("No webcam found, falling back to blank RT");
                        break;
                    }
                    if (string.IsNullOrEmpty(webcamDeviceName))
                        _webcamTex = new WebCamTexture(videoWidth, videoHeight, videoFps);
                    else
                        _webcamTex = new WebCamTexture(webcamDeviceName, videoWidth, videoHeight, videoFps);
                    _webcamTex.Play();
                    Debug.Log($"Video source: Webcam '{_webcamTex.deviceName}' → RT ({videoWidth}x{videoHeight})");
                    break;

                default:
                    Debug.Log($"Video source: None, sending blank RT ({videoWidth}x{videoHeight})");
                    break;
            }

            sendVideoTrack = new VideoStreamTrack(sendVideoTexture);
            var sender = _pc.AddTrack(sendVideoTrack);

            var parameters = sender.GetParameters();
            foreach (var encoding in parameters.encodings)
            {
                encoding.maxFramerate = (uint)videoFps;
            }
            sender.SetParameters(parameters);

            Debug.Log($"Added video track ({videoWidth}x{videoHeight}@{videoFps}fps)");
        }

        private RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            return config;
        }

        private void OnIceConnectionChange(RTCIceConnectionState state)
        {
            Debug.Log($"ICE connection state: {state}");
            if (state == RTCIceConnectionState.Disconnected ||
                state == RTCIceConnectionState.Failed ||
                state == RTCIceConnectionState.Closed)
            {
                Cleanup();
            }
        }

        private void OnConnectionStateChange(RTCPeerConnectionState state)
        {
            Debug.Log($"Connection state: {state}");
        }

        private void OnTrack(RTCTrackEvent e)
        {
            Debug.Log($"OnTrack: {e.Track.Kind}");
            if (e.Track.Kind == TrackKind.Video)
            {
                receiveVideoTrack = (VideoStreamTrack)e.Track;
                receiveVideoTrack.OnVideoReceived += OnVideoFrameReceived;
            }
            else if (e.Track.Kind == TrackKind.Audio)
            {
                receiveAudioTrack = (AudioStreamTrack)e.Track;
                receiveAudio.SetTrack(receiveAudioTrack);
                receiveAudio.loop = true;
                receiveAudio.Play();
            }
        }

        private void OnDataChannel(RTCDataChannel channel)
        {
            Debug.Log($"DataChannel received: {channel.Label}");
            if (channel.Label == "server-data")
            {
                serverDataChannel = channel;
                serverDataChannel.OnMessage = OnServerDataMessage;
            }
        }

        private void OnServerDataMessage(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            messageQueue.Enqueue(message);
        }

        private void OnVideoFrameReceived(Texture texture)
        {
            MainThreadDispatcher.ExecuteInUpdate(() =>
            {
                receiveImage.texture = texture;
                FitToScreen(texture);
            });
        }

        private void FitToScreen(Texture texture)
        {
            if (texture == null || receiveImage == null) return;

            RectTransform rt = receiveImage.rectTransform;
            RectTransform parent = rt.parent as RectTransform;
            if (parent == null) return;

            float parentW = parent.rect.width;
            float parentH = parent.rect.height;
            float videoAspect = (float)texture.width / texture.height;
            float parentAspect = parentW / parentH;

            // Fill: scale to cover entire parent area
            float w, h;
            if (videoAspect > parentAspect)
            {
                // Video is wider — match width, letterbox top/bottom
                w = parentW;
                h = parentW / videoAspect;
            }
            else
            {
                // Video is taller — match height, pillarbox left/right
                h = parentH;
                w = parentH * videoAspect;
            }

            rt.sizeDelta = new Vector2(w, h);
        }

        /// <summary>
        /// Send a JSON message through the client-signals DataChannel.
        /// Called by VADModule to send vad_start/vad_end.
        /// </summary>
        public void SendDataChannelMessage(string json)
        {
            if (clientDataChannel != null && clientDataChannel.ReadyState == RTCDataChannelState.Open)
            {
                clientDataChannel.Send(json);
            }
            else
            {
                Debug.LogWarning("client-signals DataChannel not open, cannot send: " + json);
            }
        }

        private IEnumerator SendOfferToServer(RTCSessionDescription offerDesc)
        {
            Debug.Log("Sending offer to server");
            var offerData = new OfferData
            {
                sdp = offerDesc.sdp,
                type = offerDesc.type.ToString().ToLower(),
                video_fps = videoFps,
                video_width = videoWidth,
                video_height = videoHeight
            };

            string jsonData = JsonUtility.ToJson(offerData);
            byte[] postData = Encoding.UTF8.GetBytes(jsonData);

            // Offer URL includes client_id in path
            UnityWebRequest request = new UnityWebRequest($"{serverUrl}/offer/{clientId}", "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error sending offer: {request.error}");
                yield break;
            }

            Debug.Log("Offer sent successfully");
            string responseText = request.downloadHandler.text;
            var answerData = JsonUtility.FromJson<AnswerData>(responseText);

            RTCSessionDescription answerDesc = new RTCSessionDescription
            {
                type = RTCSdpType.Answer,
                sdp = answerData.sdp
            };

            var remoteDescOp = _pc.SetRemoteDescription(ref answerDesc);
            yield return remoteDescOp;

            if (remoteDescOp.IsError)
                Debug.LogError($"Failed to set remote description: {remoteDescOp.Error.message}");
            else
                Debug.Log("Set remote description successfully");
        }

        private void Cleanup()
        {
            Debug.Log("Cleaning up WebRTC");

            if (_statsCoroutine != null)
            {
                StopCoroutine(_statsCoroutine);
                _statsCoroutine = null;
            }

            if (receiveVideoTrack != null)
            {
                receiveVideoTrack.OnVideoReceived -= OnVideoFrameReceived;
                receiveVideoTrack.Dispose();
                receiveVideoTrack = null;
            }

            if (receiveAudioTrack != null)
            {
                receiveAudio.Stop();
                receiveAudioTrack.Dispose();
                receiveAudioTrack = null;
            }

            if (sendAudioTrack != null)
            {
                sendAudioTrack.Dispose();
                sendAudioTrack = null;
            }

            if (sendVideoTrack != null)
            {
                sendVideoTrack.Dispose();
                sendVideoTrack = null;
            }

            if (_webcamTex != null)
            {
                _webcamTex.Stop();
                Destroy(_webcamTex);
                _webcamTex = null;
            }

            if (sendCamera != null && sendCamera.targetTexture == sendVideoTexture)
                sendCamera.targetTexture = null;

            if (sendVideoTexture != null)
            {
                sendVideoTexture.Release();
                Destroy(sendVideoTexture);
                sendVideoTexture = null;
            }

            if (micAudioObject != null)
            {
                Destroy(micAudioObject);
                micAudioObject = null;
                micAudioSource = null;
            }

            if (clientDataChannel != null)
            {
                clientDataChannel.Close();
                clientDataChannel.Dispose();
                clientDataChannel = null;
            }

            if (serverDataChannel != null)
            {
                serverDataChannel.Close();
                serverDataChannel.Dispose();
                serverDataChannel = null;
            }

            if (_pc != null)
            {
                _pc.Close();
                _pc.Dispose();
                _pc = null;
            }
        }

        [Serializable]
        public class OfferData
        {
            public string sdp;
            public string type;
            public int video_fps;
            public int video_width;
            public int video_height;
        }

        [Serializable]
        public class AnswerData
        {
            public string sdp;
            public string type;
        }
    }
}
