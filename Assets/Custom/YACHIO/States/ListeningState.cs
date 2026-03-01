using UnityEngine;

public class ListeningState : YYState
{
    public YYState defaultState;
    public YYState answeringState;
    bool listeningEnd;
    public override void EnterState()
    {
        base.EnterState();
        manager.signalManager.AddSignal("listening_end", OnListeningEnd);
        listeningEnd = false;
        manager.signalManager.SendSignal("yya_state", "listening");
    }

    public override void ExitState()
    {
        base.ExitState();
        manager.signalManager.RemoveSignal("listening_end", OnListeningEnd);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (listeningEnd)
        {
            manager.SwitchState(answeringState);
        }
        else if (manager.stopButton.WasPerformedThisFrame())
        {
            listeningEnd = false;
            Debug.Log("cancel in listening");
            manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in listening\"}");
            manager.SwitchState(defaultState);
        }
    }

    public void OnListeningEnd(string result)
    {
        listeningEnd = true;
    }
}