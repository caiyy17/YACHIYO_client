using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System;

public class VoiceDetector : MonoBehaviour
{
    private SignalManager signalManager;
    private MicrophoneManager microphoneManager;
    public bool useVAD = false;
    private bool _useVAD = false;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;
    public float timeWindow = 0.3f;
    public bool isSpeaking = false;

    public GameObject VADEnabledIndicator;
    public GameObject VADIndicator;
    public float currentLoudness = 0;

    [SerializeField] public InputAction recordButton;

    void Awake()
    {
        useVAD = false;
        _useVAD = true;
    }
    void Start()
    {
        signalManager = GetComponent<SignalManager>();
        microphoneManager = MicrophoneManager.Instance;
        silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);
        speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
        recordButton.Enable();
    }

    void OnDisable(){
        recordButton.Disable();
    }

    void Update()
    {
        if(!isSpeaking){
            if(recordButton.WasPerformedThisFrame()){
                signalManager.SendSignal("VAD_start","manual_start");
                isSpeaking = true;
                _useVAD = false;
            }
            else if(useVAD && _useVAD){
                currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                if (currentLoudness > speakingThreshold)
                {
                    signalManager.SendSignal("VAD_start","auto_start");
                    isSpeaking = true;
                }
            }
        }
        else{
            if(recordButton.WasReleasedThisFrame()){
                signalManager.SendSignal("VAD_stop","manual_stop");
                isSpeaking = false;
                _useVAD = true;
            }
            else if(useVAD && _useVAD){
                currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                if (currentLoudness < silenceThreshold)
                {
                    signalManager.SendSignal("VAD_stop","auto_stop");
                    isSpeaking = false;
                }
            }
        }

        if(isSpeaking){
            VADEnabledIndicator.SetActive(true);
            VADIndicator.SetActive(true);
        }
        else{
            VADEnabledIndicator.SetActive(true);
            VADIndicator.SetActive(false);
        }
    }
    public void SetVAD(bool value)
    {
        useVAD = value;
    }
}
