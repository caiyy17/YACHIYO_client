using System.Collections.Generic;
using UnityEngine;
using Klak.Ndi;

namespace Yachiyo
{
    /// <summary>
    /// Creates an NDI send stream for each camera in the list.
    /// Uses RenderTexture + CaptureMethod.Texture to include post-processing in NDI output.
    /// Each stream is named "{prefix}_{CameraName}" and visible in OBS as a separate NDI source.
    /// </summary>
    public class NdiSendManager : MonoBehaviour
    {
        [Header("NDI Configuration")]
        [Tooltip("Prefix for NDI stream names. Each stream: '{prefix}_{CameraName}'.")]
        [SerializeField] private string ndiNamePrefix = "YACHIYO";

        [Tooltip("Include alpha channel in NDI output.")]
        [SerializeField] private bool keepAlpha = false;

        [Header("Cameras")]
        [Tooltip("Each camera becomes a separate NDI source.")]
        [SerializeField] private List<Camera> cameras = new List<Camera>();

        [Header("Resolution")]
        [Tooltip("Width of the NDI output. 0 = use camera's current pixel width.")]
        [SerializeField] private int outputWidth = 1920;
        [Tooltip("Height of the NDI output. 0 = use camera's current pixel height.")]
        [SerializeField] private int outputHeight = 1080;

        [Header("Resources")]
        [Tooltip("Drag NdiResources.asset from Packages/KlakNDI/Runtime/Resource/ here.")]
        [SerializeField] private NdiResources ndiResources;

        private readonly List<NdiSender> _senders = new List<NdiSender>();
        private readonly List<RenderTexture> _renderTextures = new List<RenderTexture>();

        void Start()
        {
            if (ndiResources == null)
            {
                Debug.LogError("[NdiSendManager] NdiResources not assigned.");
                return;
            }

            foreach (var cam in cameras)
            {
                if (cam == null) continue;
                if (cam.GetComponent<NdiSender>() != null)
                {
                    Debug.LogWarning($"[NdiSendManager] '{cam.name}' already has NdiSender, skipping.");
                    continue;
                }

                // Determine resolution
                int w = outputWidth > 0 ? outputWidth : cam.pixelWidth;
                int h = outputHeight > 0 ? outputHeight : cam.pixelHeight;

                // Create RenderTexture for this camera (URP renders post-processing into targetTexture)
                var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
                rt.name = $"NDI_RT_{cam.gameObject.name}";
                rt.Create();
                _renderTextures.Add(rt);

                // Assign RT to camera so URP renders the full pipeline (including post-processing) into it
                cam.targetTexture = rt;

                // Disable camera temporarily so NdiSender.OnEnable doesn't fire before configuration
                cam.gameObject.SetActive(false);

                // Use CaptureMethod.Texture to read from the RT (which already contains post-processing)
                var sender = cam.gameObject.AddComponent<NdiSender>();
                sender.SetResources(ndiResources);
                sender.captureMethod = CaptureMethod.Texture;
                sender.sourceTexture = rt;
                sender.ndiName = $"{ndiNamePrefix}_{cam.gameObject.name}";
                sender.keepAlpha = keepAlpha;

                // Re-enable so NdiSender.OnEnable initializes with correct settings
                cam.gameObject.SetActive(true);

                _senders.Add(sender);
                Debug.Log($"[NdiSendManager] NDI sender '{sender.ndiName}' created (RT: {w}x{h}).");
            }
        }

        void OnDestroy()
        {
            foreach (var sender in _senders)
            {
                if (sender != null) Destroy(sender);
            }
            _senders.Clear();

            foreach (var rt in _renderTextures)
            {
                if (rt != null)
                {
                    rt.Release();
                    Destroy(rt);
                }
            }
            _renderTextures.Clear();
        }
    }
}
