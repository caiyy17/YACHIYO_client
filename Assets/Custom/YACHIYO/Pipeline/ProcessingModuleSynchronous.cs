using UnityEngine;
using System.Collections.Generic;

namespace Yachiyo
{
    public class ProcessingModuleSynchronous : ProcessingModule
    {
        private void Update()
        {
            if (isProcessing)
            {
                TryProcess();
            }
        }

        protected virtual void ProcessMessage(string message)
        {
        }

        protected virtual void CustomUpdate()
        {
        }

        protected void TryProcess()
        {
            CheckCancel();

            //接受部分
            string message;
            if (inputQueue.TryTake(out message))
            {
                YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);
                current_timestamp = baseMessage.timestamp;
                if (current_timestamp < cancel_timestamp)
                {
                    LogInfo($"discarding old data: {message}, cancel: {cancel_timestamp}, current: {current_timestamp}");
                    current_timestamp = null;
                }
                else
                {
                    LogInfo($"receive message: {message}");
                    if (baseMessage.destination == -2 || baseMessage.destination == index)
                    {
                        if (baseMessage.signal != "" && !capturedSignals.Contains(baseMessage.signal))
                        {
                            baseMessage.destination = -2;
                            message = JsonUtility.ToJson(baseMessage);
                            outputQueue.Add(message);
                            current_timestamp = null;
                            return;
                        }
                        LogInfo($"processing message: {message}");
                        try
                        {
                            ProcessMessage(message);
                        }
                        catch (System.Exception e)
                        {
                            LogError($"Processing error: {e.Message}");
                        }
                        current_timestamp = null;
                        return;
                    }
                    else
                    {
                        outputQueue.Add(message);
                        current_timestamp = null;
                    }
                }
            }

            //自主部分
            try
            {
                CustomUpdate();
            }
            catch (System.Exception e)
            {
                LogError($"CustomUpdate error: {e.Message}");
            }
        }
    }
}
