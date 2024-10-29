using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System;

public class VoiceDetector : MonoBehaviour
{
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
                isSpeaking = true;
                _useVAD = false;
            }
            else if(useVAD && _useVAD){
                currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                if (currentLoudness > speakingThreshold)
                {
                    isSpeaking = true;
                }
            }
        }
        else{
            if(recordButton.WasReleasedThisFrame()){
                isSpeaking = false;
                _useVAD = true;
            }
            else if(useVAD && _useVAD){
                currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                if (currentLoudness < silenceThreshold)
                {
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
