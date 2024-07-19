using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSettingsData", menuName = "YYAssistant/GameSettingsData", order = 1)]
public class GameSettingsData : ScriptableObject
{
    public string server_url ="http://localhost:5050";
    public string user_id = "0";
    public string scene_name = "SampleScene3D";
    public string system_message = "你是一个人工智能助手，你可以回答一些问题，也可以帮助用户完成一些任务。";
    public bool clear_history = true;
    public List<CharacterSettingsData> chars = new List<CharacterSettingsData>();
}