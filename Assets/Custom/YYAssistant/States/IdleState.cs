using UnityEngine;
using System.Collections;

public class IdleState : IAssistantState
{
    public void EnterState(YYStateManager manager)
    {
        Debug.Log("Entering Idle State");
    }

    public void ExitState(YYStateManager manager)
    {
        Debug.Log("Exiting Idle State");
    }

    public void UpdateState(YYStateManager manager)
    {
        // Idle状态下的更新逻辑
        manager.emotionManager.SetMotionAndExpression("idle");
        if(manager.keyMapper.ButtonRecordPressed())
        {
            if(manager.audioRecorder.isRecording){
                UnityEngine.Debug.LogError("Recorder is already recording, please stop it first");
                return;
            }
            Debug.Log("Start recording");
            manager.emotionManager.SetMotionAndExpression("listening");
            manager.audioRecorder.StartRecording();
            manager.SwitchState(manager.RecordingState);
        }
        if (manager.keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Clear all");
            manager.audioManager.ResetAll();
        }
    }
}