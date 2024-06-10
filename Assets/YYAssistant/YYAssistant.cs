using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioRecorder))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(DataFetcher))]
[RequireComponent(typeof(EmotionManager))]
public class YYAssistant : MonoBehaviour
{
    enum Mode { idle, record, answer, exit};
    Mode mode = Mode.idle;
    public KeyMapper keyMapper;

    AudioRecorder recorder;
    AudioManager audioManager;
    DataFetcher dataFetcher;
    EmotionManager emotionManager;
    public string addr = "http://localhost:5050";

    public IAssistantState CurrentState { get; private set; }

    void Start()
    {
        recorder = GetComponent<AudioRecorder>();
        audioManager = GetComponent<AudioManager>();
        dataFetcher = GetComponent<DataFetcher>();
        emotionManager = GetComponent<EmotionManager>();
        SetUrl(addr);
    }

    public void SetUrl(string url)
    {
        addr = url;
        dataFetcher.SetUrl(url);
        UnityEngine.Debug.Log("URL changed to: " + addr);
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.idle:
                HandleIdleState();
                break;

            case Mode.record:
                HandleRecordState();
                break;

            case Mode.answer:
                HandleAnswerState();
                break;

            case Mode.exit:
                HandleExitState();
                break;
        }
    }

    void HandleIdleState()
    {
        emotionManager.SetMotionAndExpression("idle");
        if(keyMapper.ButtonRecordPressed())
        {
            if(recorder.isRecording){
                UnityEngine.Debug.LogError("Recorder is already recording, please stop it first");
                return;
            }
            UnityEngine.Debug.Log("Start recording");
            emotionManager.SetMotionAndExpression("listening");
            recorder.StartRecording();
            mode = Mode.record;
        }
        if (keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Clear all");
            audioManager.StopPlayingFlag = true;
            mode = Mode.idle;
        }
    }

    void HandleRecordState()
    {
        if (keyMapper.ButtonRecordReleased())
        {
            if(!recorder.isRecording){
                UnityEngine.Debug.LogError("Recorder is not recording, please start it first");
                mode = Mode.idle;
                return;
            }
            UnityEngine.Debug.Log("Stop recording and process");
            emotionManager.SetMotionAndExpression("thinking");
            recorder.StopRecordingAndSave();
            audioManager.ResetAll();
            StartCoroutine(answer_coroutine());
            mode = Mode.answer;
        }
        if (keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Clear all");
            recorder.StopRecordingAndSave();
            mode = Mode.idle;
        }
    }

    void HandleAnswerState()
    {
        if (keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Stop fetching");
            dataFetcher.StopFetching();
            audioManager.StopPlayingFlag = true;
            mode = Mode.idle;
        }
    }

    void HandleExitState()
    {
        WaitAndExecute(0.1f, () => { Application.Quit(); });
    }

    IEnumerator answer_coroutine(){
        yield return new WaitUntil(() => recorder.isDataReady == true);
        audioManager.isAnswering = true;
        UnityEngine.Debug.Log("Send data to server");
        yield return StartCoroutine(dataFetcher.GetDataCoroutine(recorder.audioData));
        yield return new WaitForSeconds(0.1f);
        
        while (audioManager.isAudioLoadingOrPlaying){
            yield return new WaitForSeconds(0.1f);
        }
        audioManager.isAnswering = false;
        UnityEngine.Debug.Log("Finished answer");
        mode = Mode.idle;
        yield return null;
    }

    IEnumerator WaitAndExecute(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action?.Invoke();
    }
}
