using UnityEngine;

namespace Yachiyo
{
    public class AnsweringState : YYState
    {
        public YYState defaultState;
        public YYState readyState;
        public YYState listeningState;
        bool isEnd = false;
        bool isListeningStart = false;
        bool isReadyStart = false;

        public override void EnterState()
        {
            base.EnterState();
            manager.signalManager.AddSignal("answering_end", OnAnsweringEnd);
            manager.signalManager.AddSignal("listening_start", OnListeningStart);
            manager.signalManager.AddSignal("ready_start", OnReadyStart);
            isEnd = false;
            isListeningStart = false;
            isReadyStart = false;
            manager.signalManager.SendSignal("yya_state", "answering");
        }

        public override void ExitState()
        {
            base.ExitState();
            manager.signalManager.RemoveSignal("answering_end", OnAnsweringEnd);
            manager.signalManager.RemoveSignal("listening_start", OnListeningStart);
            manager.signalManager.RemoveSignal("ready_start", OnReadyStart);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (isEnd)
            {
                Debug.Log("Answering end");
                manager.SwitchState(readyState);
            }
            else if (isListeningStart)
            {
                Debug.Log("Listening start, interrupted in answering");
                manager.SwitchState(listeningState);
            }
            else if (isReadyStart)
            {
                Debug.Log("Ready start, interrupted in answering");
                manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"ready_start in answering\"}");
                manager.SwitchState(readyState);
            }
            else if (manager.stopButton.WasReleasedThisFrame())
            {
                Debug.Log("cancel in answering");
                manager.signalManager.SendSignal("cancel", "{\"signal\":\"cancel\", \"content\":\"cancel in answering\"}");
                manager.SwitchState(defaultState);
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

        public void OnReadyStart(string result)
        {
            isReadyStart = true;
        }
    }
}
