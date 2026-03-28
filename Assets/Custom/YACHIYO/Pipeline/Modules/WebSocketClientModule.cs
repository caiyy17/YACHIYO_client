using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace Yachiyo
{
    [RequireComponent(typeof(WebSocketClient))]
    public class WebSocketClientModule : ProcessingModuleSynchronous
    {
        WebSocketClient webSocketClient;
        void Awake()
        {
            moduleName = "WebSocketClientModule";
            webSocketClient = GetComponent<WebSocketClient>();
        }

        protected override async Task CustomInit()
        {
            await webSocketClient.Connect();
            await webSocketClient.StartProcessing();
            if (!webSocketClient.IsConnected)
            {
                isProcessing = false;
            }
        }

        protected override void ProcessMessage(string message)
        {
            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
            Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
            if (!jsonDict.TryGetValue("signal", out object signal))
            {
                jsonDict["signal"] = baseMessage.signal;
            }
            if (!jsonDict.TryGetValue("timestamp", out object timestamp))
            {
                jsonDict["timestamp"] = baseMessage.timestamp;
            }
            string modified_message = JsonConvert.SerializeObject(jsonDict);
            webSocketClient.SendMessageToServer(modified_message);
            LogInfo($"Sent: {modified_message}");
        }
        protected override void CustomUpdate()
        {
            string message;
            if (webSocketClient.TryGetReceivedMessage(out message))
            {
                YYMessage baseMessage = new YYMessage();
                Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                if (jsonDict.TryGetValue("signal", out object signal))
                {
                    baseMessage.signal = signal.ToString();
                }
                else
                {
                    baseMessage.signal = "";
                }

                if (jsonDict.TryGetValue("timestamp", out object timestamp))
                {
                    baseMessage.timestamp = (double)timestamp;
                }
                else
                {
                    baseMessage.timestamp = CustomFunctions.GetUnixTime();
                }

                if (jsonDict.TryGetValue("destination_client", out object destination_client))
                {
                    AddDestination(ref baseMessage, System.Convert.ToInt32(destination_client));
                }
                else
                {
                    AddDestination(ref baseMessage);
                }

                jsonDict.Remove("signal");
                jsonDict.Remove("timestamp");
                jsonDict.Remove("destination_client");

                if (jsonDict.Count == 0 && baseMessage.signal == "")
                    return;

                baseMessage.content = JsonConvert.SerializeObject(jsonDict);

                string modified_message = JsonUtility.ToJson(baseMessage);
                outputQueue.Add(modified_message);
                LogInfo($"Received: {modified_message}");
            }
        }
        protected override void CustomCancel(string message)
        {
            YYMessage data = new YYMessage
            {
                signal = "cancel",
                content = message,
                timestamp = cancel_timestamp
            };
            string cancel_message = JsonUtility.ToJson(data);
            webSocketClient.SendMessageToServer(cancel_message);
        }
    }
}
