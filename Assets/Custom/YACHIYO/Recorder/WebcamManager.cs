using UnityEngine;

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

        private void Initialize()
        {
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
        /// Get the list of available webcam devices.
        /// </summary>
        public WebCamDevice[] GetAvailableDevices()
        {
            return WebCamTexture.devices;
        }

        /// <summary>
        /// Get display names with "(Front)"/"(Back)" suffix on mobile.
        /// </summary>
        public string[] GetDeviceDisplayNames()
        {
            var devices = WebCamTexture.devices;
            var names = new string[devices.Length];
            for (int i = 0; i < devices.Length; i++)
            {
#if UNITY_ANDROID || UNITY_IOS
                string suffix = devices[i].isFrontFacing ? " (Front)" : " (Back)";
                names[i] = devices[i].name + suffix;
#else
                names[i] = devices[i].name;
#endif
            }
            return names;
        }

        /// <summary>
        /// Create and start webcam capture. Safe to call multiple times.
        /// </summary>
        private void StartCapture()
        {
            webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, captureFps);
            webcamTexture.Play();
            Debug.Log($"Webcam started: \"{deviceName}\" ({captureWidth}x{captureHeight}@{captureFps}fps)");
        }

        /// <summary>
        /// Stop and destroy the webcam texture.
        /// </summary>
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

        /// <summary>
        /// Switch to a different webcam device. Restarts capture if currently playing.
        /// </summary>
        public void SwitchDevice(string newDeviceName)
        {
            StopCapture();
            deviceName = newDeviceName;
            StartCapture();
        }

        /// <summary>
        /// Blit current webcam frame to target RT. No-op if webcam not ready or no new frame.
        /// </summary>
        public void BlitTo(RenderTexture target)
        {
            if (webcamTexture != null && webcamTexture.didUpdateThisFrame && target != null)
            {
                Graphics.Blit(webcamTexture, target);
            }
        }

    }
}
