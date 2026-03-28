using UnityEngine;

namespace Yachiyo
{
    public class TestModule : ProcessingModuleSynchronous
    {
        public string prefix = "Synchronous: "; // 额外的文本处理参数
        public string parameter = "0"; // 额外的文本处理参数
        [System.Serializable]
        class CustomMessage
        {
            public string customField = "";
        }

        void Awake()
        {
            moduleName = "TestModuleSynchronous";
        }

        protected override void ProcessMessage(string message)
        {
            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
            CustomMessage customMessage = JsonUtility.FromJson<CustomMessage>(baseMessage.content);
            customMessage.customField = $"{prefix}{customMessage.customField} with parameter {parameter}";
            baseMessage.content = JsonUtility.ToJson(customMessage);
            AddDestination(ref baseMessage);
            message = JsonUtility.ToJson(baseMessage);
            outputQueue.Add(message);
        }
    }
}
