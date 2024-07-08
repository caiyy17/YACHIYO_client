using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContentLoader : MonoBehaviour
{
    public string imageName; // 图片名称，不带扩展名
    public string textToShow; // 要显示的文字
    public Image uiImage; // 要显示图片的UI Image组件
    public TextMeshProUGUI uiText; // 要显示文字的TextMeshPro组件
    public RectTransform contentRectTransform; // Content对象的RectTransform
    public string defaultImageName = "default"; // 默认图片名称，不带扩展名
    // 示例：在Start方法中加载图片
    void Start()
    {
        LoadImage(imageName);
        LoadText(textToShow);
    }

    // 加载并显示图片
    public void LoadImage(string imageName)
    {
        if(uiImage == null)
        {
            return;
        }
        // 从 Resources 文件夹中加载图片
        Texture2D texture = Resources.Load<Texture2D>("Images/" + imageName);
        if (texture == null)
        {
            Debug.LogError("Image not found: " + imageName);
            return;
        }

        // 创建Sprite并赋值给UI Image组件
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        uiImage.sprite = sprite;

        // 调整图片长宽比
        AdjustImageAspectRatio(texture.width, texture.height);
    }

    // 调整图片长宽比
    private void AdjustImageAspectRatio(float width, float height)
    {
        AspectRatioFitter aspectRatioFitter = uiImage.GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter != null)
        {
            aspectRatioFitter.aspectRatio = width / height;
        }
    }

    public void ClearImage()
    {
        // 加载默认图片
        LoadImage(defaultImageName);
    }

    public void LoadText(string textToShow)
    {
        if (uiText == null)
        {
            return;
        }
        // 显示文字
        uiText.text = textToShow;
        AdjustContentSize(); // 调整内容大小
    }

    public void AddText(string textToAdd)
    {
        if (uiText == null)
        {
            return;
        }
        // 添加文字
        uiText.text += textToAdd;
        AdjustContentSize(); // 调整内容大小
    }

    public void ClearText()
    {
        LoadText(""); // 清空文字
    }

    private void AdjustContentSize()
    {
        // 获取Text组件的高度
        float textHeight = uiText.preferredHeight;
        // 设置Content对象的高度以适应文本内容
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, textHeight);
    }
}