using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperMainHand : MonoBehaviour
{
    public UnityEvent OnAnimationMainHandEventTriggered;
    public UnityEvent OnAttackMainHandPerformed;

    public void TriggerEventAtMainHand()
    {
        OnAnimationMainHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtMainHand()
    {
        OnAttackMainHandPerformed?.Invoke();
    }
}
