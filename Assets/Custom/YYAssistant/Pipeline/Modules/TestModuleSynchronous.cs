using UnityEngine;

public class TestModuleSynchronous : ProcessingModuleSynchronous
{
    public string prefix = "Synchronous: "; // 额外的文本处理参数
    public string parameter = "0"; // 额外的文本处理参数
    [System.Serializable]
    class CustomMessage : BaseMessage
    {
        public string customField = "";
    }

    void Awake()
    {
        moduleName = "TestModuleSynchronous";
    }

    public override void ProcessMessage(string message)
    {
        CustomMessage customMessage = JsonUtility.FromJson<CustomMessage>(message);
        customMessage.message = $"{prefix}{customMessage.message} with parameter {parameter}";
        BaseMessage baseMessage = customMessage;
        AddDestination(ref baseMessage);
        message = JsonUtility.ToJson(baseMessage);
        outputQueue.Add(message);
    }
}