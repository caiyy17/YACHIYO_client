using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(SceneLoaderWithProgress))]
public class GameStart : MonoBehaviour
{
    [Header("Game Settings")]
    public GameSettingsData settingsData;
    public string server_url;
    public string user_id;
    public int scene_id;
    public string system_message;
    public bool clear_history = true;
    public List<string> scenes = new List<string>();

    [Header("UI Elements")]
    public TMP_InputField urlInput;
    public TMP_InputField userIdInput;
    public TMP_Dropdown sceneDropdown;
    public TMP_InputField systemMessageInput;
    public Toggle clearHistory;

    public Button startGame;
    public Button resetDefault;

    public GameObject mainScreen;
    public GameObject settingPanel;
    public Button openSetting;
    public Button closeSetting;

    public TMP_Text versionText;

    SceneLoaderWithProgress sceneLoader;
    
    private void Start()
    {
        // 显示游戏版本号
        versionText.text = "Version: " + Application.version;

        // 从ScriptableObject中读取用户设置
        server_url = settingsData.server_url;
        user_id = settingsData.user_id;
        scenes = settingsData.scenes;
        string scene_name = settingsData.scene_name;
        //如果scenes中有scene_name，则返回scene_name的索引，否则返回0
        scene_id = scenes.IndexOf(scene_name);
        if (scene_id == -1)
        {
            scene_id = 0;
        }
        system_message = settingsData.system_message;
        clear_history = settingsData.clear_history;

        // 从PlayerPrefs中读取用户设置
        urlInput.text = PlayerPrefs.GetString("urlInput", server_url);
        userIdInput.text = PlayerPrefs.GetString("userId", user_id);
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(scenes);
        sceneDropdown.value = PlayerPrefs.GetInt("sceneIndex", scene_id);
        systemMessageInput.text = PlayerPrefs.GetString("systemMessageInput", system_message);
        clearHistory.isOn = PlayerPrefs.GetInt("clearHistory", clear_history ? 1 : 0) == 1;

        // 添加按钮点击事件监听器
        startGame.onClick.AddListener(OnStartGameButtonClicked);
        openSetting.onClick.AddListener(OnOpenSettingButtonClicked);
        closeSetting.onClick.AddListener(OnCloseSettingButtonClicked);
        resetDefault.onClick.AddListener(OnResetDefaultButtonClicked);

        sceneLoader = GetComponent<SceneLoaderWithProgress>();
    }
    private void OnOpenSettingButtonClicked()
    {
        // 打开设置界面
        mainScreen.SetActive(false);
        settingPanel.SetActive(true);
    }

    private void OnCloseSettingButtonClicked()
    {
        // 关闭设置界面
        settingPanel.SetActive(false);
        mainScreen.SetActive(true);
    }

    private void OnResetDefaultButtonClicked()
    {
        PlayerPrefs.SetString("urlInput", server_url);
        PlayerPrefs.SetString("userId", user_id);
        PlayerPrefs.SetString("systemMessageInput", system_message);
        PlayerPrefs.SetInt("clearHistory", clear_history ? 1 : 0);
        PlayerPrefs.SetInt("sceneIndex", scene_id);

        urlInput.text = server_url;
        userIdInput.text = user_id;
        systemMessageInput.text = system_message;
        clearHistory.isOn = clear_history;
        sceneDropdown.value = scene_id;
    }

    private void OnStartGameButtonClicked()
    {
        // 获取用户输入的参数
        PlayerPrefs.SetString("urlInput", urlInput.text);
        PlayerPrefs.SetString("userId", userIdInput.text);
        PlayerPrefs.SetInt("sceneIndex", sceneDropdown.value);
        PlayerPrefs.SetString("systemMessageInput", systemMessageInput.text);
        PlayerPrefs.SetInt("clearHistory", clearHistory.isOn ? 1 : 0);
        
        string scene = scenes[sceneDropdown.value];
        // 加载游戏场景，把其他交互禁用
        mainScreen.SetActive(false);
        settingPanel.SetActive(false);
        sceneLoader.LoadScene(scene);
    }
}
