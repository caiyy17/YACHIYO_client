using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yachiyo
{
    [RequireComponent(typeof(WebRTCClient))]
    public class WebRTCClientModule : ProcessingModuleSynchronous
    {
        WebRTCClient webRTCClient;

        void Awake()
        {
            moduleName = "WebRTCClientModule";
            webRTCClient = GetComponent<WebRTCClient>();
        }

        protected override async Task CustomInit()
        {
            capturedSignals.Add("recording_start");
            capturedSignals.Add("recording_stop");
            if (webRTCClient == null)
            {
                LogError("WebRTCClient not found in scene");
                isProcessing = false;
            }
        }

        protected override void ProcessMessage(string message)
        {
            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

            // Map pipeline signals to DataChannel signals
            string dcSignal = null;
            if (baseMessage.signal == "recording_start")
                dcSignal = "vad_start";
            else if (baseMessage.signal == "recording_stop")
                dcSignal = "vad_end";

            if (dcSignal != null)
            {
                var dcMsg = new Dictionary<string, object>
                {
                    { "signal", dcSignal }
                };
                string json = JsonConvert.SerializeObject(dcMsg);
                webRTCClient.SendDataChannelMessage(json);
                LogInfo($"Sent DC: {json}");
            }
        }

        protected override void CustomUpdate()
        {
            // Read all pending messages from WebRTC server-data DataChannel
            while (webRTCClient != null && webRTCClient.messageQueue.Count > 0)
            {
                string message = webRTCClient.messageQueue.Dequeue();
                Dictionary<string, object> jsonDict =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                YYMessage baseMessage = new YYMessage();

                if (jsonDict.TryGetValue("signal", out object signal))
                    baseMessage.signal = signal.ToString();
                else
                    baseMessage.signal = "";

                baseMessage.timestamp = CustomFunctions.GetUnixTime();

                jsonDict.Remove("signal");
                jsonDict.Remove("timestamp");

                if (jsonDict.Count == 0 && baseMessage.signal == "")
                    continue;

                baseMessage.content = JsonConvert.SerializeObject(jsonDict);
                AddDestination(ref baseMessage);

                string modified_message = JsonUtility.ToJson(baseMessage);
                outputQueue.Add(modified_message);
                LogInfo($"Received DC: {modified_message}");
            }
        }

        protected override void CustomCancel(string message)
        {
            if (webRTCClient == null) return;
            var cancelMsg = new Dictionary<string, object>
            {
                { "signal", "cancel" },
                { "content", message }
            };
            string json = JsonConvert.SerializeObject(cancelMsg);
            webRTCClient.SendDataChannelMessage(json);
        }
    }
}
