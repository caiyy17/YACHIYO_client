using UnityEngine;

public class IdleState : YYState
{
    public YYState defaultState;
    public YYState readyState;
    public YYState listeningState;
    bool readyStart;
    bool listeningStart;
    public override void EnterState()
    {
        base.EnterState();
        manager.signalManager.AddSignal("ready_start", OnReadyStart);
        manager.signalManager.AddSignal("listening_start", OnListeningStart);
        readyStart = false;
        listeningStart = false;
        manager.signalManager.SendSignal("yya_state", "idle");
    }

    public override void ExitState()
    {
        base.ExitState();
        manager.signalManager.RemoveSignal("ready_start", OnReadyStart);
        manager.signalManager.RemoveSignal("listening_start", OnListeningStart);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (readyStart)
        {
            manager.SwitchState(readyState);
        }
        else if (listeningStart)
        {
            manager.SwitchState(listeningState);
        }
        else if (manager.stopButton.WasPerformedThisFrame())
        {
            readyStart = false;
            Debug.Log("cancel in idle");
            manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in idle\"}");
            manager.SwitchState(defaultState);
        }
    }

    public void OnReadyStart(string result)
    {
        readyStart = true;
    }

    public void OnListeningStart(string result)
    {
        listeningStart = true;
    }
}