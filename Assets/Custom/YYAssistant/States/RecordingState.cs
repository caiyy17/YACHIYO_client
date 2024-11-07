using UnityEngine;
using System.Collections;

public class RecordingState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.signalManager.SendSignal("yya_state", "listening");
        this.manager.recordService.StartRecording(-manager.voiceDetector.loudnessHoldTime);
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
        if (!manager.voiceDetector.isSpeaking)
        {
            manager.debugger.text = "Stop recording and process";
            Debug.Log("Stop recording and process");
            manager.recordService.StopRecording();
            manager.SwitchState(manager.AnsweringState);
        }
        else if (manager.stopButton.WasPerformedThisFrame()){
            Debug.Log("Clear all");
            manager.recordService.StopRecording();
            manager.signalManager.SendSignal("cancel", "cancel in recording");
            manager.SwitchState(manager.IdleState);
        }
    }
}