using UnityEngine;
using System.Collections;

public class ThinkingState : IAssistantState
{
    public void EnterState(YYStateManager manager)
    {
        Debug.Log("Entering Thinking State");
        manager.StartManagedCoroutine(asking_coroutine(manager));
    }

    public void ExitState(YYStateManager manager)
    {
        Debug.Log("Exiting Thinking State");
    }

    public void UpdateState(YYStateManager manager)
    {
        // Thinking状态下的更新逻辑
        if (manager.keyMapper.ButtonStopPressed()){
            Debug.Log("Stop fetching");
            manager.dataFetcher.StopFetching();
            manager.audioManager.StopPlayingFlag = true;
            manager.SwitchState(manager.IdleState);
        }
    }

    IEnumerator asking_coroutine(YYStateManager manager){
        Debug.Log("Send data to server");
        manager.audioManager.isAnswering = true;
        yield return manager.StartManagedCoroutine(manager.dataFetcher.GetDataCoroutine(manager.audioRecorder.audioData));
        yield return new WaitForSeconds(0.1f);
        
        while (manager.audioManager.isAudioLoadingOrPlaying){
            yield return new WaitForSeconds(0.1f);
        }
        manager.audioManager.isAnswering = false;
        manager.SwitchState(manager.IdleState);
        yield return null;
    }
}