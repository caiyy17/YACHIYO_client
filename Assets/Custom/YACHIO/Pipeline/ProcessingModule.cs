using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    protected double? current_timestamp = null;

    public List<int> destinationIndices = new List<int>();
    public List<string> captuedSignals = new List<string>();
    // [System.Serializable]
    // public class message_vars
    // {
    //     public List<string> input_vars = new List<string>();
    //     public List<string> output_vars = new List<string>();
    // }
    // public List<message_vars> inputVars = new List<message_vars>();
    // public List<message_vars> passVars = new List<message_vars>();
    // public List<message_vars> outputVars = new List<message_vars>();

    protected void LogInfo(string message)
    {
        Debug.Log($"{moduleName}_{index}: {message}");
    }

    protected void LogError(string message)
    {
        Debug.LogError($"{moduleName}_{index}: {message}");
    }

    public async Task Initialize(BlockingCollection<string> input,
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
        isProcessing = true;
        await CustomInit();
        LogInfo("ProcessingModule initialized.");
    }

    protected virtual async Task CustomInit()
    {
        // 可以在子类中重写
    }

    public void StopProcessing()
    {
        isProcessing = false;
        LogInfo("ProcessingModule stopped.");
        CustomStop();
    }

    protected virtual void CustomStop()
    {
        // 可以在子类中重写
    }

    protected bool CheckCancel()
    {
        bool hasCancel = false;
        string message;
        string content = "";
        while (cancelQueue.TryTake(out message))
        {
            YYMessage cancelMessage = JsonUtility.FromJson<YYMessage>(message);
            LogInfo($"received cancel signal: {cancelMessage.timestamp}");
            cancel_timestamp = System.Math.Max(cancel_timestamp, cancelMessage.timestamp);
            content = cancelMessage.content;
            CustomCancel(content);
            if (current_timestamp != null && current_timestamp < cancel_timestamp)
            {
                LogInfo($"cancel signal newer than current data, triggered: {content}, cancel: {cancel_timestamp}, current: {current_timestamp}");
                hasCancel = true;
            }
        }
        return hasCancel;
    }

    protected virtual void CustomCancel(string message)
    {
        // 可以在子类中重写
    }

    protected void AddDestination(ref YYMessage message, int destination_index = 0)
    {
        if (destination_index <= -3)
        {
            LogError("Invalid destination index: " + destination_index);
            message.destination = -2;
            return;
        }
        if (destination_index == -2)
        {
            message.destination = -2;
            return;
        }
        else if (destination_index == -1)
        {
            message.destination = -1;
            return;
        }

        if (destinationIndices.Count > 0)
        {
            if (destination_index < destinationIndices.Count)
            {
                message.destination = destinationIndices[destination_index];
            }
            else
            {
                LogError("Invalid destination index: " + destination_index);
                message.destination = -2;
            }
        }
        else
        {
            message.destination = -2;
        }
    }

    public bool IsProcessing()
    {
        return isProcessing;
    }
}
