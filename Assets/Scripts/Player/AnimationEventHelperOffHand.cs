using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperOffHand : MonoBehaviour
{
    public UnityEvent OnAnimationOffHandEventTriggered;
    public UnityEvent OnAttackOffHandPerformed;

    public void TriggerEventAtOffHand()
    {
        OnAnimationOffHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtOffHand()
    {
        OnAttackOffHandPerformed?.Invoke();
    }
}
