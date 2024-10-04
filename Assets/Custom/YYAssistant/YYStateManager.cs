using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(DataFetcher))]
[RequireComponent(typeof(EmotionManager))]
public class YYStateManager : MonoBehaviour
{
    public TextMeshProUGUI debugger;
    public ServiceRecorder recordService;
    [HideInInspector]
    public AudioManager audioManager;
    [HideInInspector]
    public DataFetcher dataFetcher;
    [HideInInspector]
    public EmotionManager emotionManager;

    [SerializeField] public InputAction recordButton, stopButton;

    public IAssistantState CurrentState { get; private set; }
    public readonly IAssistantState IdleState = new IdleState();
    public readonly IAssistantState RecordingState = new RecordingState();
    public readonly IAssistantState AnsweringState = new AnsweringState();
    // Start is called before the first frame update

    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        dataFetcher = GetComponent<DataFetcher>();
        emotionManager = GetComponent<EmotionManager>();
        CurrentState = IdleState;
        CurrentState.EnterState(this);

        recordButton.Enable();
        stopButton.Enable();
    }

    void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CurrentState.UpdateState();
    }

    public void SwitchState(IAssistantState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState(this);
    }
}
