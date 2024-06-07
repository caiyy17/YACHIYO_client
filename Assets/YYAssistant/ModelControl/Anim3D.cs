using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim3D : MonoBehaviour
{
    public Animator anim;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    //List of KeyCode and their corresponding motion trigger, serialized for Unity Editor
    public List<KeyCode> motionKeyCodes;
    public List<string> motionTriggers;
    public List<KeyCode> expressionKeyCodes;
    public List<int> expressionIndexes;
    public int mouthIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
    }

    // Update is called once per frame
    
    void LateUpdate()
    {
        PlayAnim();
        FaceExpression();
    }
    void PlayAnim()
    {
        for (int i = 0; i < motionKeyCodes.Count; i++)
        {
            if (Input.GetKeyDown(motionKeyCodes[i]))
            {
                anim.SetTrigger(motionTriggers[i]);
            }
        }
    }

    void FaceExpression()
    {
        for (int i = 0; i < expressionKeyCodes.Count; i++)
        {
            if (Input.GetKeyDown(expressionKeyCodes[i]))
            {
                skinnedMeshRenderer.SetBlendShapeWeight(expressionIndexes[i], 100);
            }
        }
    }
    
    public void MouthControl(float value)
    {
        skinnedMeshRenderer.SetBlendShapeWeight(mouthIndex, value * 100);
    }

    public void SetMotion(string motion)
    {
        // Debug.Log("Set motion: " + motion);
        anim.SetTrigger(motion);
    }

    public void SetExpression(string expression)
    {
        // Debug.Log("Set expression: " + expression);
        // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
    }
}
