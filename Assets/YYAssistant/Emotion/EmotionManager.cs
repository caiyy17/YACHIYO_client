using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;


public class EmotionManager : MonoBehaviour
{
    public EmotionDict emotionDict;
    public StringEvent motionEvent;
    public StringEvent expressionEvent;
    private string currentEmotion;
    private float lastTime;

    public void Start()
    {
        emotionDict.Initialize();
        currentEmotion = "idle";
    }

    public void SetMotionAndExpression(string emotion)
    {
        // if (emotion == currentEmotion && emotion == "idle")
        // 记一下时，5秒钟内不再重复播放动画
        if (emotion == currentEmotion && Time.time - lastTime < 5){
            return;
        }
        currentEmotion = emotion;
        lastTime = Time.time;
        Debug.Log("Set emotion: " + emotion);
        if (emotionDict.motionDict.ContainsKey(emotion))
        {
            // ramdomly select a motion from the list
            List<string> motions = emotionDict.motionDict[emotion];
            string motion = motions[Random.Range(0, motions.Count)];
            // set the motion
            Debug.Log("Set motion: " + motion);
            // animator.SetTrigger(motion);
            if(motionEvent != null){
                motionEvent.Invoke(motion);
            }
            
        }
        else
        {
            Debug.Log("Motion not found: " + emotion);
        }

        if (emotionDict.expressionDict.ContainsKey(emotion))
        {
            // ramdomly select an expression from the list
            List<string> expressions = emotionDict.expressionDict[emotion];
            string expression = expressions[Random.Range(0, expressions.Count)];
            // set the expression
            Debug.Log("Set expression: " + expression);
            // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
            if(expressionEvent != null){
                expressionEvent.Invoke(expression);
            }
        }
        else
        {
            Debug.Log("Expression not found: " + emotion);
        }
    }
}