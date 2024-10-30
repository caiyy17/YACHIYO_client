using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
public class ProcessingModuleSynchronous : ProcessingModule
{
    public float parameter;

    public override void StartProcessing()
    {
        base.StartProcessing();
    }

    public virtual string ProcessMessage(string message)
    {
        return $"MonoBehaviour Processed: {message} with parameter {parameter}";
    }

    public override void StopProcessing()
    {
        base.StopProcessing();
    }

    private void Update()
    {
        string message;
        if (isProcessing)
        {
            while(cancelQueue.TryTake(out message))
            {
                CancelMessage cancelMessage = JsonUtility.FromJson<CancelMessage>(message);
                cancel_timestamp = cancelMessage.timestamp;
            }
            if (inputQueue.TryTake(out message))
            {
                BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
                if (baseMessage.timestamp < cancel_timestamp)
                {
                    Debug.Log($"message: {message}, time: {baseMessage.timestamp}, cancel: {cancel_timestamp}");
                }
                else
                {
                    var processedMessage = ProcessMessage(baseMessage.signal);
                    BaseMessage resultMessage = new BaseMessage
                    {
                        type = "message",
                        timestamp = baseMessage.timestamp,
                        signal = processedMessage,
                        destination = baseMessage.destination
                    };
                    outputQueue.Add(JsonUtility.ToJson(resultMessage));
                }
            }
        }
    }
}
