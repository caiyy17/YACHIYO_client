using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

public class EmotionManager : MonoBehaviour
{
    [System.Serializable]
    public class EmotionData
    {
        public EmotionDict emotionDict;
        public StringEvent motionEvent;
        public StringEvent expressionEvent;
    }
    public List<EmotionData> emotionDataList = new List<EmotionData>();
    
    private string currentEmotion;
    private float lastTime;
    public float emotionInterval = 10.0f;

    public void Start()
    {
        foreach (EmotionData emotionData in emotionDataList)
        {
            emotionData.emotionDict.Initialize();
        }
        currentEmotion = "idle";
    }

    public void SetMotionAndExpression(string emotion)
    {
        // if (emotion == currentEmotion && emotion == "idle")
        // 记一下时，一段时间内不再重复播放动画
        if (emotion == currentEmotion && Time.time - lastTime < emotionInterval){
            return;
        }
        if ((emotion == "idle" || emotion == "neutral") && Time.time - lastTime < emotionInterval){
            return;
        }
        currentEmotion = emotion;
        lastTime = Time.time;
        Debug.Log("Set emotion: " + emotion);

        foreach (EmotionData emotionData in emotionDataList)
        {
            if (emotionData.emotionDict.motionDict.ContainsKey(emotion))
            {
                // ramdomly select a motion from the list
                List<string> motions = emotionData.emotionDict.motionDict[emotion];
                string motion = motions[Random.Range(0, motions.Count)];
                // set the motion
                Debug.Log("Set motion: " + motion);
                // animator.SetTrigger(motion);
                if(emotionData.motionEvent != null){
                    emotionData.motionEvent.Invoke(motion);
                }
                
            }
            else
            {
                Debug.Log("Motion not found: " + emotion);
            }

            if (emotionData.emotionDict.expressionDict.ContainsKey(emotion))
            {
                // ramdomly select an expression from the list
                List<string> expressions = emotionData.emotionDict.expressionDict[emotion];
                string expression = expressions[Random.Range(0, expressions.Count)];
                // set the expression
                Debug.Log("Set expression: " + expression);
                // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
                if(emotionData.expressionEvent != null){
                    emotionData.expressionEvent.Invoke(expression);
                }
            }
            else
            {
                Debug.Log("Expression not found: " + emotion);
            }
        }
    }
}