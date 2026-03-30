using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Yachiyo
{
    /// <summary>
    /// Pipeline module that consumes a single configured field from messages
    /// and invokes a UnityEvent. Mount multiple instances for multiple fields.
    /// </summary>
    public class ActionModule : ProcessingModuleSynchronous
    {
        [SerializeField] private string fieldName = "action";
        [SerializeField] private StringEvent onValue;
        [SerializeField] private string sosValue;
        [SerializeField] private string eosValue;

        void Awake()
        {
            moduleName = $"ActionModule({fieldName})";
            capturedSignals = new List<string> { "SoS", "EoS" };
        }

        protected override void ProcessMessage(string message)
        {
            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

            if (baseMessage.signal == "SoS")
            {
                if (!string.IsNullOrEmpty(sosValue) && onValue != null)
                    onValue.Invoke(sosValue);
                outputQueue.Add(message);
                return;
            }

            if (baseMessage.signal == "EoS")
            {
                if (!string.IsNullOrEmpty(eosValue) && onValue != null)
                    onValue.Invoke(eosValue);
                outputQueue.Add(message);
                return;
            }

            if (string.IsNullOrEmpty(baseMessage.content)) return;

            Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
            if (jsonDict.ContainsKey(fieldName))
            {
                string value = jsonDict[fieldName]?.ToString();
                if (!string.IsNullOrEmpty(value) && onValue != null)
                    onValue.Invoke(value);
                jsonDict.Remove(fieldName);
            }

            if (jsonDict.Count > 0)
            {
                baseMessage.content = JsonConvert.SerializeObject(jsonDict);
                outputQueue.Add(JsonUtility.ToJson(baseMessage));
            }
        }
    }
}
