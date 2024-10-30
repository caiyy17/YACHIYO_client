using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket webSocket = null;
    private CancellationTokenSource cts = null;
    public string serverUrl = "ws://localhost:8000";  // WebSocket服务器地址
    public string clientId = "test-id-1";  // 客户端ID
    public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;

    // Queues for sending and receiving messages
    private ConcurrentQueue<string> sendQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<string> receiveQueue = new ConcurrentQueue<string>();

    private double cancelTimeStamp = 0;

    public async Task Connect()
    {
        serverUrl = PlayerPrefs.GetString("urlInput", serverUrl);
        clientId = PlayerPrefs.GetString("userId", clientId);
        // WebSocket连接地址，需要将client_id作为路径参数发送
        string webSocketUrl = $"ws://{this.serverUrl}/ws/{clientId}";
        await ConnectWebSocket(webSocketUrl);
        cancelTimeStamp = CustomFunctions.GetUnixTime();
    }

    // 连接WebSocket服务器
    public async Task ConnectWebSocket(string uri)
    {
        webSocket = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            // 尝试连接到WebSocket服务器
            Task connectTask = webSocket.ConnectAsync(new Uri(uri), cts.Token);
            Task timeoutTask = Task.Delay(20000);

            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                Debug.LogError("WebSocket connection timeout.");
                webSocket.Dispose();
                return;
            }

            // 检查 WebSocket 连接状态
            if (webSocket.State == WebSocketState.Open)
            {
                Debug.Log("WebSocket connected!");
            }
            else
            {
                Debug.LogError("WebSocket connection failed or was rejected by the server.");
                webSocket.Dispose(); // 确保在连接失败时释放资源
            }

            // 启动接收和发送消息
            // await Task.WhenAll(ReceiveMessages(), ProcessSendQueue());
            _ = ReceiveMessages();    // 开启接收消息的异步任务
            _ = ProcessSendQueue();   // 开启发送消息的异步任务
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket error: {e.Message}");
        }
    }

    // 外部类调用此方法向WebSocket发送消息
    public void SendMessageToServer(string message)
    {
        sendQueue.Enqueue(message);  // 将消息加入发送队列
    }

    // 获取接收到的消息，外部类调用此方法处理消息
    public bool TryGetReceivedMessage(out string message)
    {
        return receiveQueue.TryDequeue(out message);  // 从接收队列中取出消息
    }

    // 处理发送队列中的消息
    private async Task ProcessSendQueue()
    {
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                if (sendQueue.TryDequeue(out string message))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);

                    Debug.Log($"Message sent");
                }

                await Task.Delay(50);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SendQueue error: {e.Message}");
        }
    }

    class TimeStamp {
        public double timestamp_remote = 0;
    }

    // 接收来自服务器的消息
    private async Task ReceiveMessages()
    {
        try
        {
            byte[] buffer = new byte[1024 * 1024 * 16];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

                // 检查是否收到关闭帧
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("Received close frame from server. Closing connection.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing in response to server", CancellationToken.None);
                    Debug.Log("WebSocket connection closed.");
                    break;
                }

                // 正常接收文本消息
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"Message received: {message}");
                TimeStamp ts = JsonConvert.DeserializeObject<TimeStamp>(message);
                Debug.Log($"Time spend: {CustomFunctions.GetUnixTime() - ts.timestamp_remote}");
                if (ts.timestamp_remote < cancelTimeStamp){
                    Debug.Log("Received message is too late, ignore it.");
                }
                else{
                    receiveQueue.Enqueue(message);  // 将接收到的消息加入接收队列
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ReceiveMessages error: {e.Message}");
        }
    }

    // 在销毁对象时关闭WebSocket连接
    private async void OnDestroy()
    {
        if (webSocket != null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application quitting", cts.Token);
            webSocket.Dispose();
            cts.Cancel();
        }
    }

    // double GetUnixTime()
    // {
    //     // 获取当前 UTC 时间
    //     DateTime currentTime = DateTime.UtcNow;

    //     // 计算自1970年1月1日以来的秒数
    //     DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //     TimeSpan timeSpan = currentTime - unixEpoch;

    //     // 返回以秒为单位的时间戳
    //     return timeSpan.TotalSeconds;
    // }

    //////////////////////////// 以下为自定义方法 ////////////////////////////
    class CancelData
    {
        public string type;
        public string text;
        public double timestamp_remote;
    }

    public void sendCancel(string text){
        cancelTimeStamp = CustomFunctions.GetUnixTime();
        CancelData data = new CancelData {
            type = "cancel",
            text = text,
            timestamp_remote = CustomFunctions.GetUnixTime()
        };
        string message = JsonConvert.SerializeObject(data);
        SendMessageToServer(message);
        return;
    }

    class AudioData
    {
        public string type;
        public string audio_file;
        public double timestamp_remote;
    }

    public void sendAudio(byte[] audio_data){
        AudioData data = new AudioData
        {
            type = "audio",
            audio_file = Convert.ToBase64String(audio_data),
            timestamp_remote = CustomFunctions.GetUnixTime()
        };
        string message = JsonConvert.SerializeObject(data);
        SendMessageToServer(message);
        return;
    }
}
