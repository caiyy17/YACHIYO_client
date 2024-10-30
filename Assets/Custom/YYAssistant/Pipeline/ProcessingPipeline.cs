using System.Collections.Generic;
using UnityEngine;

public class ProcessingPipeline : MonoBehaviour
{
    public List<ProcessingModule> modules = new List<ProcessingModule>();

    private List<Queue<string>> queues = new List<Queue<string>>();
    private List<Queue<string>> cancelQueues = new List<Queue<string>>();

    private void Start()
    {
        // 为模块链创建队列，并将队列串联起来
        for (int i = 0; i <= modules.Count; i++)
        {
            queues.Add(new Queue<string>());
            cancelQueues.Add(new Queue<string>());
        }

        // 初始化并启动所有模块
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Initialize(queues[i], queues[i + 1], queues[queues.Count - 1], cancelQueues[i]);
            modules[i].StartProcessing(); // 启动模块
            Debug.Log($"Module {i} started.");
        }

        Debug.Log("Processing pipeline started.");
    }

    void Update(){
        // 每隔5秒向第一个模块的输入队列添加消息
        if(Time.time % 5 < Time.deltaTime){
            Debug.Log("Enqueue message.");
            EnqueueMessage("Message from ProcessingPipeline");
        }

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
        queues[0].Enqueue(message);
    }

    // 获取最后一个模块的输出结果
    public bool TryGetProcessedMessage(out string message)
    {
        if (queues[queues.Count - 1].Count > 0)
        {
            message = queues[queues.Count - 1].Dequeue();
            return true;
        }
        message = null;
        return false;
    }
}
