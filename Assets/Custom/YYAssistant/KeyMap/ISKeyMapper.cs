using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class ISKeyMapper : KeyMapper
{
    private PlayerInput playerInput;
    private InputAction recordButton, stopButton, exitButton;
    public TextMeshProUGUI text;
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        recordButton = playerInput.actions.FindAction("record");
        stopButton = playerInput.actions.FindAction("stop");
        exitButton = playerInput.actions.FindAction("exit");

        if (recordButton != null)
        {
            recordButton.Enable();
        }
        if (stopButton != null)
        {
            stopButton.Enable();
        }
        if (exitButton != null)
        {
            exitButton.Enable();
        }
    }
    public override bool ButtonRecordPressed()
    {
        if (recordButton.WasPerformedThisFrame())
        {
            Debug.Log("ButtonRecordPressed");
            text.text += "ButtonRecordPressed\n";
            return true;
        }
        return false;
    }

    public override bool ButtonRecordReleased()
    {
        if (recordButton.WasReleasedThisFrame())
        {
            Debug.Log("ButtonRecordReleased");
            text.text += "ButtonRecordReleased\n";
            return true;
        }
        return false;
    }

    public override bool ButtonStopPressed()
    {
        if (stopButton.WasPerformedThisFrame())
        {
            Debug.Log("ButtonStopPressed");
            text.text += "ButtonStopPressed\n";
            return true;
        }
        return false;
    }

    public override bool ButtonExitPressed()
    {
        if (exitButton.WasPerformedThisFrame())
        {
            Debug.Log("ButtonExitPressed");
            text.text += "ButtonExitPressed\n";
            return true;
        }
        return false;
    }
}