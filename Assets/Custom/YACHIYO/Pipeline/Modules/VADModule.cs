using System;
using UnityEngine;
using System.Threading.Tasks;

namespace Yachiyo
{
    [RequireComponent(typeof(VoiceDetector))]
    public class VADModule : ProcessingModuleSynchronous
    {
        VoiceDetector voiceDetector;
        public bool useVAD = false;
        public bool previousSpeaking = false;
        double start_timestamp = 0.0f;
        void Awake()
        {
            moduleName = "VADModule";
            voiceDetector = GetComponent<VoiceDetector>();
        }

        protected override async Task CustomInit()
        {
            previousSpeaking = false;
            useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
            voiceDetector.SetVAD(useVAD);
        }

        protected override void CustomUpdate()
        {
            if (voiceDetector.isSpeaking && !previousSpeaking)
            {
                previousSpeaking = true;
                LogInfo("Recording start");
                double ts = CustomFunctions.GetUnixTime();
                current_timestamp = ts;
                start_timestamp = ts;

                // cancel: interrupt line, timestamp slightly before recording to not cancel self
                YYMessage cancelMessage = new YYMessage();
                cancelMessage.signal = "cancel";
                cancelMessage.timestamp = ts;
                sendQueue.Add(JsonUtility.ToJson(cancelMessage));

                // recording_start to next module
                YYMessage pipelineMessage = new YYMessage();
                pipelineMessage.signal = "recording_start";
                pipelineMessage.timestamp = ts;
                AddDestination(ref pipelineMessage);
                outputQueue.Add(JsonUtility.ToJson(pipelineMessage));

                // recording_start to tail (skip all modules)
                YYMessage tailMessage = new YYMessage();
                tailMessage.signal = "recording_start";
                tailMessage.timestamp = ts;
                tailMessage.destination = -1;
                outputQueue.Add(JsonUtility.ToJson(tailMessage));

                current_timestamp = null;
            }
            else if (!voiceDetector.isSpeaking && previousSpeaking)
            {
                previousSpeaking = false;
                if (start_timestamp < cancel_timestamp)
                {
                    LogInfo("Recording cancel");
                    return;
                }
                LogInfo("Recording stop");
                double ts = CustomFunctions.GetUnixTime();
                current_timestamp = ts;

                // recording_stop to next module
                YYMessage pipelineMessage = new YYMessage();
                pipelineMessage.signal = "recording_stop";
                pipelineMessage.timestamp = ts;
                AddDestination(ref pipelineMessage);
                outputQueue.Add(JsonUtility.ToJson(pipelineMessage));

                // recording_stop to tail (skip all modules)
                YYMessage tailMessage = new YYMessage();
                tailMessage.signal = "recording_stop";
                tailMessage.timestamp = ts;
                tailMessage.destination = -1;
                outputQueue.Add(JsonUtility.ToJson(tailMessage));

                current_timestamp = null;
            }
        }
    }
}
