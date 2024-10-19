using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "YYAssistant/CharacterSettingsData", order = 1)]
public class CharacterSettingsData : ScriptableObject
{
    public string character_name = "YYAssistant";
    public string scene_name = "SampleScene3D";
    // JSON 文件引用
    public TextAsset pipeline_config;
}