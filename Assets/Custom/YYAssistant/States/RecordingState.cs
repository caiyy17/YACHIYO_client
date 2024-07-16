using UnityEngine;
using System.Collections;

public class RecordingState : YYState
{
    private float startTime;
    float deltaTime = 0.2f;
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        this.manager.debugger.text = "Start Recording";
        Debug.Log("Start recording");
        this.manager.emotionManager.SetMotionAndExpression("listening");
        this.manager.audioRecorder.StartRecording();
        startTime = Time.time;
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        manager.debugger.text = "Updating Recording State";
        base.UpdateState();
        // Recording状态下的更新逻辑
        if (manager.recordButton.WasReleasedThisFrame())
        {
            manager.debugger.text = "Stop Recording";
            if(!manager.audioRecorder.isRecording){
                Debug.LogError("Recorder is not recording, please start it first");
                manager.SwitchState(manager.IdleState);
                return;
            }
            Debug.Log("Stop recording and process");
            manager.audioRecorder.StopRecordingAndSave();
            if(Time.time - startTime < deltaTime){
                Debug.Log("Recording time is too short, please record again");
                manager.SwitchState(manager.IdleState);
                return;
            }
            manager.audioManager.ResetAll();
            manager.StartCoroutine(WaitDataReady());
        }
        else if (manager.stopButton.WasPerformedThisFrame()){
            Debug.Log("Clear all");
            manager.audioRecorder.StopRecordingAndSave();
            manager.SwitchState(manager.IdleState);
        }
    }

    IEnumerator WaitDataReady()
    {
        yield return new WaitUntil(() => manager.audioRecorder.isDataReady);
        manager.SwitchState(manager.AnsweringState);
    }
}