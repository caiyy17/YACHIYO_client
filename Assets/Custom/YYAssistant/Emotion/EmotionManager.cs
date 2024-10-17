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
    
    private string lastEmotion;
    private float lastTime;
    public float emotionInterval = 10.0f;
    public float idelInterval = 10.0f;

    public void Awake()
    {
        foreach (EmotionData emotionData in emotionDataList)
        {
            emotionData.emotionDict.Initialize();
        }
        lastEmotion = "idle";
    }

    public void Update()
    {
        if(Time.time - lastTime > idelInterval && lastEmotion == "idle"){
            SetMotionAndExpression("idle");
        }
    }

    public void SetMotionAndExpression(string emotion)
    {
        int lastLayer, currentLayer;

        foreach (EmotionData emotionData in emotionDataList)
        {
            lastLayer = 0;
            if (emotionData.emotionDict.motionDict.ContainsKey(lastEmotion))
            {
                lastLayer = emotionData.emotionDict.motionDict[lastEmotion].layer;
            }
            if (emotionData.emotionDict.motionDict.ContainsKey(emotion))
            {
                currentLayer = emotionData.emotionDict.motionDict[emotion].layer;
                if (currentLayer >= lastLayer || Time.time - lastTime > emotionInterval)
                {
                    // ramdomly select a motion from the list
                    List<string> motions = emotionData.emotionDict.motionDict[emotion].values;
                    string motion = motions[Random.Range(0, motions.Count)];
                    // set the motion
                    // Debug.Log("Set motion: " + motion);
                    if(emotionData.motionEvent != null){
                        emotionData.motionEvent.Invoke(motion);
                    }
                }
            }
            else
            {
                // Debug.Log("Motion not found: " + emotion);
            }

            if (emotionData.emotionDict.expressionDict.ContainsKey(emotion))
            {
                // ramdomly select an expression from the list
                List<string> expressions = emotionData.emotionDict.expressionDict[emotion].values;;
                string expression = expressions[Random.Range(0, expressions.Count)];
                // set the expression
                // Debug.Log("Set expression: " + expression);
                // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
                if(emotionData.expressionEvent != null){
                    emotionData.expressionEvent.Invoke(expression);
                }
            }
            else
            {
                // Debug.Log("Expression not found: " + emotion);
            }
        }

        lastEmotion = emotion;
        lastTime = Time.time;
        Debug.Log("Set emotion: " + emotion);
    }
}