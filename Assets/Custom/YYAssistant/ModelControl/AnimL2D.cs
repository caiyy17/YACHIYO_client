using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.MouthMovement;
using UnityEngine;

public class AnimL2D : MonoBehaviour
{
    private Animator anim;
    private CubismModel model;
    private Live2D.Cubism.Framework.Expression.CubismExpressionController expressionController;

    public List<KeyCode> motionKeyCodes;
    public List<string> motionTriggers;
    public List<KeyCode> expressionKeyCodes;
    public List<int> expressionIndexes;
    public string mouthParam = "PARAM_MOUTH_OPEN_Y";

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        
        anim = GetComponent<Animator>();
        model = GetComponent<CubismModel>();
        expressionController = GetComponent<Live2D.Cubism.Framework.Expression.CubismExpressionController>();

        if (anim == null)
        {
            anim = new Animator();
        }
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
                expressionController.CurrentExpressionIndex = expressionIndexes[i];
            }
        }
    }
    
    public void MouthControl(float value)
    {
        model.Parameters.FindById(mouthParam).Value = value;
    }

    public void SetMotion(string motion)
    {
        if (gameObject.activeSelf)
        {
            anim.SetTrigger(motion);
        }
    }

    public void SetExpression(string expression)
    {
        if (gameObject.activeSelf)
        {
            int index;
            int.TryParse(expression, out index);
            expressionController.CurrentExpressionIndex = index;
        }
    }
}
