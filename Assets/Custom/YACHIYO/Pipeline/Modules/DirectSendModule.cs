using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

namespace Yachiyo
{
    public class DirectSendModule : ProcessingModuleSynchronous
    {
        [System.Serializable]
        public class SendButton
        {
            public InputAction button;
            public string name;
            [TextArea(1, 10)]
            public string message;
            public int destination_remote;
        }

        public List<SendButton> sendButtons = new List<SendButton>();
        void Awake()
        {
            moduleName = "DirectSendModule";
        }

        protected override async Task CustomInit()
        {
            foreach (SendButton sendButton in sendButtons)
            {
                sendButton.button.Enable();
            }
        }

        protected override void CustomStop()
        {
            foreach (SendButton sendButton in sendButtons)
            {
                sendButton.button.Disable();
            }
        }

        protected override void CustomUpdate()
        {
            foreach (SendButton sendButton in sendButtons)
            {
                if (sendButton.button.WasPerformedThisFrame())
                {
                    LogInfo("buttom pressed: " + sendButton.name);
                    string message;
                    YYMessage baseMessage;
                    double ts = CustomFunctions.GetUnixTime();
                    current_timestamp = ts;

                    // cancel previous interaction
                    LogInfo("cancel");
                    baseMessage = new YYMessage();
                    baseMessage.signal = "cancel";
                    baseMessage.timestamp = ts;
                    sendQueue.Add(JsonUtility.ToJson(baseMessage));

                    // recording_start to tail (skip all modules, no actual recording needed)
                    LogInfo("recording_start");
                    baseMessage = new YYMessage();
                    baseMessage.signal = "recording_start";
                    baseMessage.timestamp = ts;
                    baseMessage.destination = -1;
                    outputQueue.Add(JsonUtility.ToJson(baseMessage));

                    // recording_stop to tail
                    LogInfo("recording_stop");
                    baseMessage = new YYMessage();
                    baseMessage.signal = "recording_stop";
                    baseMessage.timestamp = ts;
                    baseMessage.destination = -1;
                    outputQueue.Add(JsonUtility.ToJson(baseMessage));

                    // send string
                    ts = CustomFunctions.GetUnixTime();
                    baseMessage = new YYMessage();
                    baseMessage.signal = "";
                    baseMessage.timestamp = ts;

                    string messageContent = sendButton.message;
                    Dictionary<string, object> messageDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageContent);
                    messageDict["destination"] = sendButton.destination_remote;
                    baseMessage.content = JsonConvert.SerializeObject(messageDict);

                    AddDestination(ref baseMessage);
                    message = JsonUtility.ToJson(baseMessage);
                    outputQueue.Add(message);
                    LogInfo("send: " + message);
                    current_timestamp = null;
                    break;
                }
            }
        }
    }
}
