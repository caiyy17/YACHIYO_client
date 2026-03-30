using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yachiyo
{
    /// <summary>
    /// Reusable mapping from action keys to trigger/expression variants.
    /// Create instances via Assets > Create > YACHIYO > ActionMap.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionMap", menuName = "YACHIYO/ActionMap")]
    public class ActionMap : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public int layer = 0;
            public List<string> values;
        }

        public List<Entry> entries = new List<Entry>();

        // Runtime dictionary (not serialized)
        [NonSerialized] private Dictionary<string, Entry> _dict;

        public void Initialize()
        {
            _dict = new Dictionary<string, Entry>();
            foreach (var e in entries)
                _dict[e.key] = e;
        }

        public bool TryGetEntry(string key, out Entry entry)
        {
            return _dict.TryGetValue(key, out entry);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
    }
}
