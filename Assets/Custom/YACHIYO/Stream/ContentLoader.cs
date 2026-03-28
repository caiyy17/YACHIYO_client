using UnityEngine;
using TMPro;

namespace Yachiyo
{
    public class ContentLoader : MonoBehaviour
    {
        public string textToShow;
        public TextMeshProUGUI uiText;
        public RectTransform contentRectTransform;

        void Start()
        {
            LoadText(textToShow);
        }

        public void LoadText(string textToShow)
        {
            if (uiText == null)
            {
                return;
            }
            uiText.text = textToShow;
            AdjustContentSize();
        }

        public void AddText(string textToAdd)
        {
            if (uiText == null)
            {
                return;
            }
            uiText.text += textToAdd;
            AdjustContentSize();
        }

        private void AdjustContentSize()
        {
            float textHeight = uiText.preferredHeight;
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, textHeight);
        }
    }
}
