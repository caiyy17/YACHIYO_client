using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "YYAssistant/CharacterSettingsData", order = 1)]
public class CharacterSettingsData : ScriptableObject
{
    public string character_name = "YYAssistant";
    public string character_voice = "";
    public string character_model = "";
    public string character_config = "";
    public string scene_name = "SampleScene3D";
    public string system_message = "你是一个人工智能助手，你可以回答一些问题，也可以帮助用户完成一些任务。";
}