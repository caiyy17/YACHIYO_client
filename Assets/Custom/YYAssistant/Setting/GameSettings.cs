using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSettingsData", menuName = "YYAssistant/GameSettingsData", order = 1)]
public class GameSettingsData : ScriptableObject
{
    public string server_url ="localhost:5050";
    public string user_id = "0";
    public string char_id = "0";
    public List<CharacterSettingsData> chars = new List<CharacterSettingsData>();
}