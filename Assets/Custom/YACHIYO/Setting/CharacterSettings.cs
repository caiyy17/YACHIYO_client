using UnityEngine;
using System.Collections.Generic;

namespace Yachiyo
{
    [CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "YACHIYO/CharacterSettingsData", order = 1)]
    public class CharacterSettingsData : ScriptableObject
    {
        public string character_name = "YACHIYO";
        public string scene_name = "SampleScene3D";
    }
}
