using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProcessingModuleAsynchronous : ProcessingModule
{
    public float parameter;

    public override void StartProcessing()
    {
        base.StartProcessing();
        Task.Run(ProcessLoop); // 启动后台任务
    }

    private async Task ProcessLoop()
    {
        while (isProcessing)
        {
            if (inputQueue.Count > 0)
            {
                var message = inputQueue.Dequeue();
                var processedMessage = ProcessMessage(message);
                outputQueue.Enqueue(processedMessage);
            }
            await Task.Delay(50); // 控制频率，避免资源占用过高
        }
    }

    public virtual string ProcessMessage(string message)
    {
        return $"ScriptableObject Processed: {message} with parameter {parameter}";
    }

    public override void StopProcessing()
    {
        base.StopProcessing();
        
    }
}
