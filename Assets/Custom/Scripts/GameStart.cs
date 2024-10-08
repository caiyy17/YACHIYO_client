using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SceneLoaderWithProgress))]
public class GameStart : MonoBehaviour
{
    [Header("Game Settings")]
    public GameSettingsData settingsData;
    public string server_url;
    public string user_id;
    public int char_id;
    public string scene_name;
    [TextArea(1, 1000)]
    public string system_message;
    public bool clear_history = true;
    public List<string> chars = new List<string>();

    [Header("UI Elements")]
    public TMP_InputField urlInput;
    public TMP_InputField userIdInput;
    public TMP_Dropdown charDropDown;
    public TMP_InputField systemMessageInput;
    public Toggle clearHistory;

    public Button startGame;
    public Button resetDefault;

    public GameObject mainScreen;
    public GameObject settingPanel;
    public GameObject comfirmPanel;
    public GameObject resetPanel;
    public Button openSetting;
    public Button closeSetting;
    public Button confirmYes;
    public Button confirmNot;
    public Button resetYes;
    public Button resetNot;

    public TMP_Text versionText;

    SceneLoaderWithProgress sceneLoader;
    public GameObject errorPanel;
    public TMP_Text errorText;
    public Button closeError;

    public bool hideUI = false;
    public bool useBGM = true;
    public bool useVAD = false;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;

    public Button openAppSetting;
    public GameObject AppSettingPanel;
    public Button closeAppSetting;
    public Toggle hideUIToggle;
    public Toggle useBGMToggle;
    public Toggle useVADToggle;
    public Slider speakingThresholdSlider;
    public Slider silenceThresholdSlider;
    public Slider Display;

    public VoiceDetector voiceDetector;
    public AudioSource bgm;
    
    private void Start()
    {
        // 显示游戏版本号
        versionText.text = "Version: " + Application.version;

        // 从ScriptableObject中读取用户设置
        server_url = settingsData.server_url;
        user_id = settingsData.user_id;
        chars = new List<string>();
        chars.Add("default");
        foreach (CharacterSettingsData charData in settingsData.chars)
        {
            chars.Add(charData.character_name);
        }
        scene_name = settingsData.scene_name;
        system_message = settingsData.system_message;
        clear_history = settingsData.clear_history;

        // 从PlayerPrefs中读取用户设置
        urlInput.text = PlayerPrefs.GetString("urlInput", server_url);
        userIdInput.text = PlayerPrefs.GetString("userId", user_id);
        charDropDown.ClearOptions();
        charDropDown.AddOptions(chars);
        charDropDown.value = PlayerPrefs.GetInt("charIndex", char_id);
        systemMessageInput.text = PlayerPrefs.GetString("systemMessageInput", system_message);
        clearHistory.isOn = PlayerPrefs.GetInt("clearHistory", clear_history ? 1 : 0) == 1;
        // AppSettings
        hideUI = PlayerPrefs.GetInt("hideUI", hideUI ? 1 : 0) == 1;
        useBGM = PlayerPrefs.GetInt("useBGM", useBGM ? 1 : 0) == 1;
        useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
        speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
        silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);
        bgm.Play();
        if(!useBGM){
            bgm.Pause();
        }

        if(userIdInput.text == "0"){
            //generate a UUID
            user_id = System.Guid.NewGuid().ToString();
            userIdInput.text = user_id;
        }

        if(charDropDown.value > charDropDown.options.Count - 1){
            char_id = 0;
            charDropDown.value = char_id;
        }

        // 添加按钮点击事件监听器
        startGame.onClick.AddListener(OnStartGameButtonClicked);
        openSetting.onClick.AddListener(OnOpenSettingButtonClicked);
        closeSetting.onClick.AddListener(OnCloseSettingButtonClicked);
        resetDefault.onClick.AddListener(OnResetDefaultButtonClicked);
        confirmYes.onClick.AddListener(OnConfirmYesButtonClicked);
        confirmNot.onClick.AddListener(OnConfirmNotButtonClicked);
        resetYes.onClick.AddListener(OnResetYesButtonClicked);
        resetNot.onClick.AddListener(OnResetNotButtonClicked);
        charDropDown.onValueChanged.AddListener(delegate {
            OnCharDropDownValueChanged(charDropDown);
        });
        // AppSettings
        closeError.onClick.AddListener(() => {
            errorPanel.SetActive(false);
        });

        openAppSetting.onClick.AddListener(() => {
            hideUIToggle.isOn = hideUI;
            useBGMToggle.isOn = useBGM;
            useVADToggle.isOn = useVAD;
            speakingThresholdSlider.value = ToLog(speakingThreshold);
            silenceThresholdSlider.value = ToLog(silenceThreshold);
            AppSettingPanel.SetActive(true);
        });
        closeAppSetting.onClick.AddListener(() => {
            hideUI = hideUIToggle.isOn;
            useBGM = useBGMToggle.isOn;
            useVAD = useVADToggle.isOn;
            speakingThreshold = ToExp(speakingThresholdSlider.value);
            silenceThreshold = ToExp(silenceThresholdSlider.value);

            PlayerPrefs.SetInt("hideUI", hideUI ? 1 : 0);
            PlayerPrefs.SetInt("useBGM", useBGM ? 1 : 0);
            PlayerPrefs.SetInt("useVAD", useVAD ? 1 : 0);
            PlayerPrefs.SetFloat("speakingThreshold", speakingThreshold);
            PlayerPrefs.SetFloat("silenceThreshold", silenceThreshold);

            if(useBGM){
                if(!bgm.isPlaying){
                    bgm.UnPause();
                }
            }
            else{
                bgm.Pause();
            }

            AppSettingPanel.SetActive(false);
        });

        sceneLoader = GetComponent<SceneLoaderWithProgress>();

        PlayerPrefs.SetString("urlInput", urlInput.text);
        PlayerPrefs.SetString("userId", userIdInput.text);
        PlayerPrefs.SetInt("charIndex", charDropDown.value);
        PlayerPrefs.SetString("systemMessageInput", systemMessageInput.text);
        PlayerPrefs.SetInt("clearHistory", clearHistory.isOn ? 1 : 0);
        // AppSettings
        PlayerPrefs.SetInt("hideUI", hideUI ? 1 : 0);
        PlayerPrefs.SetInt("useBGM", useBGM ? 1 : 0);
        PlayerPrefs.SetInt("useVAD", useVAD ? 1 : 0);
        PlayerPrefs.SetFloat("speakingThreshold", speakingThreshold);
        PlayerPrefs.SetFloat("silenceThreshold", silenceThreshold);
    }

    private void OnCharDropDownValueChanged(TMP_Dropdown change)
    {
        char_id = change.value;
        if (char_id == 0)
        {
            scene_name = settingsData.scene_name;
            systemMessageInput.text = PlayerPrefs.GetString("systemMessageInput", system_message);
        }
        else
        {
            scene_name = settingsData.chars[char_id - 1].scene_name;
            systemMessageInput.text = settingsData.chars[char_id - 1].system_message;
        }
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
        if (urlInput.text != PlayerPrefs.GetString("urlInput", server_url) ||
            userIdInput.text != PlayerPrefs.GetString("userId", user_id) ||
            charDropDown.value != PlayerPrefs.GetInt("charIndex", char_id) ||
            systemMessageInput.text != PlayerPrefs.GetString("systemMessageInput", system_message) ||
            clearHistory.isOn != (PlayerPrefs.GetInt("clearHistory", clear_history ? 1 : 0) == 1)){
            comfirmPanel.SetActive(true);
            }
        else{
            mainScreen.SetActive(true);
        }
    }

    private void OnConfirmYesButtonClicked()
    {
        PlayerPrefs.SetString("urlInput", urlInput.text);
        PlayerPrefs.SetString("userId", userIdInput.text);
        PlayerPrefs.SetInt("charIndex", charDropDown.value);
        PlayerPrefs.SetString("systemMessageInput", systemMessageInput.text);
        PlayerPrefs.SetInt("clearHistory", clearHistory.isOn ? 1 : 0);
        // 关闭设置界面
        comfirmPanel.SetActive(false);
        mainScreen.SetActive(true);
    }

    private void OnConfirmNotButtonClicked()
    {
        urlInput.text = PlayerPrefs.GetString("urlInput", server_url);
        userIdInput.text = PlayerPrefs.GetString("userId", user_id);
        charDropDown.value = PlayerPrefs.GetInt("charIndex", char_id);
        systemMessageInput.text = PlayerPrefs.GetString("systemMessageInput", system_message);
        clearHistory.isOn = PlayerPrefs.GetInt("clearHistory", clear_history ? 1 : 0) == 1;
        // 关闭设置界面
        comfirmPanel.SetActive(false);
        mainScreen.SetActive(true);
    }
    private void OnResetDefaultButtonClicked()
    {
        resetPanel.SetActive(true);
        settingPanel.SetActive(false);
    }
    private void OnResetYesButtonClicked()
    {
        PlayerPrefs.SetString("urlInput", server_url);
        PlayerPrefs.SetString("userId", user_id);
        PlayerPrefs.SetString("systemMessageInput", system_message);
        PlayerPrefs.SetInt("clearHistory", clear_history ? 1 : 0);
        PlayerPrefs.SetInt("charIndex", char_id);

        urlInput.text = server_url;
        userIdInput.text = user_id;
        systemMessageInput.text = system_message;
        clearHistory.isOn = clear_history;
        charDropDown.value = char_id;

        if(userIdInput.text == "0"){
            //generate a UUID
            user_id = System.Guid.NewGuid().ToString();
            userIdInput.text = user_id;
        }
        PlayerPrefs.SetString("userId", userIdInput.text);

        resetPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    private void OnResetNotButtonClicked()
    {
        resetPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    private void OnStartGameButtonClicked()
    {
        string scene = scene_name;

        int current_id = PlayerPrefs.GetInt("charIndex", char_id);
        if(current_id == 0){
            PlayerPrefs.SetString("character_voice", "");
            PlayerPrefs.SetString("character_model", "");
            PlayerPrefs.SetString("character_config", "");
        }
        else{
            Debug.Log("current_id: " + current_id);
            Debug.Log("character_voice: " + settingsData.chars[current_id - 1].character_voice);
            Debug.Log("character_model: " + settingsData.chars[current_id - 1].character_model);
            Debug.Log("character_config: " + settingsData.chars[current_id - 1].character_config);
            PlayerPrefs.SetString("character_voice", settingsData.chars[current_id - 1].character_voice);
            PlayerPrefs.SetString("character_model", settingsData.chars[current_id - 1].character_model);
            PlayerPrefs.SetString("character_config", settingsData.chars[current_id - 1].character_config);
        }
        // 加载游戏场景，把其他交互禁用
        mainScreen.SetActive(false);
        settingPanel.SetActive(false);
        sceneLoader.LoadScene(scene);
    }

    public void OnStartGameError(string errorMessage)
    {
        // 如果加载场景失败，显示错误信息
        mainScreen.SetActive(true);
        settingPanel.SetActive(false);
        errorPanel.SetActive(true);
        errorText.text = errorMessage;
        Debug.Log("return to main screen");
    }

    float ToLog(float value)
    {
        return Math.Clamp((Mathf.Log10(value) + 5) / 5, 0, 1);
    }

    float ToExp(float value)
    {
        return Mathf.Pow(10, value * 5 - 5);
    }

    void Update(){
        Display.value = ToLog(voiceDetector.currentLoudness);
    }
}
