using UnityEngine;
using UnityEngine.UI;

public class MobileKeyMapper : KeyMapper
{
    public ButtonKeySimulator recordButton;
    public ButtonKeySimulator stopButton;
    public ButtonKeySimulator exitButton;

    public override bool ButtonRecordPressed()
    {
        return recordButton.GetKeyDown();
    }

    public override bool ButtonRecordReleased()
    {
        return recordButton.GetKeyUp();
    }

    public override bool ButtonStopPressed()
    {
        return stopButton.GetKeyDown();
    }

    public override bool ButtonExitPressed()
    {
        return exitButton.GetKeyDown();
    }
}