using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSettingsData", menuName = "YACHIO/GameSettingsData", order = 1)]
public class GameSettingsData : ScriptableObject
{
    public string server_url = "localhost:5050";
    public string webrtc_url = "localhost:5055";
    public string user_id = "0";
    public string pipeline_config = "demo_config";
    public string char_id = "0";
    public List<CharacterSettingsData> chars = new List<CharacterSettingsData>();
}