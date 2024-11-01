using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
public class ProcessingModuleSynchronous : ProcessingModule
{
    private void Update()
    {
        if (isProcessing)
        {
            TryProcess();
        }
    }

    public virtual void ProcessMessage(string message)
    {
        BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
        baseMessage.signal = $"{name}_{index} processed: {baseMessage.signal}";
        string processedMessage = JsonUtility.ToJson(baseMessage);
        outputQueue.Add(processedMessage);
    }

    protected void TryProcess()
    {
        CheckCancel();
        string message;
        if (inputQueue.TryTake(out message))
        {
            BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
            if (baseMessage.timestamp < cancel_timestamp)
            {
                LogInfo($"discarding old data: {message}");
                CustomCancel();
            }
            else
            {
                if (baseMessage.destination != -2 && baseMessage.destination != index)
                {
                    outputQueue.Add(message);
                    return;
                }

                if (!string.IsNullOrEmpty(baseMessage.signal) && !captuedSignals.Contains(baseMessage.signal))
                {
                    baseMessage.destination = -2;
                    message = JsonUtility.ToJson(baseMessage);
                    outputQueue.Add(message);
                    return;
                }

                ProcessMessage(message);
            }
        }
    }
}
