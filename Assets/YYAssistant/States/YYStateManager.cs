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
    public string addr = "http://localhost:5050";

    public IAssistantState CurrentState { get; private set; }
    public readonly IdleState IdleState = new IdleState();
    public readonly RecordingState RecordingState = new RecordingState();
    public readonly ThinkingState ThinkingState = new ThinkingState();
    public readonly AnsweringState AnsweringState = new AnsweringState();
    // Start is called before the first frame update
    void Start()
    {
        audioRecorder = GetComponent<AudioRecorder>();
        audioManager = GetComponent<AudioManager>();
        dataFetcher = GetComponent<DataFetcher>();
        emotionManager = GetComponent<EmotionManager>();
        addr = PlayerPrefs.GetString("urlInput", addr);
        dataFetcher.SetUrl(addr);

        CurrentState = IdleState;
        CurrentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        CurrentState.UpdateState(this);
    }

    public void SwitchState(IAssistantState newState)
    {
        CurrentState.ExitState(this);
        CurrentState = newState;
        CurrentState.EnterState(this);
    }

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
}
