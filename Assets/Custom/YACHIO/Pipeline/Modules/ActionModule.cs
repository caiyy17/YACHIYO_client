using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Pipeline module that handles action events.
/// Receives action data (signal="") from ContentModule via pipeline,
/// and action_set from SignalManager (e.g. yya_state → action_set route).
/// </summary>
[RequireComponent(typeof(ActionLoader))]
public class ActionModule : ProcessingModuleSynchronous
{
    ActionLoader actionLoader;
    SignalManager signalManager;

    void Awake()
    {
        moduleName = "ActionModule";
        captuedSignals = new System.Collections.Generic.List<string> { "SoS", "EoS" };
        actionLoader = GetComponent<ActionLoader>();
        signalManager = FindObjectOfType<SignalManager>();
    }

    void Start()
    {
        if (signalManager != null)
        {
            signalManager.AddSignal("action_set", OnActionSet);
        }
    }

    void OnDisable()
    {
        if (signalManager != null)
        {
            signalManager.RemoveSignal("action_set", OnActionSet);
        }
    }

    protected override void ProcessMessage(string message)
    {
        YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

        if (baseMessage.signal == "SoS")
        {
            actionLoader.SetAction("thinking");
            outputQueue.Add(message);
            return;
        }

        if (baseMessage.signal == "EoS")
        {
            actionLoader.SetAction("idle");
            outputQueue.Add(message);
            return;
        }

        if (string.IsNullOrEmpty(baseMessage.content)) return;

        // Consume action field, forward the rest
        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
        if (jsonDict.ContainsKey("action"))
        {
            actionLoader.SetAction(jsonDict["action"].ToString());
            jsonDict.Remove("action");
        }

        if (jsonDict.Count > 0)
        {
            baseMessage.content = JsonConvert.SerializeObject(jsonDict);
            outputQueue.Add(JsonUtility.ToJson(baseMessage));
        }
    }

    // Called by SignalManager via yya_state → action_set route
    void OnActionSet(string message)
    {
        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
        if (jsonDict.ContainsKey("action"))
        {
            actionLoader.SetAction(jsonDict["action"].ToString());
        }
    }
}
