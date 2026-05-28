using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yachiyo
{
    public class WebcamManager : MonoBehaviour
    {
        public static WebcamManager Instance;

        public string DeviceName => deviceName;

        [SerializeField] private int captureWidth = 640;
        [SerializeField] private int captureHeight = 480;
        [SerializeField] private int captureFps = 30;
        private string deviceName;
        private WebCamTexture webcamTexture;
        private RenderTexture blackRT;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnDestroy()
        {
            if (blackRT != null)
            {
                blackRT.Release();
                Destroy(blackRT);
                blackRT = null;
            }
        }

        // WebCamTexture stops delivering frames after scene transition.
        // Destroy stale texture on scene change so BlitTo recreates it fresh.
        private void OnSceneChanged(Scene from, Scene to)
        {
            StopCapture();
            if (!string.IsNullOrEmpty(deviceName))
                StartCapture();
        }

        private void Initialize()
        {
            // Black RT for None mode at capture resolution
            blackRT = new RenderTexture(captureWidth, captureHeight, 0);
            blackRT.Create();
            var prev = RenderTexture.active;
            RenderTexture.active = blackRT;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = prev;

            foreach (var device in WebCamTexture.devices)
            {
                Debug.Log($"Webcam: {device.name} (front={device.isFrontFacing})");
            }

            if (WebCamTexture.devices.Length > 0)
            {
                deviceName = WebCamTexture.devices[0].name;
                StartCapture();
            }
            else
            {
                Debug.LogWarning("No webcam found");
            }
        }

        /// <summary>
        /// Display names with "None" as first entry. Index matches Get/SwitchByIndex.
        /// </summary>
        public string[] GetDeviceDisplayNames()
        {
            var devices = WebCamTexture.devices;
            var names = new string[devices.Length + 1];
            names[0] = "None";
            for (int i = 0; i < devices.Length; i++)
            {
#if UNITY_ANDROID || UNITY_IOS
                string suffix = devices[i].isFrontFacing ? " (Front)" : " (Back)";
                names[i + 1] = devices[i].name + suffix;
#else
                names[i + 1] = devices[i].name;
#endif
            }
            return names;
        }

        /// <summary>
        /// Current device index in the display names list (0 = None).
        /// </summary>
        public int GetCurrentDeviceIndex()
        {
            if (string.IsNullOrEmpty(deviceName)) return 0;
            var devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].name == deviceName) return i + 1;
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
                SwitchDevice(null);
            }
            else
            {
                var devices = WebCamTexture.devices;
                int deviceIdx = index - 1;
                if (deviceIdx < devices.Length)
                    SwitchDevice(devices[deviceIdx].name);
                else
                    SwitchDevice(null);
            }
        }

        private void StartCapture()
        {
            webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, captureFps);
            webcamTexture.Play();
            Debug.Log($"Webcam started: \"{deviceName}\" ({captureWidth}x{captureHeight}@{captureFps}fps)");
        }

        public void StopCapture()
        {
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                Destroy(webcamTexture);
                webcamTexture = null;
                Debug.Log("Webcam stopped");
            }
        }

        private void SwitchDevice(string newDeviceName)
        {
            StopCapture();
            if (string.IsNullOrEmpty(newDeviceName))
            {
                deviceName = null;
                Debug.Log("Webcam disabled (None)");
                return;
            }
            deviceName = newDeviceName;
            StartCapture();
        }

        /// <summary>
        /// Blit current webcam frame to target RT. Blits internal black RT if no webcam active.
        /// </summary>
        public void BlitTo(RenderTexture target)
        {
            if (target == null) return;

            if (webcamTexture != null)
                Graphics.Blit(webcamTexture, target);
            else
                Graphics.Blit(blackRT, target);
        }
    }
}
