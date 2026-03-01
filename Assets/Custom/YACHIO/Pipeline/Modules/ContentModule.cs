using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Pipeline module that handles text display and action forwarding.
/// Captures SoS to clear text. Non-captured signals (EoS etc.) auto-forward
/// via ProcessingModuleSynchronous to outputQueue → DistributeMessage.
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

        // Text/action message from AudioModule (signal == "")
        if (baseMessage.content == "") return;

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);

        // Consume text display fields
        string text = jsonDict.ContainsKey("text") ? jsonDict["text"].ToString() : "";
        string actionHint = jsonDict.ContainsKey("action_hint") ? jsonDict["action_hint"].ToString() : "";

        if (!string.IsNullOrEmpty(actionHint))
        {
            contentLoader.AddText($"[{actionHint}]{text}");
        }
        else if (!string.IsNullOrEmpty(text))
        {
            contentLoader.AddText(text);
        }

        // Remove consumed fields, forward the rest
        jsonDict.Remove("text");
        jsonDict.Remove("action_hint");

        if (jsonDict.Count > 0)
        {
            baseMessage.content = JsonConvert.SerializeObject(jsonDict);
            outputQueue.Add(JsonUtility.ToJson(baseMessage));
        }
    }

    protected override void CustomCancel(string message)
    {
        contentLoader.LoadText("");
    }
}
