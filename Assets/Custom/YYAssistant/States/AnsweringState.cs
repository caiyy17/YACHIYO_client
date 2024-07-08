using UnityEngine;
using System.Collections;

public class AnsweringState : IAssistantState
{
    public void EnterState(YYStateManager manager)
    {
        Debug.Log("Entering Answering State");
    }

    public void ExitState(YYStateManager manager)
    {
        Debug.Log("Exiting Answering State");
    }

    public void UpdateState(YYStateManager manager)
    {
        // Answering状态下的更新逻辑
        if (manager.keyMapper.ButtonStopPressed()){
            UnityEngine.Debug.Log("Stop fetching");
            manager.dataFetcher.StopFetching();
            manager.audioManager.StopPlayingFlag = true;
            manager.SwitchState(manager.IdleState);
        }
    }
}