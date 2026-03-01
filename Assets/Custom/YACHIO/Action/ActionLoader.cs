using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure action tool. Looks up action in dictionaries
/// and triggers motion/expression events.
/// </summary>
public class ActionLoader : MonoBehaviour
{
    [System.Serializable]
    public class ActionData
    {
        public ActionDict actionDict;
        public StringEvent motionEvent;
        public StringEvent expressionEvent;
    }
    public List<ActionData> actionDataList = new List<ActionData>();

    private List<string> lastActionList = new List<string>();
    private List<float> lastTimeList = new List<float>();
    public float actionInterval = 10.0f;

    void Awake()
    {
        foreach (ActionData actionData in actionDataList)
        {
            actionData.actionDict.Initialize();
            lastActionList.Add("idle");
            lastTimeList.Add(0);
        }
    }

    public void SetAction(string action)
    {
        int lastLayer, currentLayer;

        int i = 0;
        foreach (ActionData actionData in actionDataList)
        {
            if (actionData.actionDict.motionDict.ContainsKey(lastActionList[i]))
            {
                lastLayer = actionData.actionDict.motionDict[lastActionList[i]].layer;
            }
            else
            {
                lastLayer = -999;
            }

            if (actionData.actionDict.motionDict.ContainsKey(action))
            {
                currentLayer = actionData.actionDict.motionDict[action].layer;
                if (currentLayer >= lastLayer || Time.time - lastTimeList[i] > actionInterval)
                {
                    List<string> motions = actionData.actionDict.motionDict[action].values;
                    string motion = motions[Random.Range(0, motions.Count)];
                    if (actionData.motionEvent != null)
                    {
                        actionData.motionEvent.Invoke(motion);
                    }
                    lastTimeList[i] = Time.time;
                    lastActionList[i] = action;
                }
            }

            if (actionData.actionDict.expressionDict.ContainsKey(action))
            {
                List<string> expressions = actionData.actionDict.expressionDict[action].values;
                string expression = expressions[Random.Range(0, expressions.Count)];
                if (actionData.expressionEvent != null)
                {
                    actionData.expressionEvent.Invoke(expression);
                }
            }
            i++;
        }
        Debug.Log("Set action: " + action);
    }
}
