using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yachiyo
{
    [CreateAssetMenu(fileName = "ActionDict", menuName = "YACHIYO/ActionDict", order = 1)]
    public class ActionDict : ScriptableObject
    {
        [Serializable]
        public class ActionEntry
        {
            public string key;
            public int layer = 0;
            public List<string> values;
        }

        public List<ActionEntry> motionList = new List<ActionEntry>()
        {
            new ActionEntry() { key = "listening", values = new List<string> { "idle" } },
            new ActionEntry() { key = "answering", values = new List<string> { "idle" } },
            new ActionEntry() { key = "idle", values = new List<string> { "idle" } }
        };

        public List<ActionEntry> expressionList = new List<ActionEntry>()
        {
            new ActionEntry() { key = "listening", values = new List<string> { "1" } },
            new ActionEntry() { key = "thinking", values = new List<string> { "1" } },
            new ActionEntry() { key = "idle", values = new List<string> { "1" } }
        };

        public class ActionItem
        {
            public int layer = 0;
            public List<string> values;
        }

        public Dictionary<string, ActionItem> motionDict;
        public Dictionary<string, ActionItem> expressionDict;

        public void Initialize()
        {
            motionDict = new Dictionary<string, ActionItem>();
            foreach (var entry in motionList)
            {
                motionDict[entry.key] = new ActionItem() { layer = entry.layer, values = entry.values };
            }

            expressionDict = new Dictionary<string, ActionItem>();
            foreach (var entry in expressionList)
            {
                expressionDict[entry.key] = new ActionItem() { layer = entry.layer, values = entry.values };
            }
        }
    }
}
