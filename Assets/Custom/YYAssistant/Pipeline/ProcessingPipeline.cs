using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class ProcessingPipeline : MonoBehaviour
{
    public List<ProcessingModule> modules = new List<ProcessingModule>();

    private List<BlockingCollection<string>> queues = new List<BlockingCollection<string>>();
    private List<BlockingCollection<string>> cancelQueues = new List<BlockingCollection<string>>();

    SignalManager signalManager;

    bool isStarted = false;

    private async void Start()
    {
        signalManager = GetComponent<SignalManager>();
        // 为模块链创建队列，并将队列串联起来
        for (int i = 0; i <= modules.Count; i++)
        {
            queues.Add(new BlockingCollection<string>());
            cancelQueues.Add(new BlockingCollection<string>());
        }

        // 初始化并启动所有模块
        for (int i = 0; i < modules.Count; i++)
        {
            await modules[i].Initialize(queues[i], queues[i + 1], queues[queues.Count - 1], cancelQueues[i], i);
            await modules[i].StartProcessing(); // 启动模块
        }

        signalManager.AddSignal("enqueue_message", EnqueueMessage);
        Debug.Log("Processing pipeline started.");
        isStarted = true;
    }

    void Update(){

        // 获取最后一个模块的输出结果
        if(TryGetProcessedMessage(out string message)){
            Debug.Log($"Processed message: {message}");
        }
    }

    private void OnDisable()
    {
        // 停止所有模块的处理
        foreach (var module in modules)
        {
            module.StopProcessing();
        }
    }

    // 示例：向第一个模块的输入队列添加消息
    public void EnqueueMessage(string message)
    {
        message = message.Replace("\"timestamp\":TIME_STAMP", $"\"timestamp\":{CustomFunctions.GetUnixTime()}");
        Debug.Log($"Enqueue message: {message}");
        queues[0].Add(message);
    }

    // 获取最后一个模块的输出结果
    public bool TryGetProcessedMessage(out string message)
    {
        message = null;
        if (queues[queues.Count - 1].TryTake(out message))
        {
            return true;
        }
        return false;
    }
}
