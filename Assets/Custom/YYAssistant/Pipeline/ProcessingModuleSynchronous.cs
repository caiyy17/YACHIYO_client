using UnityEngine;
using System.Collections.Generic;

public class ProcessingModuleSynchronous : ProcessingModule
{
    public float parameter;

    public override void StartProcessing()
    {
        base.StartProcessing();
    }

    public virtual string ProcessMessage(string message)
    {
        return $"MonoBehaviour Processed: {message} with parameter {parameter}";
    }

    public override void StopProcessing()
    {
        base.StopProcessing();
    }

    private void Update()
    {
        if (isProcessing)
        {
            while(cancelQueue.Count > 0)
            {
                var cancelMessage = cancelQueue.Dequeue();
            }
            if (inputQueue.Count > 0)
            {
                var message = inputQueue.Dequeue();
                var processedMessage = ProcessMessage(message);
                outputQueue.Enqueue(processedMessage);
            }
        }
    }
}
