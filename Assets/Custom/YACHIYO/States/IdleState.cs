using UnityEngine;

namespace Yachiyo
{
    public class IdleState : YYState
    {
        public YYState idleState;
        public YYState listeningState;
        bool listeningStart;
        public override void EnterState()
        {
            base.EnterState();
            manager.signalManager.AddSignal("listening_start", OnListeningStart);
            listeningStart = false;
            manager.signalManager.SendSignal("yya_state", "idle");
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
            else if (manager.stopButton.WasPerformedThisFrame())
            {
                Debug.Log("cancel in idle");
                manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in idle\"}");
                manager.SwitchState(idleState);
            }
        }

        public void OnListeningStart(string result)
        {
            listeningStart = true;
        }
    }
}
