using UnityEngine;
using System.Collections;

public class WebSocketClientModule : ProcessingModuleSynchronous
{
    double timestamp = 0;
    [System.Serializable]
    class CustomMessage : BaseMessage
    {
        public string audio_data = "";
    }
    void Awake()
    {
        moduleName = "WebSocketClientModule";
    }

    void Start()
    {
    }

    public override void ProcessMessage(string message)
    {
    }
}