using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioRecorder))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(DataFetcher))]
[RequireComponent(typeof(EmotionManager))]
public class YYStateManager : MonoBehaviour
{
    [HideInInspector]
    public AudioRecorder audioRecorder;
    [HideInInspector]
    public AudioManager audioManager;
    [HideInInspector]
    public DataFetcher dataFetcher;
    [HideInInspector]
    public EmotionManager emotionManager;
    public KeyMapper keyMapper;

    public IAssistantState CurrentState { get; private set; }
    public readonly IAssistantState IdleState = new IdleState();
    public readonly IAssistantState RecordingState = new RecordingState();
    public readonly IAssistantState AnsweringState = new AnsweringState();
    // Start is called before the first frame update

    void Start()
    {
        audioRecorder = GetComponent<AudioRecorder>();
        audioManager = GetComponent<AudioManager>();
        dataFetcher = GetComponent<DataFetcher>();
        emotionManager = GetComponent<EmotionManager>();

        CurrentState = IdleState;
        CurrentState.EnterState(this);
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
