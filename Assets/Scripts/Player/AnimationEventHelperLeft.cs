using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperLeft : MonoBehaviour
{
    public UnityEvent OnAnimationLeftHandEventTriggered;
    public UnityEvent OnAttackLeftHandPerformed;

    public void TriggerEventAtLeftHand()
    {
        OnAnimationLeftHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtLeftHand()
    {
        OnAttackLeftHandPerformed?.Invoke();
    }
}
