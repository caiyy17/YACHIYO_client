using UnityEngine;
using UnityEngine.UI;

public class MobileKeyMapper : KeyMapper
{
    public ButtonKeySimulator recordButton;
    public ButtonKeySimulator stopButton;
    public ButtonKeySimulator exitButton;

    public override bool ButtonRecordPressed()
    {
        if(recordButton == null)
        {
            Debug.LogError("Record button is not set");
            return false;
        }
        return recordButton.GetKeyDown();
    }

    public override bool ButtonRecordReleased()
    {
        if(recordButton == null)
        {
            Debug.LogError("Record button is not set");
            return false;
        }
        return recordButton.GetKeyUp();
    }

    public override bool ButtonStopPressed()
    {
        if(recordButton == null)
        {
            Debug.LogError("Record button is not set");
            return false;
        }
        return stopButton.GetKeyDown();
    }

    public override bool ButtonExitPressed()
    {
        if(recordButton == null)
        {
            Debug.LogError("Record button is not set");
            return false;
        }
        return exitButton.GetKeyDown();
    }
}