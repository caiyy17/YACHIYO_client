using UnityEngine;
using System.Collections;

public class IdleState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
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
        if(manager.keyMapper.ButtonRecordPressed())
        {
            if(manager.audioRecorder.isRecording){
                UnityEngine.Debug.LogError("Recorder is already recording, please stop it first");
                return;
            }
            manager.SwitchState(manager.RecordingState);
        }
        if (manager.keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Clear all");
            manager.audioManager.ResetAll();
        }
    }
}