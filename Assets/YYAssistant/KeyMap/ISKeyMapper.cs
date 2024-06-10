using UnityEngine;
using UnityEngine.InputSystem;
public class ISKeyMapper : KeyMapper
{
    public void OnRecord(InputAction.CallbackContext context)
    {
        Debug.Log("Record button pressed");
        recordButtonPressed = context.started;
        recordButtonReleased = context.canceled;
    }

    public void OnStop(InputAction.CallbackContext context)
    {
        stopButtonPressed = context.started;
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        exitButtonPressed = context.started;
    }
    public override bool ButtonRecordPressed()
    {
        return recordButtonPressed;
    }

    public override bool ButtonRecordReleased()
    {
        return recordButtonReleased;
    }

    public override bool ButtonStopPressed()
    {
        return stopButtonPressed;
    }

    public override bool ButtonExitPressed()
    {
        return exitButtonPressed;
    }
}