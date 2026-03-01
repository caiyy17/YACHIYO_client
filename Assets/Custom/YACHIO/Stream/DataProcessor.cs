using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

[RequireComponent(typeof(WebSocketClient))]
public class DataProcessor : MonoBehaviour
{
    WebSocketClient wsClient;
    [System.Serializable]
    public class DataRouter
    {
        public List<string> markers;
        public StringEvent eventHandler;
    }
    public List<DataRouter> dataRouters = new List<DataRouter>();

    void Awake()
    {
        wsClient = GetComponent<WebSocketClient>();
    }

    void Update()
    {
        if (wsClient.TryGetReceivedMessage(out string message))
        {
            ProcessData(message);
        }
    }
    public void ProcessData(string segment)
    {
        foreach (DataRouter router in dataRouters)
        {
            foreach (string marker in router.markers)
            {
                if (segment.Contains(marker))
                {
                    if (router.eventHandler != null)
                    {
                        router.eventHandler.Invoke(segment);
                    }
                }
            }
        }
    }
}