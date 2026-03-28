using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yachiyo
{
    public class Anim3D : MonoBehaviour
    {
        private Animator anim;
        private List<int> triggerHashes = new List<int>();
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
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                anim = new Animator();
            }
            CacheTriggerHashes();
        }

        public void MouthControl(float value)
        {
            if (skinnedMeshRenderer == null) return;
            skinnedMeshRenderer.SetBlendShapeWeight(mouthIndex, value * 100);
        }

        public void SetMotion(string motion)
        {
            // 如果当前组件是激活的
            if (gameObject.activeInHierarchy)
            {
                // Debug.Log("Set motion: " + motion);
                ResetAllTriggers();
                anim.SetTrigger(motion);
            }
        }

        public void SetExpression(string expression)
        {
            // Debug.Log("Set expression: " + expression);
            // expressionController.CurrentExpressionIndex = (int)expression[0] - '0';
        }

        private void CacheTriggerHashes()
        {
            triggerHashes = new List<int>();
            AnimatorControllerParameter[] parameters = anim.parameters;
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
                anim.ResetTrigger(hash);
            }
        }
    }
}
