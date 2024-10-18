using UnityEngine;
using System.Collections;

public class IdleState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.stateChangeEvent.Invoke("idle");
        manager.voiceDetector.SetVAD(manager.useVAD);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        // Idle状态下的更新逻辑
        if(manager.recordButton.WasPerformedThisFrame()){
            manager.voiceDetector.SetVAD(false);
        }

        if(manager.recordButton.WasPerformedThisFrame() || (manager.voiceDetector.useVAD && manager.voiceDetector.isSpeaking))
        {
            if(manager.recordService.isRecording){
                UnityEngine.Debug.LogError("Recorder is already recording, stop it first");
                manager.recordService.StopRecording();
                manager.voiceDetector.SetVAD(manager.useVAD);
            }
            else{
                manager.SwitchState(manager.RecordingState);
            }
        }
        else if(manager.stopButton.WasPerformedThisFrame()){
            manager.cancelEvent.Invoke("cancel in idle");
        }
    }
}