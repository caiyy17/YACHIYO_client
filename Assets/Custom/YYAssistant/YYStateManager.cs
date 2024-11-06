using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ServiceRecorder))]
[RequireComponent(typeof(VoiceDetector))]
[RequireComponent(typeof(WebSocketClient))]
[RequireComponent(typeof(SignalManager))]
public class YYStateManager : MonoBehaviour
{
    public TextMeshProUGUI debugger;
    [HideInInspector]
    public ServiceRecorder recordService;
    [HideInInspector]
    public VoiceDetector voiceDetector;
    [HideInInspector]
    public WebSocketClient webSocketClient;
    [HideInInspector]
    public SignalManager signalManager;
    public bool useVAD = true;

    [SerializeField] public InputAction stopButton;

    public IAssistantState CurrentState { get; private set; }
    public readonly IAssistantState IdleState = new IdleState();
    public readonly IAssistantState RecordingState = new RecordingState();
    public readonly IAssistantState AnsweringState = new AnsweringState();

    private bool isStarted = false;

    void Awake()
    {
        isStarted = false;
    }
    async void Start()
    {
        recordService = GetComponent<ServiceRecorder>();
        voiceDetector = GetComponent<VoiceDetector>();
        webSocketClient = GetComponent<WebSocketClient>();
        signalManager = GetComponent<SignalManager>();
        signalManager.SendSignal("yya_start", "Connecting to server...");
        await webSocketClient.Connect();
        await webSocketClient.StartProcessing();
        Init();
    }

    void OnDisable(){
        stopButton.Disable();
    }

    void Init()
    {
        if(webSocketClient.IsConnected){
            useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
            voiceDetector.SetVAD(useVAD);
            CurrentState = IdleState;
            CurrentState.EnterState(this);
            stopButton.Enable();
            isStarted = true;
            signalManager.SendSignal("yya_start", "started");
            Debug.Log("YYStateManager is started");
        }
        else{
            signalManager.SendSignal("yya_start", "Error in start");
            Debug.LogError("WebSocketClient is not connected, please check the connection");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isStarted){
            CurrentState.UpdateState();
        }
    }

    public void SwitchState(IAssistantState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState(this);
    }
}
