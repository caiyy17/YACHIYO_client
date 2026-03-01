using UnityEngine.Events;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }
[System.Serializable]
public class FloatEvent : UnityEvent<float> { }
[System.Serializable]
public class SignalEvent : UnityEvent<string, string> { }