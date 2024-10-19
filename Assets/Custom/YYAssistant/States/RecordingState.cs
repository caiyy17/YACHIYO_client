using UnityEngine;
using System.Collections;

public class RecordingState : YYState
{
    private float startTime;
    float deltaTime = 0.7f;
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        if(this.manager.stateChangeEvent != null){
            this.manager.stateChangeEvent.Invoke("listening");
        }
        this.manager.recordService.StartRecording(-0.3f);
        startTime = Time.time;
    }

    public override void ExitState()
    {
        base.ExitState();
        manager.voiceDetector.SetVAD(false);
    }

    public override void UpdateState()
    {
        manager.debugger.text = "Updating Recording State";
        base.UpdateState();
        // Recording状态下的更新逻辑
        if (manager.recordButton.WasReleasedThisFrame() || (manager.voiceDetector.useVAD && !manager.voiceDetector.isSpeaking) || Time.time - startTime > manager.recordService.maxRecordingTime)
        {
            if(!manager.recordService.isRecording){
                Debug.LogError("Recorder is not recording, please start it first");
                manager.SwitchState(manager.IdleState);
                return;
            }
            manager.debugger.text = "Stop recording and process";
            Debug.Log("Stop recording and process");
            manager.recordService.StopRecording();
            if(Time.time - startTime < deltaTime){
                Debug.Log("Recording time is too short, please record again");
                manager.SwitchState(manager.IdleState);
                return;
            }
            manager.SwitchState(manager.AnsweringState);
        }
        else if (manager.stopButton.WasPerformedThisFrame()){
            Debug.Log("Clear all");
            manager.recordService.StopRecording();
            if(manager.cancelEvent != null){
                manager.cancelEvent.Invoke("cancel in recording");
            }
            manager.SwitchState(manager.IdleState);
        }
    }
}