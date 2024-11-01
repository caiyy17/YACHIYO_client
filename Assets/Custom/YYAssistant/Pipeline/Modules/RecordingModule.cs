using System;
using UnityEngine;
using System.Collections;

public class RecordingModule : ProcessingModuleSynchronous
{
    ServiceRecorder serviceRecorder;
    double timestamp = 0;
    [System.Serializable]
    class CustomMessage : BaseMessage
    {
        public string audio_data = "";
    }
    void Awake()
    {
        moduleName = "RecordingModule";
    }

    void Start()
    {
        serviceRecorder = GetComponent<ServiceRecorder>();
    }

    public override void ProcessMessage(string message)
    {
        BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
        if (baseMessage.signal == "recording_start")
        {
            LogInfo("Recording Start");
            serviceRecorder.StartRecording();
            timestamp = baseMessage.timestamp;
        }
        else if (baseMessage.signal == "recording_stop")
        {
            if (cancel_timestamp > timestamp 
            || !serviceRecorder.isRecording
            || baseMessage.timestamp - timestamp > serviceRecorder.maxRecordingTime)
            {
                LogInfo("Recording Cancelled");
                return;
            }
            else
            {
                LogInfo("Recording Stop");
                StartCoroutine(SendAudio(baseMessage));
            }
        }
    }

    IEnumerator SendAudio(BaseMessage baseMessage){
        serviceRecorder.StopRecording();
        while(!serviceRecorder.isDataReady){
            yield return null;
        }
        CustomMessage customMessage = new CustomMessage();
        customMessage.timestamp = baseMessage.timestamp;
        string audio_file = Convert.ToBase64String(serviceRecorder.wavData);
        customMessage.audio_data = audio_file;
        BaseMessage resultMessage = customMessage;
        AddDestination(ref resultMessage);
        string message = JsonUtility.ToJson(resultMessage);
        outputQueue.Add(message);
        yield break;
    }
}