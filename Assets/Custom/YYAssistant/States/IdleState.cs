using UnityEngine;
using System.Collections;

public class IdleState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.signalManager.SendSignal("yya_state", "idle");
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        // Idle状态下的更新逻辑

        if(manager.voiceDetector.isSpeaking)
        {
            manager.SwitchState(manager.RecordingState);
        }
        else if(manager.stopButton.WasPerformedThisFrame()){
            manager.signalManager.SendSignal("cancel", "cancel in idle");
        }
    }
}