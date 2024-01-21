using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelper : MonoBehaviour
{
    public UnityEvent OnAnimationRightHandEventTriggered;
    public UnityEvent OnAnimationLeftHandEventTriggered;
    public UnityEvent OnAttackPerformed;

    public void TriggerEventAtRightHand()
    {
        OnAnimationRightHandEventTriggered?.Invoke();
    }

    public void TriggerEventAtLeftHand()
    {
        OnAnimationLeftHandEventTriggered?.Invoke();
    }

    public void TriggerAttack()
    {
        OnAttackPerformed?.Invoke();
    }
}
