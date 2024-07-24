using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmotionDict", menuName = "YYAssistant/EmotionDict", order = 1)]
public class EmotionDict : ScriptableObject
{
    [Serializable]
    public class EmotionEntry
    {
        public string key;
        public int layer = 0;
        public List<string> values;
    }

    public List<EmotionEntry> motionList = new List<EmotionEntry>()
    {
        new EmotionEntry() { key = "neutral", values = new List<string> { "t1", "t4" } },
        new EmotionEntry() { key = "joy", values = new List<string> { "t4", "s1" } },
        new EmotionEntry() { key = "sadness", values = new List<string> { "t6", "t8" } },
        new EmotionEntry() { key = "anger", values = new List<string> { "t5", "s4" } },
        new EmotionEntry() { key = "fear", values = new List<string> { "t2" } },
        new EmotionEntry() { key = "disgust", values = new List<string> { "s6", "s3", "s5" } },
        new EmotionEntry() { key = "surprise", values = new List<string> { "t3", "t7", "s2" } },
        new EmotionEntry() { key = "listening", values = new List<string> { "idle" } },
        new EmotionEntry() { key = "thinking", values = new List<string> { "idle" } },
        new EmotionEntry() { key = "idle", values = new List<string> { "idle" } }
    };

    public List<EmotionEntry> expressionList = new List<EmotionEntry>()
    {
        new EmotionEntry() { key = "neutral", values = new List<string> { "1", "0" } },
        new EmotionEntry() { key = "joy", values = new List<string> { "0" } },
        new EmotionEntry() { key = "sadness", values = new List<string> { "4" } },
        new EmotionEntry() { key = "anger", values = new List<string> { "3" } },
        new EmotionEntry() { key = "fear", values = new List<string> { "4" } },
        new EmotionEntry() { key = "disgust", values = new List<string> { "5" } },
        new EmotionEntry() { key = "surprise", values = new List<string> { "6" } },
        new EmotionEntry() { key = "listening", values = new List<string> { "1" } },
        new EmotionEntry() { key = "thinking", values = new List<string> { "1" } },
        new EmotionEntry() { key = "idle", values = new List<string> { "1" } }
    };

    public class EmotionItem
    {
        public int layer = 0;
        public List<string> values;
    }

    public Dictionary<string, EmotionItem> motionDict;
    public Dictionary<string, EmotionItem> expressionDict;

    public void Initialize()
    {
        motionDict = new Dictionary<string, EmotionItem>();
        foreach (var entry in motionList)
        {
            motionDict[entry.key] = new EmotionItem() { layer = entry.layer, values = entry.values };
        }

        expressionDict = new Dictionary<string, EmotionItem>();
        foreach (var entry in expressionList)
        {
            expressionDict[entry.key] = new EmotionItem() { layer = entry.layer, values = entry.values };
        }
    }
}