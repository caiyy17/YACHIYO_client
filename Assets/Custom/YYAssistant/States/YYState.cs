using UnityEngine;
using System.Collections;

public interface IAssistantState
{
    void EnterState(YYStateManager manager);
    void ExitState();
    void UpdateState();
}

public class YYState : IAssistantState
{
    protected YYStateManager manager;
    public virtual void EnterState(YYStateManager manager)
    {
        this.manager = manager;
        Debug.Log("Entering " + this.GetType().Name);
    }

    public virtual void ExitState()
    {
        Debug.Log("Exiting " + this.GetType().Name);
    }

    public virtual void UpdateState()
    {
    }
}