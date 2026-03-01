using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

[RequireComponent(typeof(ServiceRecorder))]
public class RecordingModule : ProcessingModuleSynchronous
{
    ServiceRecorder serviceRecorder;
    public float offset = -0.3f;
    [System.Serializable]
    class AudioMessage
    {
        public string audio_file = "";
    }
    void Awake()
    {
        moduleName = "RecordingModule";
        serviceRecorder = GetComponent<ServiceRecorder>();
    }

    protected override async Task CustomInit()
    {
        captuedSignals.Add("recording_start");
        captuedSignals.Add("recording_stop");
    }

    protected override void ProcessMessage(string message)
    {
        YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
        if (baseMessage.signal == "recording_start")
        {
            LogInfo("Recording Start");
            StopAllCoroutines();
            serviceRecorder.StartRecording(offset);
        }
        else if (baseMessage.signal == "recording_stop")
        {
            if (!serviceRecorder.isRecording)
            {
                LogInfo("Recording not started");
                return;
            }
            else
            {
                LogInfo("Recording Stop");
                StartCoroutine(SendAudio(baseMessage));
            }
        }
    }

    IEnumerator SendAudio(YYMessage baseMessage)
    {
        serviceRecorder.StopRecording();
        while (!serviceRecorder.isDataReady)
        {
            yield return null;
        }
        YYMessage resultMessage = new YYMessage();
        resultMessage.signal = "";
        resultMessage.timestamp = baseMessage.timestamp;
        AudioMessage audio = new AudioMessage();
        string audio_file = Convert.ToBase64String(serviceRecorder.wavData);
        audio.audio_file = audio_file;
        string content = JsonUtility.ToJson(audio);
        resultMessage.content = content;
        AddDestination(ref resultMessage);
        string message = JsonUtility.ToJson(resultMessage);
        outputQueue.Add(message);
        yield break;
    }
}