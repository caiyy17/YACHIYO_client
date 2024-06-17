using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim2DSeq : MonoBehaviour
{
    public Animator animator;
    private List<int> triggerHashes;
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
        if (animator== null)
        {
            animator= new Animator();
        }
        if (skinnedMeshRenderer == null)
        {
            skinnedMeshRenderer = new SkinnedMeshRenderer();
        }
        CacheTriggerHashes();
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
                animator.SetTrigger(motionTriggers[i]);
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
        ResetAllTriggers();
        animator.SetTrigger(motion);
    }

    public void SetExpression(string expression)
    {
        // Debug.Log("Set expression: " + expression);
        // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
    }

    // 缓存所有触发器的哈希
    private void CacheTriggerHashes()
    {
        triggerHashes = new List<int>();
        AnimatorControllerParameter[] parameters = animator.parameters;
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                triggerHashes.Add(Animator.StringToHash(parameter.name));
            }
        }
    }

    // 调用此方法以重置所有触发器
    public void ResetAllTriggers()
    {
        foreach (int hash in triggerHashes)
        {
            animator.ResetTrigger(hash);
        }
    }
}
