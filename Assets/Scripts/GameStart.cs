using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameStart : MonoBehaviour
{
    public TMP_InputField urlInput;
    public TMP_Dropdown sceneDropdown;
    public Button startButton;
    public List<string> scenes;

    private void Start()
    {
        // 添加按钮点击事件监听器
        startButton.onClick.AddListener(OnStartButtonClicked);
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(scenes);
    }

    private void OnStartButtonClicked()
    {
        // 获取用户输入的参数
        string url = urlInput.text;
        PlayerPrefs.SetString("urlInput", url);
        string scene = scenes[sceneDropdown.value];

        // 加载游戏场景
        SceneManager.LoadScene(scene);
    }
}
