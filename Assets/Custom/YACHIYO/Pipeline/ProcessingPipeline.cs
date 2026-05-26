using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using Newtonsoft.Json;

namespace Yachiyo
{
    // -2是默认值，表示送至下一个节点
    // -1表示直接跳过，直至输出节点
    [System.Serializable]
    public class YYMessage
    {
        public string signal = "";
        public string content = "";
        public int destination = -2;
        public double timestamp = 0;
    }

    public class ProcessingPipeline : MonoBehaviour
    {
        public List<ProcessingModule> modules = new List<ProcessingModule>();
        public SignalEvent sendSignal = new SignalEvent();
        public double cancelOffset = -0.001;

        private List<BlockingCollection<string>> queues = new List<BlockingCollection<string>>();
        private List<BlockingCollection<string>> cancelQueues = new List<BlockingCollection<string>>();

        bool isStarted = false;
        bool errorInStart = false;

        [System.Serializable]
        public class DataRouter
        {
            public List<string> markers;
            public string eventName;
        }
        public List<DataRouter> dataRouters = new List<DataRouter>();

        protected void LogInfo(string message)
        {
            Debug.Log($"Pipeline: {message}");
        }

        private async void Start()
        {
            // 为模块链创建队列，并将队列串联起来
            for (int i = 0; i <= modules.Count; i++)
            {
                queues.Add(new BlockingCollection<string>());
                cancelQueues.Add(new BlockingCollection<string>());
            }

            // 初始化并启动所有模块
            for (int i = 0; i < modules.Count; i++)
            {
                await modules[i].Initialize(queues[i], queues[i + 1], queues[queues.Count - 1], cancelQueues[i], i);
                if (!modules[i].IsProcessing())
                {
                    LogInfo($"Module {i} failed to start.");
                    errorInStart = true;
                    return;
                }
            }
            LogInfo("Started.");
            isStarted = true;
        }

        public bool IsStarted()
        {
            return isStarted;
        }

        public bool ErrorInStart()
        {
            return errorInStart;
        }

        private double cancel_timestamp = 0;

        private void LateUpdate()
        {
            float start = Time.realtimeSinceStartup;
            CheckTailCancel();

            if (TryGetProcessedMessage(out string message))
            {
                YYMessage inputMessage = JsonUtility.FromJson<YYMessage>(message);

                // cancel signal from module sendQueue: forward to head for distribution
                if (inputMessage.signal == "cancel")
                {
                    LogInfo($"Cancel received at tail, forwarding to head: {message}");
                    EnqueueMessage(message);
                    return;
                }

                // Discard stale messages
                if (inputMessage.timestamp < cancel_timestamp)
                {
                    LogInfo($"Discard stale output: {message}, cancel: {cancel_timestamp}");
                    return;
                }

                Dictionary<string, object> jsonDict = new Dictionary<string, object>();
                if (inputMessage.content != "")
                {
                    jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(inputMessage.content);
                }
                jsonDict["signal"] = inputMessage.signal;
                jsonDict["timestamp"] = inputMessage.timestamp;
                string modified_message = JsonConvert.SerializeObject(jsonDict);
                LogInfo($"Output: {modified_message}");
                DistributeMessage(modified_message);
            }
            float ms = (Time.realtimeSinceStartup - start) * 1000f;
            if (ms > 30f)
                LogInfo($"[Perf] LateUpdate took {ms:F1}ms");
        }

        private void OnDisable()
        {
            // 停止所有模块的处理
            foreach (var module in modules)
            {
                module.StopProcessing();
            }
        }

        // 示例：向第一个模块的输入队列添加消息

        public void EnqueueMessage(string message)
        {
            if (!isStarted)
            {
                LogInfo($"Pipeline not ready, discarding: {message}");
                return;
            }

            YYMessage inputMessage = JsonUtility.FromJson<YYMessage>(message);

            if (inputMessage.signal == "cancel")
            {
                // Cancel: timestamp <= 0 means "now", offset -0.001 to not cancel concurrent events
                if (inputMessage.timestamp <= 0)
                    inputMessage.timestamp = CustomFunctions.GetUnixTime();
                inputMessage.timestamp += cancelOffset;
                message = JsonUtility.ToJson(inputMessage);
                LogInfo($"input: {message}");
                for (int i = 0; i < cancelQueues.Count; i++)
                {
                    cancelQueues[i].Add(message);
                }
            }
            else
            {
                inputMessage.timestamp = CustomFunctions.GetUnixTime();
                message = JsonUtility.ToJson(inputMessage);
                LogInfo($"input: {message}");
                queues[0].Add(message);
            }
        }

        private void CheckTailCancel()
        {
            BlockingCollection<string> tailCancelQueue = cancelQueues[cancelQueues.Count - 1];
            while (tailCancelQueue.TryTake(out string cancelMsg))
            {
                YYMessage cm = JsonUtility.FromJson<YYMessage>(cancelMsg);
                cancel_timestamp = System.Math.Max(cancel_timestamp, cm.timestamp);
            }
        }

        // 获取最后一个模块的输出结果
        private bool TryGetProcessedMessage(out string message)
        {
            return queues[queues.Count - 1].TryTake(out message);
        }

        private void DistributeMessage(string message)
        {
            foreach (DataRouter router in dataRouters)
            {
                foreach (string marker in router.markers)
                {
                    if (message.Contains(marker))
                    {
                        if (sendSignal != null)
                        {
                            sendSignal.Invoke(router.eventName, message);
                        }
                    }
                }
            }
        }
    }
}
