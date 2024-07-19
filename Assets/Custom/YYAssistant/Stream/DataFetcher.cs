using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DataFetcher : MonoBehaviour
{
    public StringEvent segmentEvent;
    public bool cancellationToken = false; // 添加中断标志
    public string url = "";
    bool started = false;
    public string userId;
    void Start()
    {
        url = PlayerPrefs.GetString("urlInput", url);
        userId = PlayerPrefs.GetString("userId", "0");
    }

    public void StopFetching()
    {
        cancellationToken = true; // 设置中断标志
        StartCoroutine(CancelRequest(userId));
    }

    public IEnumerator GetDataCoroutine(byte[] audioData, string userId)
    {
        cancellationToken = false; // 在每次调用时重置中断标志
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("id", userId);
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url + "/asr_llm_tts", form))
        {
            // 设置自定义的DownloadHandler
            webRequest.downloadHandler = new CustomDownloadHandler(this);
            // Set the content type header to application/json
            // webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                if (cancellationToken)
                {
                    webRequest.Abort(); // 中断请求
                    Debug.Log("Request aborted");
                    yield break; // 退出Coroutine
                }
                yield return null; // 等待下一帧
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("All data received");
            }
        }
    }

    public IEnumerator CancelRequest(string userId)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url + "/cancel", "POST");
        string json = "{\"id\":\"" + userId + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Cancel request error: " + webRequest.error);
        }
        else
        {
            Debug.Log("Cancel request sent successfully");
        }
    }
}