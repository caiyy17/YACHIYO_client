using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

[RequireComponent(typeof(KeywordDetector))]
public class KWSModule : ProcessingModuleSynchronous
{
    KeywordDetector keywordDetector;
    public bool useKWS = false;
    void Awake()
    {
        moduleName = "KWSModule";
        keywordDetector = GetComponent<KeywordDetector>();
    }

    protected override async Task CustomInit()
    {
        captuedSignals.Add("kws_start");
        captuedSignals.Add("kws_stop");
        keywordDetector.SetKWS(useKWS);
    }

    protected override void ProcessMessage(string message)
    {
        YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
        if (baseMessage.signal == "kws_start")
        {
            LogInfo("kws_start");
            keywordDetector.SetKWS(true);
        }
        else if (baseMessage.signal == "kws_stop")
        {
            LogInfo("kws_stop");
            keywordDetector.SetKWS(false);
        }
    }

    protected override void CustomUpdate()
    {
        if (keywordDetector.IsDetected())
        {
            LogInfo("KWS detected");
            YYMessage baseMessage = new YYMessage();
            double ts = CustomFunctions.GetUnixTime();
            current_timestamp = ts;
            baseMessage.signal = "kws_detected";
            baseMessage.timestamp = ts;
            AddDestination(ref baseMessage);
            string message = JsonUtility.ToJson(baseMessage);
            sendQueue.Add(message);
            current_timestamp = null;
        }
    }
}