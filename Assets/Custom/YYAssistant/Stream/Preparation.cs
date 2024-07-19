using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

using UnityEngine.UI;
using TMPro;

public class Preparation : MonoBehaviour
{
    public string url = "";
    public string userId;
    void Start()
    {
    }

    public IEnumerator ClearHistory(string userId)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url + "/clear", "POST");
        string json = "{\"id\":\"" + userId + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

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

    public IEnumerator HealthCheck()
    {
        UnityWebRequest webRequest = new UnityWebRequest(url + "/heartbeat", "POST");
        string json = "{}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Health check error: " + webRequest.error);
        }
        else
        {
            Debug.Log("Response: " + webRequest.downloadHandler.text);
        }

    }

    private IEnumerator SetSystemMessage(string systemPrompt, string userId)
    {
        // Create a JSON object
        string json = JsonUtility.ToJson(new SystemPromptData(systemPrompt, userId));

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
                Debug.Log("Set system message error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator SetTTSModel(string characterVoice, string characterModel, string characterConfig, string userId)
    {
        // Create a JSON object
        string json = JsonUtility.ToJson(new TTSModelData(characterVoice, characterModel, characterConfig, userId));

        // Create a UnityWebRequest for POST
        using (UnityWebRequest webRequest = new UnityWebRequest(url + "/set_model", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Set TTS model error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    public IEnumerator AssistantInit(Slider progressBar, TMP_Text progressText, float weight)
    {
        float initializationWeight = progressBar.value;
        url = PlayerPrefs.GetString("urlInput", url);
        userId = PlayerPrefs.GetString("userId", "0");

        progressBar.value += weight * 0.2f;
        progressText.text = "Health check...";
        yield return StartCoroutine(HealthCheck());
        yield return new WaitForSeconds(0.5f);
        progressBar.value += weight * 0.2f;

        if (PlayerPrefs.GetInt("clearHistory", 0) == 1)
        {
            progressText.text = "Setting up...";
            yield return StartCoroutine(ClearHistory(userId));
            progressBar.value += weight * 0.2f;
            yield return StartCoroutine(SetSystemMessage(PlayerPrefs.GetString("systemMessageInput", ""), userId));
            yield return new WaitForSeconds(0.5f);
            progressBar.value += weight * 0.2f;
        }
        else{
            progressText.text = "Setting up...";
            yield return new WaitForSeconds(0.5f);
            progressBar.value += weight * 0.4f;
        }

        string character_voice = PlayerPrefs.GetString("character_voice", "");
        string character_model = PlayerPrefs.GetString("character_model", "");
        string character_config = PlayerPrefs.GetString("character_config", "");

        if(character_voice != "" && character_model != "" && character_config != ""){
            progressText.text = "TTS Model...";
            yield return StartCoroutine(SetTTSModel(character_voice, character_model, character_config, userId));
            yield return new WaitForSeconds(0.5f);
            progressBar.value += weight * 0.2f;
        }
        else{
            progressText.text = "Skip TTS Setting...";
            yield return new WaitForSeconds(0.5f);
            progressBar.value += weight * 0.2f;
        }
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

    [System.Serializable]
    public class TTSModelData
    {
        public string speaker;
        public string model;
        public string config;
        public string id;

        public TTSModelData(string character_voice, string character_model, string character_config, string id)
        {
            this.speaker = character_voice;
            this.model = character_model;
            this.config = character_config;
            this.id = id;
        }
    }
}