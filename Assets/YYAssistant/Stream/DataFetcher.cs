using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DataFetcher : MonoBehaviour
{
    public StringEvent segmentEvent;
    public bool cancellationToken = false; // 添加中断标志
    public string url = "http://localhost:5050";
    bool started = false;
    void Start()
    {
        url = PlayerPrefs.GetString("urlInput", url);
        if (PlayerPrefs.GetInt("clearHistory", 0) == 1)
        {
            StartCoroutine(ExecuteInOrder());
        }
        else{
            started = true;
        }
    }

    public void StopFetching()
    {
        cancellationToken = true; // 设置中断标志
        StartCoroutine(CancelRequest());
    }

    public IEnumerator GetDataCoroutine(byte[] audioData)
    {
        if (!started)
        {
            yield return new WaitUntil(() => started);
        }
        cancellationToken = false; // 在每次调用时重置中断标志
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
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

    public IEnumerator CancelRequest()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url + "/cancel", ""))
        {
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

    public IEnumerator ClearHistory()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url + "/clear", ""))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Clear history error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator SetSystemMessage(string systemPrompt, string id = "0")
    {
        // Create a JSON object
        string json = JsonUtility.ToJson(new SystemPromptData(systemPrompt, id));

        // Create a UnityWebRequest for POST
        using (UnityWebRequest webRequest = new UnityWebRequest(url + "/set_system_prompt", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Clear history error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator ExecuteInOrder()
    {
        // Wait for ClearHistory to finish
        yield return StartCoroutine(ClearHistory());

        // After ClearHistory finishes, execute SetSystemMessage
        yield return StartCoroutine(SetSystemMessage(PlayerPrefs.GetString("systemMessageInput", "")));
        started = true;
    }

    [System.Serializable]
    public class SystemPromptData
    {
        public string system_prompt;
        public string id;

        public SystemPromptData(string system_prompt, string id)
        {
            this.system_prompt = system_prompt;
            this.id = id;
        }
    }
}