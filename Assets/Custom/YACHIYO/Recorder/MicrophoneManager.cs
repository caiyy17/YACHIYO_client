using UnityEngine;

namespace Yachiyo
{
    public class MicrophoneManager : MonoBehaviour
    {
        public static MicrophoneManager Instance;
        public int sampleRate { get; private set; } = 16000;
        public int bufferSize { get; private set; } = 1024;
        private AudioClip microphoneClip;
        public AudioClip MicrophoneClip => microphoneClip;
        public string DeviceName => microphone;
        private int bufferLength = 60;
        private float[] audioBuffer;
        private int bufferPosition = 0;
        private bool isRecording = false;
        private string microphone;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMicrophone();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Display names with "None" as first entry. Index matches Get/SwitchByIndex.
        /// </summary>
        public string[] GetDeviceDisplayNames()
        {
            var devices = Microphone.devices;
            var names = new string[devices.Length + 1];
            names[0] = "None";
            for (int i = 0; i < devices.Length; i++)
                names[i + 1] = devices[i];
            return names;
        }

        /// <summary>
        /// Current device index in the display names list (0 = None).
        /// </summary>
        public int GetCurrentDeviceIndex()
        {
            if (string.IsNullOrEmpty(microphone)) return 0;
            var devices = Microphone.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i] == microphone) return i + 1;
            }
            return 0;
        }

        /// <summary>
        /// Switch device by display list index (0 = None, 1+ = device).
        /// </summary>
        public void SwitchByIndex(int index)
        {
            if (index == GetCurrentDeviceIndex()) return;
            if (index <= 0)
            {
                SwitchMicrophone(null);
            }
            else
            {
                var devices = Microphone.devices;
                int deviceIdx = index - 1;
                if (deviceIdx < devices.Length)
                    SwitchMicrophone(devices[deviceIdx]);
                else
                    SwitchMicrophone(null);
            }
        }

        private void SwitchMicrophone(string deviceName)
        {
            // stop current recording
            if (isRecording)
            {
                Microphone.End(microphone);
                isRecording = false;
            }

            if (string.IsNullOrEmpty(deviceName))
            {
                microphone = null;
                microphoneClip = AudioClip.Create("silence", sampleRate * bufferLength, 1, sampleRate, false);
                bufferSize = sampleRate * bufferLength;
                audioBuffer = new float[bufferSize];
                bufferPosition = 0;
                Debug.Log("Microphone disabled (None)");
                return;
            }

            microphone = deviceName;
            StartRecording();
        }

        private void InitializeMicrophone()
        {
            foreach (var device in Microphone.devices)
            {
                Debug.Log("Microphone name: " + device);
            }
            if (Microphone.devices.Length > 0)
            {
                microphone = Microphone.devices[0];
                StartRecording();
            }
            else
            {
                Debug.LogWarning("No microphone found");
                microphoneClip = AudioClip.Create("silence", sampleRate * bufferLength, 1, sampleRate, false);
                bufferSize = sampleRate * bufferLength;
                audioBuffer = new float[bufferSize];
            }
            AudioClip empty = WavUtility.emptyClip;
        }

        private void StartRecording()
        {
            bufferSize = sampleRate * bufferLength;
            audioBuffer = new float[bufferSize];
            bufferPosition = 0;
            microphoneClip = Microphone.Start(microphone, true, bufferLength, sampleRate);
            isRecording = true;
            Debug.Log($"Recording started: \"{microphone}\" @ {sampleRate}Hz");
        }

        private void Update()
        {
            if (isRecording)
            {
                bufferPosition = Microphone.GetPosition(microphone);
            }
            else
            {
                bufferPosition = (bufferPosition + (int)(Time.deltaTime * sampleRate)) % bufferSize;
            }
        }

        public float[] GetAudioData(int startSample, int endSample)
        {
            // make startSample and endSample in the range of audioBuffer
            startSample = (startSample % audioBuffer.Length + audioBuffer.Length) % audioBuffer.Length;
            endSample = (endSample % audioBuffer.Length + audioBuffer.Length) % audioBuffer.Length;

            if (microphoneClip != null)
                microphoneClip.GetData(audioBuffer, 0);
            int length;
            if (startSample <= endSample)
            {
                length = endSample - startSample;
            }
            else
            {
                length = audioBuffer.Length - startSample + endSample;
            }

            float[] data = new float[length];
            if (startSample < endSample)
            {
                System.Array.Copy(audioBuffer, startSample, data, 0, length);
            }
            else
            {
                // 处理循环缓冲区的情况
                int firstPartLength = audioBuffer.Length - startSample;
                System.Array.Copy(audioBuffer, startSample, data, 0, firstPartLength);
                System.Array.Copy(audioBuffer, 0, data, firstPartLength, endSample);
            }
            return data;
        }

        public float[] GetAudioDataLength(int endSample, int length)
        {
            int startSample = endSample - length;
            return GetAudioData(startSample, endSample);
        }

        public int GetCurrentSamplePosition(float offset = 0)
        {
            int position = bufferPosition + (int)(offset * sampleRate);
            position = (position % audioBuffer.Length + audioBuffer.Length) % audioBuffer.Length;
            return position;
        }

        public float GetCurrentLoudness(float timeWindow = 0.5f)
        {
            int startPosition = bufferPosition - (int)(sampleRate * timeWindow);
            int endPosition = bufferPosition;
            float[] audioData = GetAudioData(startPosition, endPosition);

            float sum = 0;
            for (int i = 0; i < audioData.Length; i++)
            {
                sum += audioData[i] * audioData[i];
            }

            if (audioData.Length == 0) return 0f;
            float rmsValue = Mathf.Sqrt(sum / audioData.Length);
            return rmsValue;
        }
    }
}
