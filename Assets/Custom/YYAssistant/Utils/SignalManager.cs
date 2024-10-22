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
    List<Signal> _signals = new List<Signal>();

    [System.Serializable]
    public class SignalRoute
    {
        public string source;
        public string target;
        public string sourceMessage;
        public string targetMessage;
    }
    public List<SignalRoute> signalRoutes = new List<SignalRoute>();

    void Start()
    {
        _signals = new List<Signal>(signals);
    }

    public void SendSignal(string name, string data)
    {
        SendSignalDirect(name, data);
        foreach (SignalRoute route in signalRoutes)
        {
            if (route.source == name)
            {
                if (route.sourceMessage == "all" && route.targetMessage == "all")
                {
                    SendSignalDirect(route.target, data);
                }
                else if (route.sourceMessage == "all")
                {
                    SendSignalDirect(route.target, route.targetMessage);
                }
                else if (route.targetMessage == "all" && route.sourceMessage == data)
                {
                    SendSignalDirect(route.target, data);
                }
                else if (route.sourceMessage == data)
                {
                    SendSignalDirect(route.target, route.targetMessage);
                }
            }
        }
    }

    void SendSignalDirect(string name, string data)
    {
        foreach (Signal signal in _signals)
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
        foreach (Signal signal in _signals)
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
        _signals.Add(newSignal);
    }

    public void RemoveSignal(string name, UnityAction<string> action)
    {
        if (action == null) return;
        foreach (Signal signal in _signals)
        {
            if (signal.name == name)
            {
                signal.signalEvent.RemoveListener(action);
                return;
            }
        }
    }

    void Dispose()
    {
        foreach (Signal signal in _signals)
        {
            signal.signalEvent.RemoveAllListeners();
        }
    }
}