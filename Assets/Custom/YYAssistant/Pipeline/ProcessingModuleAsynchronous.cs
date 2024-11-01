using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ProcessingModuleAsynchronous : ProcessingModule
{
    public override void StartProcessing()
    {
        base.StartProcessing();
        Task.Run(ProcessLoop); // 启动后台任务
    }

    private async Task ProcessLoop()
    {
        while (isProcessing)
        {
            await TryProcess();
            await Task.Delay(50); // 控制频率，避免资源占用过高
        }
    }

    public virtual async Task ProcessMessage(string message)
    {
        BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(message);
        await Task.Delay(100); // 模拟耗时操作
        baseMessage.signal = $"{name}_{index} processed: {baseMessage.signal}";
        string processedMessage = JsonUtility.ToJson(baseMessage);
        outputQueue.Add(processedMessage);
    }

    protected async Task TryProcess()
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

                await ProcessMessage(message);
            }
        }
    }
}
