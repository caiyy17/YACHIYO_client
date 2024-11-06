using UnityEngine;

public class EyeAnim : MonoBehaviour
{
    public bool isActive = true; // 是否启用眨眼
    public float closeDuration = 0.1f; // 闭眼时间
    public float openDuration = 0.2f; // 张开时间
    public float closedHoldTime = 0.05f; // 闭合后短暂停留时间
    public float interval = 10f;

    private AnimationCurve closeCurve; // 闭眼的动画曲线
    private AnimationCurve openCurve; // 张开的动画曲线

    public FloatEvent eyeEvent;
    private float timer = 0f;
    private float nextTime = 0f;
    private enum BlinkState { Idle, Closing, ClosedHold, Opening }
    private BlinkState blinkState = BlinkState.Idle;

    private void Start()
    {
        // 定义闭眼和张开的动画曲线
        closeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 快速闭合
        openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 缓慢张开
    }

    private void LateUpdate()
    {
        if (!isActive)
        {
            return;
        }
        switch (blinkState)
        {
            case BlinkState.Idle:
                // 随机等待下次眨眼
                if (timer >= nextTime)
                {
                    timer = 0f;
                    blinkState = BlinkState.Closing;
                }
                else
                {
                    timer += Time.deltaTime;
                }
                break;

            case BlinkState.Closing:
                timer += Time.deltaTime;
                float closeProgress = Mathf.Clamp01(timer / closeDuration);
                float closeValue = closeCurve.Evaluate(closeProgress); // 根据曲线调整闭眼速度
                eyeEvent.Invoke(closeValue);
                // Debug.Log($"close: {closeValue}");

                if (closeProgress >= 1f)
                {
                    timer = 0f;
                    blinkState = BlinkState.ClosedHold;
                }
                break;

            case BlinkState.ClosedHold:
                timer += Time.deltaTime;

                if (timer >= closedHoldTime)
                {
                    timer = 0f;
                    blinkState = BlinkState.Opening;
                }
                break;

            case BlinkState.Opening:
                timer += Time.deltaTime;
                float openProgress = Mathf.Clamp01(timer / openDuration);
                float openValue = openCurve.Evaluate(openProgress); // 根据曲线调整张开速度
                eyeEvent.Invoke(openValue);
                // Debug.Log($"open: {openValue}");

                if (openProgress >= 1f)
                {
                    blinkState = BlinkState.Idle;
                    nextTime = Random.Range(0f, interval);
                }
                break;
        }
    }
}
