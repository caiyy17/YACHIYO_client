using UnityEngine;

public class PipelineTest : MonoBehaviour
{
    SignalManager signalManager;
    public int interval = 5;
    [TextArea]
    public string[] messages;
    int index;

    void Start()
    {
        index = 0;
        signalManager = GetComponent<SignalManager>();
    }

    void Update()
    {
        if (Time.time % interval < Time.deltaTime)
        {
            signalManager.SendSignal("enqueue_message", messages[index]);
            index++;
        }
        index = index % messages.Length;
    }
}