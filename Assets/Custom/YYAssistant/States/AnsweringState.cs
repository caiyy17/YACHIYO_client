using UnityEngine;
using System.Collections;
using NUnit.Framework.Internal;

public class AnsweringState : YYState
{
    bool isFetching = false;
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        isFetching = false;
        manager.stateChangeEvent.Invoke("answering");
        // Answering状态下的进入逻辑
    }

    public override void ExitState()
    {
        base.ExitState();
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
            if (manager.stopButton.WasReleasedThisFrame()){
                Debug.Log("Stop fetching");
                manager.webSocketClient.sendCancel("stop fetching");
                manager.cancelEvent.Invoke("stop fetching");
                manager.SwitchState(manager.IdleState);
            }
        }
    }
}