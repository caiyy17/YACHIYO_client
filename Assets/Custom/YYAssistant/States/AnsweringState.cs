using UnityEngine;
using System.Collections;
using NUnit.Framework.Internal;

public class AnsweringState : YYState
{
    bool isFetching = false;
    bool isFinished = false;
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.signalManager.AddSignal("answer_end", OnAnsweringFinished); 
        isFetching = false;
        isFinished = false;
        manager.stateChangeEvent.Invoke("answering");
        // Answering状态下的进入逻辑
    }

    public override void ExitState()
    {
        base.ExitState();
        manager.signalManager.RemoveSignal("answering", OnAnsweringFinished);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!manager.recordService.isDataReady && !isFetching){
            return;
        }
        else if(manager.recordService.isDataReady && !isFetching){
            Debug.Log("Start fetching");
            manager.webSocketClient.sendAudio(manager.recordService.wavData);
            isFetching = true;
        }
        else if(isFetching){
            if(isFinished){
                Debug.Log("Answering finished");
                manager.SwitchState(manager.IdleState);
            }
            else if (manager.stopButton.WasReleasedThisFrame()){
                Debug.Log("Stop fetching");
                manager.webSocketClient.sendCancel("cancel");
                manager.cancelEvent.Invoke("cancel in answering");
                manager.SwitchState(manager.IdleState);
            }
        }
    }
    
    void OnAnsweringFinished(string result){
        isFinished = true;
    }
}