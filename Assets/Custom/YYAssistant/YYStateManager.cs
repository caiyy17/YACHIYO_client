using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ServiceRecorder))]
[RequireComponent(typeof(VoiceDetector))]
public class YYStateManager : MonoBehaviour
{
    public TextMeshProUGUI debugger;
    [HideInInspector]
    public ServiceRecorder recordService;
    [HideInInspector]
    public VoiceDetector voiceDetector;
    [HideInInspector]
    public WebSocketClient webSocketClient;
    public bool useVAD = true;

    [SerializeField] public InputAction recordButton, stopButton;

    public IAssistantState CurrentState { get; private set; }
    public readonly IAssistantState IdleState = new IdleState();
    public readonly IAssistantState RecordingState = new RecordingState();
    public readonly IAssistantState AnsweringState = new AnsweringState();

    public StringEvent stateChangeEvent, cancelEvent;
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
        await webSocketClient.Connect();
        Init();
    }

    void Init()
    {
        useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
        CurrentState = IdleState;
        CurrentState.EnterState(this);
        recordButton.Enable();
        stopButton.Enable();
        isStarted = true;
        Debug.Log("YYStateManager is started");
        Debug.Log(voiceDetector.useVAD);
    }

    void OnEnable()
    {
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
