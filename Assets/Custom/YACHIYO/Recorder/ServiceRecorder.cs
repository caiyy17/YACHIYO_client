using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

namespace Yachiyo
{
    public class ServiceRecorder : MonoBehaviour
    {
        private int startSample;
        private int endSample;

        public string serviceName = "default";

        public int sampleRate;
        public float maxRecordingTime = 60f;
        private float[] audioData = null;
        [HideInInspector]
        public byte[] wavData = null;

        public bool isRecording = false;
        public bool isDataReady = true;

        private void Start()
        {
            sampleRate = MicrophoneManager.Instance.sampleRate;
        }

        public void StartRecording(float offset = 0)
        {
            startSample = MicrophoneManager.Instance.GetCurrentSamplePosition(offset);
            isRecording = true;
        }

        public void StopRecording(float offset = 0)
        {
            endSample = MicrophoneManager.Instance.GetCurrentSamplePosition(offset);
            isRecording = false;
            isDataReady = false;
            ProcessAudioData();
        }

        void ProcessAudioData()
        {
            audioData = MicrophoneManager.Instance.GetAudioData(startSample, endSample);
            // 对音频数据进行处理，例如保存或发送
            Thread saveThread = new Thread(() =>
            {
                WavUtility.WavFormat wav = new WavUtility.WavFormat
                {
                    channels = 1,
                    frequency = sampleRate,
                    samples = audioData,
                    samplesCount = audioData.Length
                };
                // wav = WavUtility.TrimSilence(wav, 0.0001f);
                wavData = WavUtility.ConvertToWav(wav.samples, wav.samplesCount, wav.channels, wav.frequency);
                MainThreadDispatcher.ExecuteInUpdate(() => OnSaveComplete());
            });
            saveThread.Start();
        }

        private void OnSaveComplete()
        {
            // 这里是音频保存完成后的处理，将在主线程上执行
            isDataReady = true;
            Debug.Log("Recording saved");
        }
    }
}
