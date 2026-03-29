using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Yachiyo
{
    /// <summary>
    /// Pipeline module that decodes and plays audio clips sequentially.
    /// Uses the inputQueue as a natural buffer: while audio is playing,
    /// no new messages are dequeued, so EoS is only forwarded after all audio finishes.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioModule : ProcessingModule
    {
        AudioSource audioSource;
        bool isPlayingAudio = false;

        void Awake()
        {
            moduleName = "AudioModule";
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!isProcessing) return;

            CheckCancel();

            // Wait for current audio to finish
            if (isPlayingAudio)
            {
                if (!audioSource.isPlaying)
                {
                    isPlayingAudio = false;
                }
                return;
            }

            // Try to take next message from input queue
            string message;
            if (!inputQueue.TryTake(out message)) return;

            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
            current_timestamp = baseMessage.timestamp;

            // Discard messages older than cancel
            if (current_timestamp < cancel_timestamp)
            {
                LogInfo($"discarding old data: {message}, cancel: {cancel_timestamp}, current: {current_timestamp}");
                current_timestamp = null;
                return;
            }

            // Forward non-targeted messages
            if (baseMessage.destination != -2 && baseMessage.destination != index)
            {
                outputQueue.Add(message);
                current_timestamp = null;
                return;
            }

            string signal = baseMessage.signal;

            // Forward unrecognized signals (same as ProcessingModuleSynchronous)
            if (signal != "" && signal != "SoS" && signal != "EoS")
            {
                baseMessage.destination = -2;
                outputQueue.Add(JsonUtility.ToJson(baseMessage));
                current_timestamp = null;
                return;
            }

            if (signal == "SoS" || signal == "EoS")
            {
                // Forward control signals as-is
                outputQueue.Add(message);
                current_timestamp = null;
                return;
            }

            // Audio message (signal == "")
            Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);

            // Decode and play audio
            string audioBase64 = jsonDict.ContainsKey("audio_data") ? jsonDict["audio_data"].ToString() : "";
            if (!string.IsNullOrEmpty(audioBase64))
            {
                AudioClip clip = WavUtility.ToAudioClip(System.Convert.FromBase64String(audioBase64));
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    isPlayingAudio = true;
                }
            }

            // Forward remaining fields to downstream (without audio_data)
            jsonDict.Remove("audio_data");
            if (jsonDict.Count > 0)
            {
                YYMessage textMessage = new YYMessage
                {
                    signal = "",
                    content = JsonConvert.SerializeObject(jsonDict),
                    destination = -2,
                    timestamp = baseMessage.timestamp
                };
                outputQueue.Add(JsonUtility.ToJson(textMessage));
            }

            current_timestamp = null;
        }

        protected override void CustomCancel(string message)
        {
            audioSource.Stop();
            isPlayingAudio = false;
        }
    }
}
