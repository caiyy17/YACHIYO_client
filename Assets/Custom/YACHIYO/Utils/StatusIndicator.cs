using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Yachiyo
{
    [RequireComponent(typeof(SignalManager))]
    public class StatusIndicator : MonoBehaviour
    {
        SignalManager signalManager;
        [System.Serializable]
        public class TextColorPair
        {
            // 需要更改颜色的 UI Text 组件
            public string Text;
            // 对应的目标颜色
            public Color targetColor = Color.white;
        }
        public Image Indicator;
        public Color defaultColor = Color.white;
        public List<TextColorPair> textColorList = new List<TextColorPair>();
        public Dictionary<string, Color> textColorDict = new Dictionary<string, Color>();

        void Awake()
        {
            signalManager = GetComponent<SignalManager>();
        }

        public void Start()
        {
            signalManager.AddSignal("change_status", ApplyColor);
            // 初始化字典，将文本和颜色的对应关系存储在字典中
            foreach (TextColorPair pair in textColorList)
            {
                if (!textColorDict.ContainsKey(pair.Text))
                {
                    textColorDict.Add(pair.Text, pair.targetColor);
                }
            }
        }

        void OnDisable()
        {
            signalManager.RemoveSignal("change_status", ApplyColor);
        }

        public void ApplyColor(string text)
        {
            // 检查字典中是否存在对应的颜色
            if (textColorDict.TryGetValue(text, out Color color))
            {
                Indicator.color = color;
            }
            else
            {
                //pass
            }
        }
    }
}
