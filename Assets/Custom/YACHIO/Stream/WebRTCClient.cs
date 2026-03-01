using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.WebRTC;

public class WebRTCClient : MonoBehaviour
{
    public string serverUrl = "localhost:18082"; // WebRTC signaling server
    public string mainServerUrl = "localhost:8000"; // Main REST API server
    public string clientId = "unity-client";
    public string pipelineConfig = "default";

    [SerializeField] private RawImage receiveImage;
    [SerializeField] private AudioSource receiveAudio;

    private RTCPeerConnection _pc;

    // Receive tracks
    private VideoStreamTrack receiveVideoTrack;
    private AudioStreamTrack receiveAudioTrack;

    // Send tracks
    private GameObject micAudioObject;
    private AudioSource micAudioSource;
    private AudioStreamTrack sendAudioTrack;
    private RenderTexture sendVideoTexture;
    private VideoStreamTrack sendVideoTrack;

    // Data channels
    private RTCDataChannel clientDataChannel; // "client-signals" — created by us
    private RTCDataChannel serverDataChannel; // "server-data" — created by server

    // Single message queue for all server-data DC messages
    public Queue<string> messageQueue = new Queue<string>();

    private void Awake()
    {
        serverUrl = PlayerPrefs.GetString("webrtcUrlInput", serverUrl);
        clientId = PlayerPrefs.GetString("userId", clientId);
        mainServerUrl = PlayerPrefs.GetString("urlInput", mainServerUrl);
        pipelineConfig = PlayerPrefs.GetString("pipelineConfig", pipelineConfig);

        serverUrl = $"http://{serverUrl}";
        mainServerUrl = $"http://{mainServerUrl}";
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Start()
    {
        StartCoroutine(ConnectFlow());
        StartCoroutine(WebRTC.Update());
    }

    /// <summary>
    /// Full connection flow: register → init pipeline → WebRTC signaling
    /// </summary>
    private IEnumerator ConnectFlow()
    {
        yield return StartCoroutine(Register());
        yield return StartCoroutine(InitPipeline());
        yield return StartCoroutine(Call());
    }

    private IEnumerator Register()
    {
        Debug.Log("Registering client...");
        string json = $"{{\"client_id\":\"{clientId}\"}}";
        byte[] postData = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest($"{mainServerUrl}/register/", "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"Registration failed: {request.error}");
        else
            Debug.Log($"Registration successful: {request.downloadHandler.text}");
    }

    private IEnumerator InitPipeline()
    {
        Debug.Log("Initializing pipeline...");
        string json = $"{{\"config\":\"{pipelineConfig}\"}}";
        byte[] postData = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest($"{mainServerUrl}/init_pipeline/{clientId}", "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"Init pipeline failed: {request.error}");
        else
            Debug.Log($"Init pipeline successful: {request.downloadHandler.text}");
    }

    private IEnumerator Call()
    {
        RTCConfiguration config = GetSelectedSdpSemantics();
        _pc = new RTCPeerConnection(ref config);
        Debug.Log("Created peer connection object");

        _pc.OnIceConnectionChange = OnIceConnectionChange;
        _pc.OnConnectionStateChange = OnConnectionStateChange;
        _pc.OnTrack = OnTrack;
        _pc.OnDataChannel = OnDataChannel;

        // Add local mic audio track → transceiver becomes SendRecv
        SetupMicAudioTrack();

        // Add local placeholder video track → transceiver becomes SendRecv
        SetupPlaceholderVideoTrack();

        // Create "client-signals" DataChannel for VAD signals
        RTCDataChannelInit dcConf = new RTCDataChannelInit { ordered = true };
        clientDataChannel = _pc.CreateDataChannel("client-signals", dcConf);
        clientDataChannel.OnOpen = () => Debug.Log("client-signals DataChannel opened");
        clientDataChannel.OnClose = () => Debug.Log("client-signals DataChannel closed");

        // Create offer
        var op = _pc.CreateOffer();
        yield return op;

        if (op.IsError)
        {
            Debug.LogError($"Failed to create offer: {op.Error.message}");
            yield break;
        }

        Debug.Log("Created offer successfully");
        RTCSessionDescription desc = op.Desc;
        var localDescOp = _pc.SetLocalDescription(ref desc);
        yield return localDescOp;

        if (localDescOp.IsError)
        {
            Debug.LogError($"Failed to set local description: {localDescOp.Error.message}");
            yield break;
        }

        Debug.Log("Set local description successfully");
        yield return StartCoroutine(SendOfferToServer(op.Desc));
    }

    private void SetupMicAudioTrack()
    {
        var micManager = MicrophoneManager.Instance;
        if (micManager == null || micManager.MicrophoneClip == null)
        {
            Debug.LogWarning("MicrophoneManager not available, skipping mic track");
            return;
        }

        // Create child GameObject to avoid AudioCustomFilter conflict with receiveAudio
        micAudioObject = new GameObject("MicAudioSender");
        micAudioObject.transform.SetParent(transform);
        micAudioSource = micAudioObject.AddComponent<AudioSource>();
        micAudioSource.clip = micManager.MicrophoneClip;
        micAudioSource.loop = true;
        micAudioSource.volume = 0f;
        micAudioSource.Play();

        sendAudioTrack = new AudioStreamTrack(micAudioSource);
        _pc.AddTrack(sendAudioTrack);
        Debug.Log("Added local mic audio track");
    }

    private void SetupPlaceholderVideoTrack()
    {
        sendVideoTexture = new RenderTexture(320, 240, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_SRGB);
        sendVideoTexture.Create();

        sendVideoTrack = new VideoStreamTrack(sendVideoTexture);
        _pc.AddTrack(sendVideoTrack);
        Debug.Log("Added local placeholder video track");
    }

    private RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
        return config;
    }

    private void OnIceConnectionChange(RTCIceConnectionState state)
    {
        Debug.Log($"ICE connection state: {state}");
        if (state == RTCIceConnectionState.Disconnected ||
            state == RTCIceConnectionState.Failed ||
            state == RTCIceConnectionState.Closed)
        {
            Cleanup();
        }
    }

    private void OnConnectionStateChange(RTCPeerConnectionState state)
    {
        Debug.Log($"Connection state: {state}");
    }

    private void OnTrack(RTCTrackEvent e)
    {
        Debug.Log($"OnTrack: {e.Track.Kind}");
        if (e.Track.Kind == TrackKind.Video)
        {
            receiveVideoTrack = (VideoStreamTrack)e.Track;
            receiveVideoTrack.OnVideoReceived += OnVideoFrameReceived;
        }
        else if (e.Track.Kind == TrackKind.Audio)
        {
            receiveAudioTrack = (AudioStreamTrack)e.Track;
            receiveAudio.SetTrack(receiveAudioTrack);
            receiveAudio.loop = true;
            receiveAudio.Play();
        }
    }

    private void OnDataChannel(RTCDataChannel channel)
    {
        Debug.Log($"DataChannel received: {channel.Label}");
        if (channel.Label == "server-data")
        {
            serverDataChannel = channel;
            serverDataChannel.OnMessage = OnServerDataMessage;
        }
    }

    private void OnServerDataMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        messageQueue.Enqueue(message);
    }

    private void OnVideoFrameReceived(Texture texture)
    {
        MainThreadDispatcher.ExecuteInUpdate(() =>
        {
            receiveImage.texture = texture;
        });
    }

    /// <summary>
    /// Send a JSON message through the client-signals DataChannel.
    /// Called by VADModule to send vad_start/vad_end.
    /// </summary>
    public void SendDataChannelMessage(string json)
    {
        if (clientDataChannel != null && clientDataChannel.ReadyState == RTCDataChannelState.Open)
        {
            clientDataChannel.Send(json);
        }
        else
        {
            Debug.LogWarning("client-signals DataChannel not open, cannot send: " + json);
        }
    }

    private IEnumerator SendOfferToServer(RTCSessionDescription offerDesc)
    {
        Debug.Log("Sending offer to server");
        var offerData = new OfferData
        {
            sdp = offerDesc.sdp,
            type = offerDesc.type.ToString().ToLower()
        };

        string jsonData = JsonUtility.ToJson(offerData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // Offer URL includes client_id in path
        UnityWebRequest request = new UnityWebRequest($"{serverUrl}/offer/{clientId}", "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error sending offer: {request.error}");
            yield break;
        }

        Debug.Log("Offer sent successfully");
        string responseText = request.downloadHandler.text;
        var answerData = JsonUtility.FromJson<AnswerData>(responseText);

        RTCSessionDescription answerDesc = new RTCSessionDescription
        {
            type = RTCSdpType.Answer,
            sdp = answerData.sdp
        };

        var remoteDescOp = _pc.SetRemoteDescription(ref answerDesc);
        yield return remoteDescOp;

        if (remoteDescOp.IsError)
            Debug.LogError($"Failed to set remote description: {remoteDescOp.Error.message}");
        else
            Debug.Log("Set remote description successfully");
    }

    private void Cleanup()
    {
        Debug.Log("Cleaning up WebRTC");

        if (receiveVideoTrack != null)
        {
            receiveVideoTrack.OnVideoReceived -= OnVideoFrameReceived;
            receiveVideoTrack.Dispose();
            receiveVideoTrack = null;
        }

        if (receiveAudioTrack != null)
        {
            receiveAudio.Stop();
            receiveAudioTrack.Dispose();
            receiveAudioTrack = null;
        }

        if (sendAudioTrack != null)
        {
            sendAudioTrack.Dispose();
            sendAudioTrack = null;
        }

        if (sendVideoTrack != null)
        {
            sendVideoTrack.Dispose();
            sendVideoTrack = null;
        }

        if (sendVideoTexture != null)
        {
            sendVideoTexture.Release();
            Destroy(sendVideoTexture);
            sendVideoTexture = null;
        }

        if (micAudioObject != null)
        {
            Destroy(micAudioObject);
            micAudioObject = null;
            micAudioSource = null;
        }

        if (clientDataChannel != null)
        {
            clientDataChannel.Close();
            clientDataChannel.Dispose();
            clientDataChannel = null;
        }

        if (serverDataChannel != null)
        {
            serverDataChannel.Close();
            serverDataChannel.Dispose();
            serverDataChannel = null;
        }

        if (_pc != null)
        {
            _pc.Close();
            _pc.Dispose();
            _pc = null;
        }
    }

    [Serializable]
    public class OfferData
    {
        public string sdp;
        public string type;
    }

    [Serializable]
    public class AnswerData
    {
        public string sdp;
        public string type;
    }
}
