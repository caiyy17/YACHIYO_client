using UnityEngine;

public class ReadyState : YYState
{
    public YYState defaultState;
    public YYState idleState;
    public YYState listeningState;
    public float countDownTime = 5.0f;

    bool listeningStart;
    float timeStart = 0.0f;

    public override void EnterState()
    {
        base.EnterState();
        manager.signalManager.AddSignal("listening_start", OnListeningStart);
        timeStart = Time.time;
        listeningStart = false;
        manager.signalManager.SendSignal("yya_state", "ready");
    }

    public override void ExitState()
    {
        base.ExitState();
        manager.signalManager.RemoveSignal("listening_start", OnListeningStart);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (listeningStart)
        {
            manager.SwitchState(listeningState);
        }
        else if ((countDownTime != 0) && (Time.time - timeStart > countDownTime))
        {
            Debug.Log("count down finished, switch to idle");
            manager.SwitchState(idleState);
        }
        else if (manager.stopButton.WasPerformedThisFrame())
        {
            listeningStart = false;
            Debug.Log("cancel in ready");
            manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in ready\"}");
            manager.SwitchState(defaultState);
        }
    }

    public void OnListeningStart(string result)
    {
        listeningStart = true;
    }
}
