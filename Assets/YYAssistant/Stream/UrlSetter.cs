using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UrlSetter : MonoBehaviour
{
    public TMP_InputField urlInputField; // 引用InputField组件
    public YYAssistant assistant;

    void Start()
    {
        if (urlInputField != null)
        {
            // 初始化输入框的默认值
            urlInputField.text = assistant.addr;
            // 添加输入框的值变化监听器
            urlInputField.onValueChanged.AddListener(OnURLChanged);
        }
    }

    void OnURLChanged(string text)
    {
        assistant.SetUrl(text);
    }
}