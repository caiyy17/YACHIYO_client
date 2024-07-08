using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonKeySimulator : MonoBehaviour
{
    private bool isButtonHeld = false;
    private bool wasButtonHeld = false;

    void Start()
    {
        // 获取 EventTrigger 组件
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }

        // 创建按下事件
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        trigger.triggers.Add(pointerDownEntry);

        // 创建释放事件
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        trigger.triggers.Add(pointerUpEntry);
    }

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("Button Down");
        isButtonHeld = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        Debug.Log("Button Up");
        isButtonHeld = false;
    }

    public bool GetKeyDown()
    {
        return isButtonHeld && !wasButtonHeld;
    }

    public bool GetKeyUp()
    {
        return !isButtonHeld && wasButtonHeld;
    }

    void LateUpdate()
    {
        wasButtonHeld = isButtonHeld;
    }
}
