using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

public class VoiceDetector : MonoBehaviour
{
    public bool useVAD = true;
    private int startSample;
    private int endSample;
    private float[] audioData = null;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;
    public float timeWindow = 0.5f;
    private bool isSpeaking = false;
    private bool isEnding = false;

    public string serviceName = "vad";
    public int sampleRate = 16000;

    public GameObject VADIndicator;

    private void Start()
    {
        sampleRate = MicrophoneManager.Instance.sampleRate;
        WavUtility.WavFormat wav = WavUtility.Load(WavUtility.emptyClip);
    }

    void Update()
    {
        if(useVAD)
        {
            startSample = MicrophoneManager.Instance.GetCurrentSamplePosition() - (int)(sampleRate * timeWindow);
            endSample = MicrophoneManager.Instance.GetCurrentSamplePosition();
            float loudness = GetCurrentLoudness();
            Debug.Log("Loudness: " + loudness);
            if (loudness > speakingThreshold)
            {
                isSpeaking = true;
                isEnding = false;
                VADIndicator.SetActive(true);
            }
            else if (loudness < silenceThreshold)
            {
                isSpeaking = false;
                isEnding = true;
                VADIndicator.SetActive(false);
            }
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
        return isSpeaking;
    }

    public bool IsEnding()
    {
        return isEnding;
    }
}
