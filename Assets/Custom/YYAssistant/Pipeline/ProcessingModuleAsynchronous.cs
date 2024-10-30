using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        string message;
        while (isProcessing)
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
