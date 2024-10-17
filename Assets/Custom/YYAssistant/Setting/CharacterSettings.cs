using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "YYAssistant/CharacterSettingsData", order = 1)]
public class CharacterSettingsData : ScriptableObject
{
    public string character_name = "YYAssistant";
    public string scene_name = "SampleScene3D";
    // JSON 文件引用
    public TextAsset jsonFile;

    // 用私有字段来存储pipeline_config
    [TextArea(1, 1000)]
    [SerializeField] 
    private string _pipeline_config = "";

    // 使用属性来访问pipeline_config，并在每次访问时自动加载JSON
    public string pipeline_config
    {
        get
        {
            // 检查jsonFile是否有内容
            if (jsonFile != null)
            {
                _pipeline_config = jsonFile.text;
            }
            else
            {
                Debug.LogError("JSON文件未分配");
            }
            return _pipeline_config;
        }
    }
}