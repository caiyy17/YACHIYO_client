using UnityEngine;

public abstract class KeyMapper : MonoBehaviour
{
    public abstract bool ButtonRecordPressed();
    public abstract bool ButtonRecordReleased();
    public abstract bool ButtonStopPressed();
    public abstract bool ButtonExitPressed();
}