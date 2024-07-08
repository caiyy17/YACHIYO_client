using UnityEngine;

public class PCKeyMapper : KeyMapper
{
    public override bool ButtonRecordPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public override bool ButtonRecordReleased()
    {
        return Input.GetKeyUp(KeyCode.Space);
    }

    public override bool ButtonStopPressed()
    {
        return Input.GetKeyDown(KeyCode.N);
    }

    public override bool ButtonExitPressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}