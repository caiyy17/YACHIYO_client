using UnityEngine;
using System.Collections;

public class RecordingState : IAssistantState
{
    bool isWaitingData = false;
    public void EnterState(YYStateManager manager)
    {
        isWaitingData = false;
        Debug.Log("Entering Recording State");
    }

    public void ExitState(YYStateManager manager)
    {
        Debug.Log("Exiting Recording State");
    }

    public void UpdateState(YYStateManager manager)
    {
        // Recording状态下的更新逻辑
        if (isWaitingData){
            return;
        }
        else if (manager.keyMapper.ButtonRecordReleased())
        {
            if(!manager.audioRecorder.isRecording){
                Debug.LogError("Recorder is not recording, please start it first");
                manager.SwitchState(manager.IdleState);
                return;
            }
            Debug.Log("Stop recording and process");
            manager.emotionManager.SetMotionAndExpression("thinking");
            manager.audioRecorder.StopRecordingAndSave();
            manager.audioManager.ResetAll();
            isWaitingData = true;
            manager.StartManagedCoroutine(WaitDataReady(manager));
        }
        else if (manager.keyMapper.ButtonStopPressed()){
            Debug.Log("Clear all");
            manager.audioRecorder.StopRecordingAndSave();
            manager.SwitchState(manager.IdleState);
        }
    }

    IEnumerator WaitDataReady(YYStateManager manager)
    {
        yield return new WaitUntil(() => manager.audioRecorder.isDataReady);
        manager.SwitchState(manager.ThinkingState);
    }
}