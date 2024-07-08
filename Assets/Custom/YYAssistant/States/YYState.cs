using UnityEngine;
using System.Collections;

public interface IAssistantState
{
    void EnterState(YYStateManager manager);
    void ExitState(YYStateManager manager);
    void UpdateState(YYStateManager manager);
}

