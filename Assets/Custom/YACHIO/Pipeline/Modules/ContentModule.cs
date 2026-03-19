using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Pipeline module that displays content and forwards message as-is.
/// Captures SoS to clear text, EoS forwarded directly.
/// </summary>
[RequireComponent(typeof(ContentLoader))]
public class ContentModule : ProcessingModuleSynchronous
{
    ContentLoader contentLoader;

    void Awake()
    {
        moduleName = "ContentModule";
        captuedSignals = new System.Collections.Generic.List<string> { "SoS", "EoS" };
        contentLoader = GetComponent<ContentLoader>();
    }

    protected override void ProcessMessage(string message)
    {
        YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

        if (baseMessage.signal == "SoS")
        {
            contentLoader.LoadText("");
            outputQueue.Add(message);
            return;
        }

        if (baseMessage.signal == "EoS")
        {
            outputQueue.Add(message);
            return;
        }

        if (!string.IsNullOrEmpty(baseMessage.content))
        {
            // Filter out empty fields for display
            var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
            var nonEmpty = jsonDict.Where(kv => kv.Value != null && kv.Value.ToString() != "").ToDictionary(kv => kv.Key, kv => kv.Value);
            if (nonEmpty.Count > 0)
            {
                contentLoader.AddText(JsonConvert.SerializeObject(nonEmpty));
            }
            outputQueue.Add(message);
        }
    }

    protected override void CustomCancel(string message)
    {
        contentLoader.LoadText("");
    }
}
