using UnityEngine;

public abstract class KeyMapper : MonoBehaviour
{
    protected bool recordButtonPressed = false;
    protected bool recordButtonReleased = false;
    protected bool stopButtonPressed = false;
    protected bool exitButtonPressed = false;
    public abstract bool ButtonRecordPressed();
    public abstract bool ButtonRecordReleased();
    public abstract bool ButtonStopPressed();
    public abstract bool ButtonExitPressed();

    protected void LateUpdate()
    {
        recordButtonPressed = false;
        recordButtonReleased = false;
        stopButtonPressed = false;
        exitButtonPressed = false;
    }
}