using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SampleSceneUI : MonoBehaviour
{
    //Model selection
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

    // Start is called before the first frame update
    void Start()
    {
        nextModel.onClick.AddListener(NextModel);
        prevModel.onClick.AddListener(PrevModel);
        hideUI.onClick.AddListener(HideUI);
        playBGM.onClick.AddListener(PlayBGM);
        SettingButton.onClick.AddListener(ShowSetting);
        CloseSetting.onClick.AddListener(CloseSettingPanel);
        HomeButton.onClick.AddListener(ReturnHome);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NextModel()
    {
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
            //disable DynamicUI
            playBGM.GetComponent<DynamicUI>().enabled = false;
            playBGM.GetComponent<Image>().sprite = pauseTexture;
        }
        else
        {
            bgm.Play();
            //enable DynamicUI
            playBGM.GetComponent<DynamicUI>().enabled = true;
        }
    }

    void ShowSetting()
    {
        MainScreen.SetActive(false);
        SettingPanel.SetActive(true);
    }

    void CloseSettingPanel()
    {
        SettingPanel.SetActive(false);
        MainScreen.SetActive(true);
    }

    void ReturnHome()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
