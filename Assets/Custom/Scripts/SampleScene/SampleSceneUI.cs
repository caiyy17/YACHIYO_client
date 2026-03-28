using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEngine.InputSystem;
using Yachiyo;

public class SampleSceneUI : MonoBehaviour
{
    //Home scene
    public string homeSceneName = "GameStart";

    //Model selection
    public Transform modelParent;
    public List<GameObject> modelList;
    public Button nextModel;
    public Button prevModel;
    int currentModelIndex = 0;

    //UI Hide
    public GameObject uiPanel;
    public Button hideUI;
    public Sprite buttonOnTexture;
    public Sprite buttonOffTexture;

    //BGM
    public AudioSource bgm;
    public Button playBGM;
    public Sprite playTexture;
    public Sprite pauseTexture;

    //Settings
    public Button SettingButton;
    public GameObject MainScreen;
    public GameObject SettingPanel;
    public Button CloseSetting;
    public Button HomeButton;

    public Slider SpeakingThresholdLow;
    public Slider SpeakingThresholdHigh;
    public Slider Display;

    public GameObject startingPanel;
    public TMP_Text startingText;

    [SerializeField] public InputAction homeAction, leftAction, rightAction;

    // Start is called before the first frame update
    void Start()
    {
        startingPanel.SetActive(true);

        nextModel.onClick.AddListener(NextModel);
        prevModel.onClick.AddListener(PrevModel);
        hideUI.onClick.AddListener(HideUI);
        playBGM.onClick.AddListener(PlayBGM);
        SettingButton.onClick.AddListener(ShowSetting);
        CloseSetting.onClick.AddListener(CloseSettingPanel);
        HomeButton.onClick.AddListener(ReturnHome);

        bool useBGM = PlayerPrefs.GetInt("useBGM", 1) == 1;
        bgm.Play();
        if (useBGM)
        {
            playBGM.GetComponent<DynamicUI>().enabled = true;
        }
        else
        {
            bgm.Pause();
            playBGM.GetComponent<DynamicUI>().enabled = false;
            playBGM.GetComponent<Image>().sprite = pauseTexture;
        }

        bool isHideUI = PlayerPrefs.GetInt("hideUI", 0) == 1;
        if (isHideUI)
        {
            uiPanel.SetActive(false);
            hideUI.GetComponent<Image>().sprite = buttonOffTexture;
        }
        else
        {
            uiPanel.SetActive(true);
            hideUI.GetComponent<Image>().sprite = buttonOnTexture;
        }

        homeAction.Enable();
        homeAction.performed += ctx => ReturnHome();

        leftAction.Enable();
        rightAction.Enable();
        leftAction.performed += ctx => PrevModel();
        rightAction.performed += ctx => NextModel();
    }

    void OnDisable()
    {
        nextModel.onClick.RemoveAllListeners();
        prevModel.onClick.RemoveAllListeners();
        hideUI.onClick.RemoveAllListeners();
        playBGM.onClick.RemoveAllListeners();
        SettingButton.onClick.RemoveAllListeners();
        CloseSetting.onClick.RemoveAllListeners();
        HomeButton.onClick.RemoveAllListeners();

        homeAction.Disable();
        leftAction.Disable();
        rightAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        float current_volumn = MicrophoneManager.Instance.GetCurrentLoudness(0.5f);
        Display.value = ToLog(current_volumn);
    }

    void NextModel()
    {
        if (modelParent != null) modelParent.rotation = Quaternion.identity;
        modelList[currentModelIndex].SetActive(false);
        currentModelIndex++;
        if (currentModelIndex >= modelList.Count)
        {
            currentModelIndex = 0;
        }
        modelList[currentModelIndex].SetActive(true);
    }

    void PrevModel()
    {
        if (modelParent != null) modelParent.rotation = Quaternion.identity;
        modelList[currentModelIndex].SetActive(false);
        currentModelIndex--;
        if (currentModelIndex < 0)
        {
            currentModelIndex = modelList.Count - 1;
        }
        modelList[currentModelIndex].SetActive(true);
    }

    void HideUI()
    {
        if (uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            hideUI.GetComponent<Image>().sprite = buttonOffTexture;
        }
        else
        {
            uiPanel.SetActive(true);
            hideUI.GetComponent<Image>().sprite = buttonOnTexture;
        }
    }

    public void PlayBGM()
    {
        if (bgm.isPlaying)
        {
            bgm.Pause();
            // disable DynamicUI
            playBGM.GetComponent<DynamicUI>().enabled = false;
            playBGM.GetComponent<Image>().sprite = pauseTexture;
        }
        else
        {
            bgm.UnPause();
            //enable DynamicUI
            playBGM.GetComponent<DynamicUI>().enabled = true;
        }
    }

    void ShowSetting()
    {
        // disable character collider
        if (modelParent != null) modelParent.GetComponent<BoxCollider>().enabled = false;
        MainScreen.SetActive(false);
        SettingPanel.SetActive(true);

        // set VAD toggle
        SpeakingThresholdLow.value = ToLog(PlayerPrefs.GetFloat("silenceThreshold", 0));
        SpeakingThresholdHigh.value = ToLog(PlayerPrefs.GetFloat("speakingThreshold", 1));
    }

    void CloseSettingPanel()
    {
        // enable character collider
        if (modelParent != null) modelParent.GetComponent<BoxCollider>().enabled = true;
        SettingPanel.SetActive(false);
        MainScreen.SetActive(true);
    }

    void ReturnHome()
    {
        homeAction.performed -= ctx => ReturnHome();
        SceneManager.LoadScene(homeSceneName, LoadSceneMode.Single);
    }

    float ToLog(float value)
    {
        return Math.Clamp((Mathf.Log10(value) + 5) / 5, 0, 1);
    }

    float ToExp(float value)
    {
        return Mathf.Pow(10, value * 5 - 5);
    }

    public void SetStartingPanel(string text)
    {
        if (text == "started")
        {
            startingPanel.SetActive(false);
        }
        else
        {
            startingPanel.SetActive(true);
            startingText.text = text;
        }
    }
}
