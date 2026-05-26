using UnityEngine;

namespace Yachiyo
{
    public class AnsweringState : YYState
    {
        public YYState idleState;
        public YYState listeningState;
        bool isEnd = false;
        bool isListeningStart = false;

        public override void EnterState()
        {
            base.EnterState();
            manager.signalManager.AddSignal("answering_end", OnAnsweringEnd);
            manager.signalManager.AddSignal("listening_start", OnListeningStart);
            isEnd = false;
            isListeningStart = false;
            manager.signalManager.SendSignal("yya_state", "answering");
        }

        public override void ExitState()
        {
            base.ExitState();
            manager.signalManager.RemoveSignal("answering_end", OnAnsweringEnd);
            manager.signalManager.RemoveSignal("listening_start", OnListeningStart);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (isEnd)
            {
                Debug.Log("Answering end");
                manager.SwitchState(idleState);
            }
            else if (isListeningStart)
            {
                Debug.Log("Listening start, interrupted in answering");
                manager.SwitchState(listeningState);
            }
            else if (manager.stopButton.WasReleasedThisFrame())
            {
                Debug.Log("cancel in answering");
                manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in answering\"}");
                manager.SwitchState(idleState);
            }
        }
        public void OnAnsweringEnd(string result)
        {
            isEnd = true;
        }
        public void OnListeningStart(string result)
        {
            isListeningStart = true;
        }
    }
}
