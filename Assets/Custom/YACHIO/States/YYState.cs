using UnityEngine;

public interface IAssistantState
{
    void EnterState();
    void ExitState();
    void UpdateState();
}

public class YYState : MonoBehaviour, IAssistantState
{
    protected YYStateManager manager;
    void Awake()
    {
        manager = GetComponent<YYStateManager>();
    }

    public virtual void EnterState()
    {
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