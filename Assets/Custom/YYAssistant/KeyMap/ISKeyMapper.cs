using UnityEngine;
using UnityEngine.InputSystem;
public class ISKeyMapper : KeyMapper
{
    private PlayerInput playerInput;
    private InputAction recordButton, stopButton, exitButton;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        recordButton = playerInput.actions.FindAction("record");
        stopButton = playerInput.actions.FindAction("stop");
        exitButton = playerInput.actions.FindAction("exit");
    }
    public override bool ButtonRecordPressed()
    {
        return recordButton.WasPerformedThisFrame();
    }

    public override bool ButtonRecordReleased()
    {
        return recordButton.WasReleasedThisFrame();
    }

    public override bool ButtonStopPressed()
    {
        return stopButton.WasPerformedThisFrame();
    }

    public override bool ButtonExitPressed()
    {
        return exitButton.WasPerformedThisFrame();
    }
}