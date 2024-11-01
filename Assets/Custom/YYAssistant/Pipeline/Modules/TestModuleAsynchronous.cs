using UnityEngine;
using System.Threading.Tasks;

public class TestModuleAsynchronous : ProcessingModuleAsynchronous
{
    public string prefix = "Asynchronous: "; // 额外的文本处理参数
    public string parameter = "0"; // 额外的文本处理参数
    public int delay = 100; // 模拟耗时操作的延迟时间
    [System.Serializable]
    class CustomMessage : BaseMessage
    {
        public string customField = "";
    }
    void Awake()
    {
        moduleName = "TestModuleAsynchronous";
    }
    public override async Task ProcessMessage(string message)
    {
        CustomMessage customMessage = JsonUtility.FromJson<CustomMessage>(message);
        await Task.Delay(delay); // 模拟耗时操作
        customMessage.message = $"{prefix}{customMessage.message} with parameter {parameter}";
        BaseMessage baseMessage = customMessage;
        AddDestination(ref baseMessage);
        message = JsonUtility.ToJson(baseMessage);
        outputQueue.Add(message);
    }
}