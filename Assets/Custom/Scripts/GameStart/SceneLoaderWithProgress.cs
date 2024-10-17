using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using TMPro;

public class SceneLoaderWithProgress : MonoBehaviour
{
    public Slider progressBar;
    public TMP_Text progressText;
    public SetupPipeline preparation;
    public UnityEvent<string> onErrorMessage;

    public float init_weight = 0.8f;
    public float load_weight = 0.2f;

    public float current_loading_progress = 0.0f;
    public string current_loading_status = "";

    void Update()
    {
        if (preparation.errorOccurred)
        {
            StopAllCoroutines();
            Debug.LogError(preparation.errorMessage);
            progressBar.gameObject.SetActive(false);
            onErrorMessage.Invoke(preparation.errorMessage);
            current_loading_progress = 0.0f;
            current_loading_status = "";
            preparation.errorOccurred = false;
        }

        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        progressBar.value = preparation.current_progress * init_weight + current_loading_progress * load_weight;
        progressText.text = preparation.current_status + current_loading_status;
    }

    // 开始加载场景的协程
    public void LoadScene(string sceneName)
    {
        // 显示进度条
        progressBar.gameObject.SetActive(true);
        StartCoroutine(LoadSceneAsyncWithCombinedProgress(sceneName));
    }

    private IEnumerator LoadSceneAsyncWithCombinedProgress(string sceneName)
    {
        // 执行初始化任务
        yield return StartCoroutine(PerformInitializationTasks());

        // 开始异步加载场景
        current_loading_progress = 0.0f;
        current_loading_status = "Loading... " + (int)(current_loading_progress * 100) + "%";
        UpdateProgressBar();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        // 在场景加载完成之前，不允许自动切换场景
        operation.allowSceneActivation = false;

        // 更新进度条
        while (!operation.isDone)
        {
            // operation.progress is between 0.0f and 0.9f
            current_loading_progress = operation.progress;

            // 显示进度百分比
            current_loading_status = "Loading... " + (int)(current_loading_progress * 100) + "%";

            // 如果加载进度达到90%，就认为加载完成
            if (operation.progress >= 0.9f)
            {
                // 允许场景激活
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator PerformInitializationTasks()
    {
        if (preparation != null)
        {
            yield return preparation.AssistantSetup();
        }
        // 所有初始化任务完成
        yield return null;
    }
}