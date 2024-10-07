using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance;

    private AudioClip microphoneClip;
    public int sampleRate = 16000;
    private int bufferLength = 60;
    private float[] audioBuffer;
    private int bufferPosition = 0;
    private bool isRecording = false;
    private string microphone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMicrophone();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps(microphone, out minFreq, out maxFreq);
            sampleRate = Mathf.Clamp(sampleRate, minFreq, maxFreq);
            Debug.Log("Sample rate: " + sampleRate + " in (" + minFreq + ", " + maxFreq + ")");

            audioBuffer = new float[sampleRate * bufferLength];
            microphoneClip = Microphone.Start(microphone, true, bufferLength, sampleRate);
            isRecording = true;
        }
        else
        {
            Debug.LogError("No microphone found");
        }
    }

    private void Update()
    {
        if (isRecording)
        {
            int micPosition = Microphone.GetPosition(microphone);
            bufferPosition = micPosition;
        }
    }

    public float[] GetAudioData(int startSample, int endSample)
    {
        // make startSample and endSample in the range of audioBuffer
        startSample = ((startSample) % audioBuffer.Length + audioBuffer.Length) % audioBuffer.Length;
        endSample = ((endSample) % audioBuffer.Length + audioBuffer.Length) % audioBuffer.Length;

        microphoneClip.GetData(audioBuffer, 0);
        int length;
        if (startSample < endSample){
            length = endSample - startSample;
        }
        else{
            length = audioBuffer.Length - startSample + endSample;
        }

        float[] data = new float[length];
        if (startSample < endSample)
        {
            System.Array.Copy(audioBuffer, startSample, data, 0, length);
        }
        else
        {
            // 处理循环缓冲区的情况
            int firstPartLength = audioBuffer.Length - startSample;
            System.Array.Copy(audioBuffer, startSample, data, 0, firstPartLength);
            System.Array.Copy(audioBuffer, 0, data, firstPartLength, endSample);
        }
        return data;
    }

    public float[] GetAudioDataLength(int startSample, int length){
        int endSample = startSample + length;
        return GetAudioData(startSample, endSample);
    }

    public int GetCurrentSamplePosition()
    {
        return bufferPosition;
    }
}
