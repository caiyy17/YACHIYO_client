using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;

namespace Yachiyo
{
    /// <summary>
    /// Pipeline module that displays content and forwards message as-is.
    /// Only fields listed in displayFields are shown (with color). All data is forwarded unmodified.
    /// </summary>
    public class ContentModule : ProcessingModuleSynchronous
    {
        [System.Serializable]
        public class DisplayField
        {
            public string name;
            public Color color = Color.white;
        }

        [SerializeField] private TextMeshProUGUI uiText;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] private List<DisplayField> displayFields = new List<DisplayField>();

        private Dictionary<string, Color> _fieldColorMap;

        void Awake()
        {
            moduleName = "ContentModule";
            capturedSignals = new List<string> { "SoS", "EoS" };
            _fieldColorMap = new Dictionary<string, Color>();
            foreach (var field in displayFields)
            {
                if (!string.IsNullOrEmpty(field.name))
                    _fieldColorMap[field.name] = field.color;
            }
        }

        protected override void ProcessMessage(string message)
        {
            YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

            if (baseMessage.signal == "SoS")
            {
                ClearText();
                outputQueue.Add(message);
                return;
            }

            if (baseMessage.signal == "EoS")
            {
                outputQueue.Add(message);
                return;
            }

            if (!string.IsNullOrEmpty(baseMessage.content))
            {
                var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
                AppendDisplayFields(jsonDict);
                outputQueue.Add(message);
            }
        }

        protected override void CustomCancel(string message)
        {
            ClearText();
        }

        private void AppendDisplayFields(Dictionary<string, object> jsonDict)
        {
            if (uiText == null) return;

            foreach (var kv in _fieldColorMap)
            {
                if (jsonDict.TryGetValue(kv.Key, out object value) && value != null)
                {
                    string text = value.ToString();
                    if (string.IsNullOrEmpty(text)) continue;
                    string hex = ColorUtility.ToHtmlStringRGB(kv.Value);
                    uiText.text += $"<color=#{hex}>{text}</color>";
                }
            }
            AdjustContentSize();
        }

        private void ClearText()
        {
            if (uiText == null) return;
            uiText.text = "";
            AdjustContentSize();
        }

        private void AdjustContentSize()
        {
            if (contentRectTransform == null) return;
            float textHeight = uiText.preferredHeight;
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, textHeight);
        }
    }
}
