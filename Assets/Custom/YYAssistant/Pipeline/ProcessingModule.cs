using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProcessingModule : MonoBehaviour
{
    protected BlockingCollection<string> inputQueue;
    protected BlockingCollection<string> outputQueue;
    protected BlockingCollection<string> sendQueue;
    protected BlockingCollection<string> cancelQueue;
    protected bool isProcessing = false;

    protected double cancel_timestamp = 0;

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
        public double timestamp = 0;
        public string signal = null;
        public int destination = 0;
    }

    public virtual void Initialize(BlockingCollection<string> input, BlockingCollection<string> output, BlockingCollection<string> send, BlockingCollection<string> cancel)
    {
        inputQueue = input;
        outputQueue = output;
        sendQueue = send;
        cancelQueue = cancel;
        Debug.Log("ProcessingModule initialized.");
    }
    public virtual void StartProcessing()
    {
        // 默认实现，具体模块可以重写
        isProcessing = true;
        Debug.Log("ProcessingModule started.");
    }
    public virtual void StopProcessing()
    {
        // 默认实现，具体模块可以重写
        isProcessing = false;
        Debug.Log("ProcessingModule stopped.");
    }
    
    protected void CheckCancel()
    {
        string message;
        while(cancelQueue.TryTake(out message))
        {
            CancelMessage cancelMessage = JsonUtility.FromJson<CancelMessage>(message);
            Debug.Log($"Cancel timestamp: {cancelMessage.timestamp}");
            cancel_timestamp = cancelMessage.timestamp;
        }
    }
}
