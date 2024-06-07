using System.Threading;
using UnityEngine;
using System.IO;

public class AudioRecorder : MonoBehaviour
{
    private AudioClip recording;
    public bool isRecording = false;
    public bool isDataReady = false;
    private string microphone;
    public byte[] audioData = null;
    private int sampleRate = 16000;

    void Start()
    {
        // 获取默认麦克风
        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps(microphone, out minFreq, out maxFreq);
            sampleRate = Mathf.Clamp(sampleRate, minFreq, maxFreq);
            Debug.Log("Sample rate: " + sampleRate + " in (" + minFreq + ", " + maxFreq + ")");
        }
        else
        {
            Debug.LogError("No microphone found");
        }
    }

    void Update()
    {
        // 按下M键开始录音
        if(Input.GetKeyDown(KeyCode.M) && !isRecording)
        {
            StartRecording();
        }
        // 松开M键结束录音
        if(Input.GetKeyUp(KeyCode.M) && isRecording)
        {
            StopRecordingAndSave();
        }
    }

    public void StartRecording()
    {
        if (microphone != null)
        {
            isRecording = true;
            recording = Microphone.Start(microphone, false, 60, sampleRate);
        }
    }

    public void StopRecordingAndSave()
    {
        if (microphone != null && isRecording)
        {
            Microphone.End(microphone);
            isRecording = false;
            SaveRecording();
        }
    }

    void SaveRecording()
    {
        isDataReady = false;
        if (recording == null){
            return;
        }
        WavUtility.WavFormat wav = WavUtility.Load(recording);
        Thread saveThread = new Thread(() => {
            wav = WavUtility.TrimSilence(wav, 0.0001f);
            audioData = WavUtility.ConvertToWav(wav.samples, wav.samplesCount, wav.channels, wav.frequency);
            MainThreadDispatcher.ExecuteInUpdate(() => OnSaveComplete());
            });
        saveThread.Start();
    }

    private void OnSaveComplete()
    {
        // 这里是音频保存完成后的处理，将在主线程上执行
        isDataReady = true;
        Debug.Log("Recording saved");
    }
}