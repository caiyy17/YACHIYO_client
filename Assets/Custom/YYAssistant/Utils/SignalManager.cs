using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    [System.Serializable]
    public class Signal
    {
        public string name;
        public StringEvent signalEvent;
    }
    public List<Signal> signals = new List<Signal>();

    public void SendSignal(string name, string data)
    {
        foreach (Signal signal in signals)
        {
            if (signal.name == name)
            {
                if (signal.signalEvent == null) return;
                signal.signalEvent.Invoke(data);
                return;
            }
        }
    }

    public void AddSignal(string name, UnityAction<string> action)
    {
        if (action == null) return;
        foreach (Signal signal in signals)
        {
            if (signal.name == name)
            {
                signal.signalEvent.AddListener(action);
                return;
            }
        }
        Signal newSignal = new Signal();
        newSignal.name = name;
        newSignal.signalEvent = new StringEvent();
        newSignal.signalEvent.AddListener(action);
        signals.Add(newSignal);
    }

    public void RemoveSignal(string name, UnityAction<string> action)
    {
        if (action == null) return;
        foreach (Signal signal in signals)
        {
            if (signal.name == name)
            {
                signal.signalEvent.RemoveListener(action);
                return;
            }
        }
    }
}