using System.Collections.Generic;
using UnityEngine;

public abstract class ProcessingModule : MonoBehaviour
{
    protected Queue<string> inputQueue;
    protected Queue<string> outputQueue;
    protected Queue<string> sendQueue;
    protected Queue<string> cancelQueue;
    protected bool isProcessing;
    public virtual void Initialize(Queue<string> input, Queue<string> output, Queue<string> send, Queue<string> cancel)
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
}
