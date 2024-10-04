using UnityEngine;
using System.Collections;

public class AnsweringState : YYState
{
    public override void EnterState(YYStateManager manager)
    {
        base.EnterState(manager);
        manager.emotionManager.SetMotionAndExpression("thinking");
        manager.StartCoroutine(asking_coroutine());
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        // Answering状态下的更新逻辑
        if (manager.stopButton.WasReleasedThisFrame()){
            Debug.Log("Stop fetching");
            manager.dataFetcher.StopFetching();
            manager.audioManager.StopPlayingFlag = true;
            manager.SwitchState(manager.IdleState);
        }
    }

    IEnumerator asking_coroutine(){
        Debug.Log("Send data to server");
        manager.audioManager.isAnswering = true;
        yield return manager.StartCoroutine(manager.dataFetcher.GetDataCoroutine(manager.recordService.wavData, manager.dataFetcher.userId));
        yield return new WaitForSeconds(0.1f);
        
        while (manager.audioManager.isAudioWaitingOrPlaying){
            yield return new WaitForSeconds(0.1f);
        }
        manager.audioManager.isAnswering = false;
        manager.SwitchState(manager.IdleState);
        yield return null;
    }
}