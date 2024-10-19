using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

using UnityEngine.UI;
using TMPro;

public class SetupPipeline : MonoBehaviour
{
    private string url = "";
    private string userId = "";
    [TextArea(1, 1000)]
    private string pipeline_config = "";

    public bool errorOccurred = false;
    public string errorMessage = "";

    public float current_progress = 0.0f;
    public string current_status = "";
    void Start()
    {
    }

    public IEnumerator CustomWebRequest(string url, string json, string name)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(name + " error: " + webRequest.error);
            errorOccurred = true;
            errorMessage = name + " error: " + webRequest.error;
        }
        else
        {
            Debug.Log("Response: " + webRequest.downloadHandler.text);
        }
    }

    public IEnumerator AssistantSetup()
    {
        // 从PlayerPrefs中获取设置
        current_progress = 0.0f;
        current_status = "Setting up...";
        errorOccurred = false;
        errorMessage = "";
        url = PlayerPrefs.GetString("urlInput", url);
        userId = PlayerPrefs.GetString("userId", userId);
        pipeline_config = PlayerPrefs.GetString("pipelineConfig", pipeline_config);

        // Health check
        current_progress = 0.2f;
        current_status = "Health check...";
        string healthCheckJson = JsonUtility.ToJson(new ClientData(userId));
        yield return CustomWebRequest("http://" + url + "/heartbeat/", healthCheckJson, "Health check");
        yield return new WaitForSeconds(1f);
        
        // Setup pipeline
        current_progress = 0.4f;
        current_status = "Register client...";
        string clientJson = JsonUtility.ToJson(new ClientData(userId));
        yield return CustomWebRequest("http://" + url + "/register/", clientJson, "Register client");
        yield return new WaitForSeconds(1f);

        current_progress = 0.6f;
        current_status = "Setup pipeline...";
        bool force = true;
        string charId = PlayerPrefs.GetString("charId", "default");
        if (charId == "default")
        {
            force = false;
        }
        string configJson = JsonUtility.ToJson(new ConfigData(pipeline_config, force));
        yield return CustomWebRequest("http://" + url + "/init_pipeline/" + userId, configJson, "Setup pipeline");
        yield return new WaitForSeconds(1f);

        current_progress = 1.0f;
        current_status = "Setup complete";
        yield return new WaitForSeconds(1f);

        // Setup complete
        current_status = "";
        yield return null;
    }

    [System.Serializable]
    public class ClientData
    {
        public string client_id;

        public ClientData(string client_id)
        {
            this.client_id = client_id;
        }
    }

    [System.Serializable]
    public class ConfigData
    {
        public string config;
        public bool force;

        public ConfigData(string config, bool force)
        {
            this.config = config;
            this.force = force;
        }
    }
}