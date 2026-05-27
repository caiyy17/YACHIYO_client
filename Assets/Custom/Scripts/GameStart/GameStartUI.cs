using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using Yachiyo;

[RequireComponent(typeof(SceneLoaderWithProgress))]
public class GameStartUI : MonoBehaviour
{
    [Header("Game Settings")]
    public GameSettingsData settingsData;
    public string server_url;
    public string webrtc_url;
    public string user_id;
    public string char_id;
    public string scene_name;
    public string pipeline_config;
    public List<string> chars = new List<string>();

    [Header("UI Elements")]
    public TMP_InputField urlInput;
    public TMP_InputField webrtcUrlInput;
    public TMP_InputField userIdInput;
    public TMP_Dropdown charDropDown;
    public TMP_InputField pipelineConfigInput;

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
    public bool useVAD = false;

    public float speakingThreshold = 0.01f;
    public float silenceThreshold = 0.001f;

    public Button openAppSetting;
    public GameObject AppSettingPanel;
    public Button closeAppSetting;
    public Toggle hideUIToggle;
    public Toggle useVADToggle;
    public Slider speakingThresholdSlider;
    public Slider silenceThresholdSlider;
    public TMP_Dropdown micDropdown;
    public TMP_Dropdown webcamDropdown;
    public Slider Display;
    public float current_volumn = 0;

    [SerializeField] public InputAction startAction;

    private void Start()
    {
        // 显示游戏版本号
        versionText.text = "Version: " + Application.version;

        // 从PlayerPrefs中读取用户设置
        server_url = PlayerPrefs.GetString("urlInput", settingsData.server_url);
        webrtc_url = PlayerPrefs.GetString("webrtcUrlInput", settingsData.webrtc_url);
        user_id = PlayerPrefs.GetString("userId", settingsData.user_id);
        pipeline_config = PlayerPrefs.GetString("pipelineConfig", settingsData.pipeline_config);
        char_id = PlayerPrefs.GetString("charId", settingsData.char_id);

        chars = new List<string>();
        foreach (CharacterSettingsData charData in settingsData.chars)
        {
            chars.Add(charData.character_name);
        }
        int char_index = chars.IndexOf(char_id);
        if (char_index == -1)
        {
            char_index = 0;
        }
        char_id = settingsData.chars[char_index].character_name;
        scene_name = settingsData.chars[char_index].scene_name;

        // 设置UI元素的初始值
        urlInput.text = server_url;
        webrtcUrlInput.text = webrtc_url;
        userIdInput.text = user_id;
        pipelineConfigInput.text = pipeline_config;
        charDropDown.ClearOptions();
        charDropDown.AddOptions(chars);
        charDropDown.value = char_index;
        // AppSettings
        hideUI = PlayerPrefs.GetInt("hideUI", hideUI ? 1 : 0) == 1;
        useVAD = PlayerPrefs.GetInt("useVAD", useVAD ? 1 : 0) == 1;
        speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
        silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);

        if (userIdInput.text == "0")
        {
            //generate a UUID
            user_id = System.Guid.NewGuid().ToString();
            userIdInput.text = user_id;
        }

        if (charDropDown.value > charDropDown.options.Count - 1)
        {
            char_id = chars[0];
            charDropDown.value = 0;
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
        charDropDown.onValueChanged.AddListener(delegate
        {
            OnCharDropDownValueChanged(charDropDown);
        });
        // AppSettings
        closeError.onClick.AddListener(() =>
        {
            errorPanel.SetActive(false);
        });

        openAppSetting.onClick.AddListener(() =>
        {
            mainScreen.SetActive(false);
            hideUIToggle.isOn = hideUI;
            useVADToggle.isOn = useVAD;
            speakingThresholdSlider.value = ToLog(speakingThreshold);
            silenceThresholdSlider.value = ToLog(silenceThreshold);
            // Populate microphone dropdown
            if (micDropdown != null)
            {
                micDropdown.ClearOptions();
                string[] devices = MicrophoneManager.Instance.GetAvailableDevices();
                micDropdown.AddOptions(new List<string>(devices));
                string currentMic = MicrophoneManager.Instance.DeviceName;
                int micIndex = Array.IndexOf(devices, currentMic);
                micDropdown.value = Mathf.Max(0, micIndex);
            }
            // Populate webcam dropdown
            if (webcamDropdown != null && WebcamManager.Instance != null)
            {
                webcamDropdown.ClearOptions();
                string[] displayNames = WebcamManager.Instance.GetDeviceDisplayNames();
                webcamDropdown.AddOptions(new List<string>(displayNames));
                var camDevices = WebcamManager.Instance.GetAvailableDevices();
                string currentCam = WebcamManager.Instance.DeviceName;
                int camIndex = 0;
                for (int i = 0; i < camDevices.Length; i++)
                {
                    if (camDevices[i].name == currentCam) { camIndex = i; break; }
                }
                webcamDropdown.value = camIndex;
            }
            AppSettingPanel.SetActive(true);
        });
        closeAppSetting.onClick.AddListener(() =>
        {
            hideUI = hideUIToggle.isOn;
            useVAD = useVADToggle.isOn;
            speakingThreshold = ToExp(speakingThresholdSlider.value);
            silenceThreshold = ToExp(silenceThresholdSlider.value);

            // Apply microphone selection
            if (micDropdown != null && micDropdown.options.Count > 0)
            {
                string selectedMic = micDropdown.options[micDropdown.value].text;
                if (selectedMic != MicrophoneManager.Instance.DeviceName)
                {
                    MicrophoneManager.Instance.SwitchMicrophone(selectedMic);
                }
            }
            // Apply webcam selection
            if (webcamDropdown != null && webcamDropdown.options.Count > 0 && WebcamManager.Instance != null)
            {
                var camDevices = WebcamManager.Instance.GetAvailableDevices();
                int selectedIdx = webcamDropdown.value;
                if (selectedIdx < camDevices.Length)
                {
                    string selectedCam = camDevices[selectedIdx].name;
                    if (selectedCam != WebcamManager.Instance.DeviceName)
                    {
                        WebcamManager.Instance.SwitchDevice(selectedCam);
                    }
                }
            }

            PlayerPrefs.SetInt("hideUI", hideUI ? 1 : 0);
            PlayerPrefs.SetInt("useVAD", useVAD ? 1 : 0);
            PlayerPrefs.SetFloat("speakingThreshold", speakingThreshold);
            PlayerPrefs.SetFloat("silenceThreshold", silenceThreshold);

            AppSettingPanel.SetActive(false);
            mainScreen.SetActive(true);
        });

        sceneLoader = GetComponent<SceneLoaderWithProgress>();

        PlayerPrefs.SetString("urlInput", urlInput.text);
        PlayerPrefs.SetString("webrtcUrlInput", webrtcUrlInput.text);
        PlayerPrefs.SetString("userId", userIdInput.text);
        PlayerPrefs.SetString("charId", char_id);
        PlayerPrefs.SetString("sceneName", scene_name);
        PlayerPrefs.SetString("pipelineConfig", pipelineConfigInput.text);
        // AppSettings
        PlayerPrefs.SetInt("hideUI", hideUI ? 1 : 0);
        PlayerPrefs.SetInt("useVAD", useVAD ? 1 : 0);
        PlayerPrefs.SetFloat("speakingThreshold", speakingThreshold);
        PlayerPrefs.SetFloat("silenceThreshold", silenceThreshold);

        startAction.Enable();
        startAction.performed += ctx => OnStartGameButtonClicked();
    }

    void OnDisable()
    {
        startGame.onClick.RemoveAllListeners();
        openSetting.onClick.RemoveAllListeners();
        closeSetting.onClick.RemoveAllListeners();
        resetDefault.onClick.RemoveAllListeners();
        confirmYes.onClick.RemoveAllListeners();
        confirmNot.onClick.RemoveAllListeners();
        resetYes.onClick.RemoveAllListeners();
        resetNot.onClick.RemoveAllListeners();
        charDropDown.onValueChanged.RemoveAllListeners();
        startAction.Disable();
    }

    private void OnCharDropDownValueChanged(TMP_Dropdown change)
    {
        int index = change.value;
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
            webrtcUrlInput.text != PlayerPrefs.GetString("webrtcUrlInput", webrtc_url) ||
            userIdInput.text != PlayerPrefs.GetString("userId", user_id) ||
            pipelineConfigInput.text != PlayerPrefs.GetString("pipelineConfig", pipeline_config) ||
            chars[charDropDown.value] != PlayerPrefs.GetString("charId", char_id))
        {
            comfirmPanel.SetActive(true);
        }
        else
        {
            mainScreen.SetActive(true);
        }
    }

    private void OnConfirmYesButtonClicked()
    {
        if (userIdInput.text == "0")
        {
            //generate a UUID
            user_id = System.Guid.NewGuid().ToString();
            userIdInput.text = user_id;
        }
        server_url = urlInput.text;
        webrtc_url = webrtcUrlInput.text;
        user_id = userIdInput.text;
        pipeline_config = pipelineConfigInput.text;
        char_id = chars[charDropDown.value];
        scene_name = settingsData.chars[charDropDown.value].scene_name;
        PlayerPrefs.SetString("urlInput", server_url);
        PlayerPrefs.SetString("webrtcUrlInput", webrtc_url);
        PlayerPrefs.SetString("userId", user_id);
        PlayerPrefs.SetString("pipelineConfig", pipeline_config);
        PlayerPrefs.SetString("charId", char_id);
        PlayerPrefs.SetString("sceneName", scene_name);
        // 关闭设置界面
        comfirmPanel.SetActive(false);
        mainScreen.SetActive(true);
    }

    private void OnConfirmNotButtonClicked()
    {
        urlInput.text = PlayerPrefs.GetString("urlInput", server_url);
        webrtcUrlInput.text = PlayerPrefs.GetString("webrtcUrlInput", webrtc_url);
        userIdInput.text = PlayerPrefs.GetString("userId", user_id);
        pipelineConfigInput.text = PlayerPrefs.GetString("pipelineConfig", pipeline_config);
        charDropDown.value = chars.IndexOf(PlayerPrefs.GetString("charId", char_id));
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
        // 从ScriptableObject中读取用户设置
        server_url = settingsData.server_url;
        user_id = settingsData.user_id;
        char_id = settingsData.char_id;
        pipeline_config = settingsData.pipeline_config;
        chars = new List<string>();
        foreach (CharacterSettingsData charData in settingsData.chars)
        {
            chars.Add(charData.character_name);
        }
        int char_index = chars.IndexOf(settingsData.char_id);
        char_id = settingsData.chars[char_index].character_name;
        scene_name = settingsData.chars[char_index].scene_name;

        PlayerPrefs.SetString("urlInput", server_url);
        PlayerPrefs.SetString("webrtcUrlInput", webrtc_url);
        PlayerPrefs.SetString("userId", user_id);
        PlayerPrefs.SetString("charId", char_id);
        PlayerPrefs.SetString("sceneName", scene_name);
        PlayerPrefs.SetString("pipelineConfigInput", pipeline_config);

        urlInput.text = server_url;
        webrtcUrlInput.text = webrtc_url;
        userIdInput.text = user_id;
        pipelineConfigInput.text = pipeline_config;
        charDropDown.ClearOptions();
        charDropDown.AddOptions(chars);
        charDropDown.value = char_index;

        if (userIdInput.text == "0")
        {
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
        startAction.Disable();
        string scene = scene_name;
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
        startAction.Enable();
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

    void Update()
    {
        current_volumn = MicrophoneManager.Instance.GetCurrentLoudness(0.5f);
        Display.value = ToLog(current_volumn);
    }
}
