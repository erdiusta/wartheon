using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperRight : MonoBehaviour
{
    public UnityEvent OnAnimationRightHandEventTriggered;
    public UnityEvent OnAttackRightHandPerformed;

    public void TriggerEventAtRightHand()
    {
        OnAnimationRightHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtRightHand()
    {
        OnAttackRightHandPerformed?.Invoke();
    }
}
