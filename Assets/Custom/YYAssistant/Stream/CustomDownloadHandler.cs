using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class CustomDownloadHandler : DownloadHandlerScript
{
    // 临时存储接收到的数据
    private StringBuilder receivedData = new StringBuilder();
    private DataFetcher dataFetcher;

    //传入continueFetching的引用
    public CustomDownloadHandler(DataFetcher dataFetcher)
    {
        this.dataFetcher = dataFetcher;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0)
            return false;

        if (dataFetcher.cancellationToken) // 检查中断标志
        {
            Debug.Log("Receiving data cancelled by user.");
            return false;
        }

        string text = Encoding.UTF8.GetString(data, 0, dataLength);
        receivedData.Append(text);
        ProcessData();
        return true;
    }

    private void ProcessData()
    {
        string data = receivedData.ToString();
        int index;
        // 检查是否包含分隔符（此处假设使用换行符）
        while ((index = data.IndexOf('\n')) != -1)
        {
            // 提取一段完整的数据
            string segment = data.Substring(0, index);
            data = data.Substring(index + 1);
            receivedData = new StringBuilder(data); // 更新剩余数据

            if (!string.IsNullOrEmpty(segment))
            {
                // 在这里处理每段数据，例如解析JSON
                Debug.Log("Received segment: " + segment);
                dataFetcher.segmentEvent.Invoke(segment);
            }
        }
    }
}