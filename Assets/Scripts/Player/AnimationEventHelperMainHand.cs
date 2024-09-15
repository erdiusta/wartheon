using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperMainHand : MonoBehaviour
{
    public UnityEvent OnAnimationMainHandEventTriggered;
    public UnityEvent OnAttackOffHandPerformed;

    public void TriggerEventAtMainHand()
    {
        OnAnimationMainHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtMainHand()
    {
        OnAttackOffHandPerformed?.Invoke();
    }
}
