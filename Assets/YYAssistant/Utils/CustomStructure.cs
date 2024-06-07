using UnityEngine.Events;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }
[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public interface IAssistantState
{
    void EnterState(YYAssistant assistant);
    void ExitState(YYAssistant assistant);
    void UpdateState(YYAssistant assistant);
}