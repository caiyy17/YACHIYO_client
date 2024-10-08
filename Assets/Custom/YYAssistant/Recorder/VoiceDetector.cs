using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

public class VoiceDetector : MonoBehaviour
{
    public bool useVAD = true;
    public bool activateVAD = false;
    private int startSample;
    private int endSample;
    private float[] audioData = null;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;
    public float timeWindow = 0.5f;
    public bool isSpeaking = false;
    public bool isEnding = true;

    public string serviceName = "vad";
    public int sampleRate = 16000;

    public GameObject VADEnabledIndicator;
    public GameObject VADIndicator;
    public float currentLoudness = 0;
    

    private void Start()
    {
        sampleRate = MicrophoneManager.Instance.sampleRate;
        WavUtility.WavFormat wav = WavUtility.Load(WavUtility.emptyClip);

        useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
        silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);
        speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
    }

    void Update()
    {
        if(useVAD)
        {
            startSample = MicrophoneManager.Instance.GetCurrentSamplePosition() - (int)(sampleRate * timeWindow);
            endSample = MicrophoneManager.Instance.GetCurrentSamplePosition();
            currentLoudness = GetCurrentLoudness();
            // Debug.Log("Loudness: " + currentLoudness);
            if(!isSpeaking){
                if (currentLoudness > speakingThreshold)
                {
                    isSpeaking = true;
                    isEnding = false;
                }
            }
            else if (currentLoudness < silenceThreshold)
            {
                isSpeaking = false;
                isEnding = true;
            }

            if(activateVAD){
                if(isSpeaking){
                    VADEnabledIndicator.SetActive(true);
                    VADIndicator.SetActive(true);
                }
                else{
                    VADEnabledIndicator.SetActive(true);
                    VADIndicator.SetActive(false);
                }
            }
            else{
                VADEnabledIndicator.SetActive(false);
                VADIndicator.SetActive(false);
            }

        }
        else
        {
            isSpeaking = false;
            isEnding = false;
            VADEnabledIndicator.SetActive(false);
            VADIndicator.SetActive(false);
        }
    }

    float GetCurrentLoudness()
    {
        audioData = MicrophoneManager.Instance.GetAudioData(startSample, endSample);

        float sum = 0;
        for (int i = 0; i < audioData.Length; i++)
        {
            sum += audioData[i] * audioData[i];
        }

        float rmsValue = Mathf.Sqrt(sum / audioData.Length);
        return rmsValue;
    }

    public bool IsSpeaking()
    {
        return isSpeaking && activateVAD;
    }

    public bool IsEnding()
    {
        return isEnding && activateVAD;
    }
    
    public void SetVAD(bool value)
    {
        activateVAD = value;
    }
}
