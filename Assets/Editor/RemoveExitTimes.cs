using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class RemoveAllExitTimes : MonoBehaviour
{
    [MenuItem("Tools/Remove All Exit Times")]
    public static void RemoveAllExitTimesFromAnimator()
    {
        AnimatorController animatorController = Selection.activeObject as AnimatorController;

        if (animatorController == null)
        {
            Debug.LogError("请选中一个Animator Controller对象");
            return;
        }

        foreach (AnimatorControllerLayer layer in animatorController.layers)
        {
            RemoveExitTimesFromStateMachine(layer.stateMachine);
            RemoveExitTimesFromAnyState(layer.stateMachine);
        }

        Debug.Log("已移除所有Exit Time");
    }

    private static void RemoveExitTimesFromStateMachine(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState state in stateMachine.states)
        {
            RemoveExitTimesFromState(state.state);
        }

        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
        {
            RemoveExitTimesFromStateMachine(childStateMachine.stateMachine);
            RemoveExitTimesFromAnyState(childStateMachine.stateMachine);
        }
    }

    private static void RemoveExitTimesFromState(AnimatorState state)
    {
        foreach (AnimatorStateTransition transition in state.transitions)
        {
            transition.hasExitTime = false;
        }
    }

    private static void RemoveExitTimesFromAnyState(AnimatorStateMachine stateMachine)
    {
        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
        {
            transition.hasExitTime = false;
        }
    }
}
