using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoaderWithProgress : MonoBehaviour
{
    public Slider progressBar;
    public TMP_Text progressText;

    // 开始加载场景的协程
    public void LoadScene(string sceneName)
    {
        // 显示进度条
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0f;
        progressText.text = "0%";
        StartCoroutine(LoadSceneAsyncWithCombinedProgress(sceneName));
    }

    private IEnumerator LoadSceneAsyncWithCombinedProgress(string sceneName)
    {
        // 开始异步加载场景
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 在场景加载完成之前，不允许自动切换场景
        operation.allowSceneActivation = false;

        // 初始化任务的总权重
        float initializationWeight = 0.6f;
        // 场景加载的总权重
        float loadingWeight = 1 - initializationWeight;

        // 执行初始化任务
        yield return StartCoroutine(PerformInitializationTasks(initializationWeight));

        // 更新进度条
        while (!operation.isDone)
        {
            // operation.progress is between 0.0f and 0.9f
            float loadingProgress = Mathf.Clamp01(operation.progress) * loadingWeight;
            progressBar.value = initializationWeight + loadingProgress;

            // 显示进度百分比
            if (progressText != null)
            {
                progressText.text = (progressBar.value * 100).ToString("F2") + "%";
            }

            // 如果加载进度达到90%，就认为加载完成
            if (operation.progress >= 0.9f)
            {
                // 允许场景激活
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator PerformInitializationTasks(float weight)
    {
        // 模拟一个初始化任务，分为多个步骤
        int steps = 5;
        for (int i = 0; i < steps; i++)
        {
            // 模拟每个步骤的耗时（这里使用WaitForSeconds）
            yield return new WaitForSeconds(0.2f);

            // 更新进度条（假设每个步骤占初始化总进度的相应比例）
            if (progressBar != null)
            {
                progressBar.value += weight / steps;
            }

            // 更新进度文本
            if (progressText != null)
            {
                progressText.text = (progressBar.value * 100).ToString("F2") + "%";
            }
        }

        // 所有初始化任务完成
        yield return null;
    }
}