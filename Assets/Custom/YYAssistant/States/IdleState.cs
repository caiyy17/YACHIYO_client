using UnityEngine;
using System.Collections;

public class IdleState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.voiceDetector.SetVAD(true);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        // Idle状态下的更新逻辑
        manager.emotionManager.SetMotionAndExpression("idle");

        if(manager.recordButton.WasPerformedThisFrame()){
            manager.voiceDetector.SetVAD(false);
        }

        if(manager.recordButton.WasPerformedThisFrame() || manager.voiceDetector.IsSpeaking())
        {
            if(manager.recordService.isRecording){
                UnityEngine.Debug.LogError("Recorder is already recording, stop it first");
                manager.recordService.StopRecording();
            }
            else{
                manager.SwitchState(manager.RecordingState);
            }
        }
        else if (manager.stopButton.WasPerformedThisFrame()){
            UnityEngine.Debug.Log("Clear all");
            manager.audioManager.ResetAll();
        }
    }
}