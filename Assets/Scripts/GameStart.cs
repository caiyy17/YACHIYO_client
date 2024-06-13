using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameStart : MonoBehaviour
{
    public string url = "http://localhost:5050";
    public string system_message = "你是一个人工智能助手，你可以回答一些问题，也可以帮助用户完成一些任务。";
    public bool clear = true;
    public int sceneIndex = 0;
    public List<string> scenes;

    public TMP_Dropdown sceneDropdown;
    public TMP_InputField urlInput;
    public TMP_InputField systemMessageInput;
    //UI panal
    public GameObject settingPanel;
    public Button startGame;
    public Button openSetting;
    public Button closeSetting;
    public Button resetDefault;
    public Toggle clearHistory;
    
    private void Start()
    {
        urlInput.text = PlayerPrefs.GetString("urlInput", url);
        systemMessageInput.text = PlayerPrefs.GetString("systemMessageInput", system_message);
        clearHistory.isOn = PlayerPrefs.GetInt("clearHistory", clear ? 1 : 0) == 1;
        // 添加按钮点击事件监听器
        startGame.onClick.AddListener(OnStartGameButtonClicked);
        openSetting.onClick.AddListener(OnOpenSettingButtonClicked);
        closeSetting.onClick.AddListener(OnCloseSettingButtonClicked);
        resetDefault.onClick.AddListener(OnResetDefaultButtonClicked);
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(scenes);
        sceneDropdown.value = PlayerPrefs.GetInt("sceneIndex", sceneIndex);
    }
    private void OnOpenSettingButtonClicked()
    {
        // 打开设置界面
        settingPanel.SetActive(true);
    }

    private void OnCloseSettingButtonClicked()
    {
        // 关闭设置界面
        settingPanel.SetActive(false);
    }

    private void OnResetDefaultButtonClicked()
    {
        PlayerPrefs.SetString("urlInput", url);
        PlayerPrefs.SetString("systemMessageInput", system_message);
        PlayerPrefs.SetInt("clearHistory", clear ? 1 : 0);
        PlayerPrefs.SetInt("sceneIndex", sceneIndex);
    }

    private void OnStartGameButtonClicked()
    {
        // 获取用户输入的参数
        PlayerPrefs.SetString("urlInput", urlInput.text);
        PlayerPrefs.SetString("systemMessageInput", systemMessageInput.text);
        PlayerPrefs.SetInt("clearHistory", clearHistory.isOn ? 1 : 0);
        PlayerPrefs.SetInt("sceneIndex", sceneDropdown.value);
        string scene = scenes[sceneDropdown.value];
        // 加载游戏场景
        SceneManager.LoadScene(scene);
    }
}
