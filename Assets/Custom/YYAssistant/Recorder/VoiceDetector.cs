using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

public class VoiceDetector : MonoBehaviour
{
    private MicrophoneManager microphoneManager;
    public bool useVAD = false;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;
    public float timeWindow = 0.3f;
    public bool isSpeaking = false;

    public GameObject VADEnabledIndicator;
    public GameObject VADIndicator;
    public float currentLoudness = 0;

    void Awake()
    {
        useVAD = false;
    }
    void Start()
    {
        microphoneManager = MicrophoneManager.Instance;
        silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);
        speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
    }

    void Update()
    {
        if(useVAD)
        {
            currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
            // Debug.Log("Loudness: " + currentLoudness);
            if(!isSpeaking){
                if (currentLoudness > speakingThreshold)
                {
                    isSpeaking = true;
                }
            }
            else if (currentLoudness < silenceThreshold)
            {
                isSpeaking = false;
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
        else
        {
            isSpeaking = false;
            VADEnabledIndicator.SetActive(false);
            VADIndicator.SetActive(false);
        }
    }
    public void SetVAD(bool value)
    {
        useVAD = value;
    }
}
