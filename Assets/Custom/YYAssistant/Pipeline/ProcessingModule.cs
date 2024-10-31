using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProcessingModule : MonoBehaviour
{
    protected string moduleName = "Module";
    protected BlockingCollection<string> inputQueue;
    protected BlockingCollection<string> outputQueue;
    protected BlockingCollection<string> sendQueue;
    protected BlockingCollection<string> cancelQueue;
    protected int index;
    protected bool isProcessing = false;
    protected double cancel_timestamp = 0;

    public List<int> destinationIndices = new List<int>();
    public List<string> captuedSignals = new List<string>();
    
    [System.Serializable]
    protected class CancelMessage
    {
        public string type = "cancel";
        public double timestamp = 0;
    }

    [System.Serializable]
    protected class BaseMessage
    {
        public string type = "message";
        public string message = "";
        public double timestamp = 0;
        public string signal = "";
        public int destination = -1;
    }

    protected void LogInfo(string message)
    {
        Debug.Log($"{moduleName}_{index}: {message}");
    }

    public void Initialize(BlockingCollection<string> input, 
    BlockingCollection<string> output, 
    BlockingCollection<string> send, 
    BlockingCollection<string> cancel, 
    int index)
    {
        inputQueue = input;
        outputQueue = output;
        sendQueue = send;
        cancelQueue = cancel;
        this.index = index;
        CustomInit();
        LogInfo("ProcessingModule initialized.");
    }

    protected virtual void CustomInit()
    {
        // 可以在子类中重写
    }

    public virtual void StartProcessing()
    {
        // 默认实现，具体模块可以重写
        isProcessing = true;
        LogInfo("ProcessingModule started.");
    }
    public virtual void StopProcessing()
    {
        // 默认实现，具体模块可以重写
        isProcessing = false;
        LogInfo("ProcessingModule stopped.");
    }

    public virtual string ProcessMessage(string message)
    {
        BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
        baseMessage.signal = $"{name}_{index} processed: {baseMessage.signal}";
        message = JsonUtility.ToJson(baseMessage);
        return message;
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
                if (baseMessage.destination != -1 && baseMessage.destination != index)
                {
                    outputQueue.Add(message);
                    return;
                }

                if (!string.IsNullOrEmpty(baseMessage.signal) && !captuedSignals.Contains(baseMessage.signal))
                {
                    baseMessage.destination = -1;
                    message = JsonUtility.ToJson(baseMessage);
                    outputQueue.Add(message);
                    return;
                }

                var processedMessage = ProcessMessage(message);
                outputQueue.Add(processedMessage);
            }
        }
    }
    
    protected void CheckCancel()
    {
        string message;
        while(cancelQueue.TryTake(out message))
        {
            CancelMessage cancelMessage = JsonUtility.FromJson<CancelMessage>(message);
            LogInfo($"received cancel signal: {cancelMessage.timestamp}");
            cancel_timestamp = cancelMessage.timestamp;
        }
    }

    protected virtual void CustomCancel(){
        // 可以在子类中重写
    }

    protected void AddDestination(ref BaseMessage message, int destination_index = 0)
    {
        if(destination_index < 0 || destinationIndices.Count <= destination_index)
        {
            message.destination = -1;
        }
        else{
            message.destination = destinationIndices[destination_index];
        }
    }
}
