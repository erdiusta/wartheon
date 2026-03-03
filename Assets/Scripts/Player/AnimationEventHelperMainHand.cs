using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelperMainHand : MonoBehaviour
{
    public UnityEvent OnAnimationMainHandEventTriggered;
    public UnityEvent OnAttackMainHandPerformed;
    public UnityEvent OnSeismicSlamSoundTriggered;
    public UnityEvent OnSeismicSlamTriggered;
    public UnityEvent OnShieldBashEventCompleted;
    public UnityEvent OnShieldBashEventPerformed;
    public UnityEvent OnShatterCryTriggered;

    public void TriggerEventAtMainHand()
    {
        OnAnimationMainHandEventTriggered?.Invoke();
    }

    public void TriggerAttackAtMainHand()
    {
        if (OnAttackMainHandPerformed == null)
        {
            Debug.LogError("OnAttackMainHandPerformed is null!");
        }

        OnAttackMainHandPerformed?.Invoke();
    }

    public void TriggerSeismicSlam()
    {
        OnSeismicSlamTriggered?.Invoke();
    }

    public void TriggerSeismicSlamSound()
    {
        OnSeismicSlamSoundTriggered?.Invoke();
    }

    public void TriggerShieldBashCompleted()
    {
        OnShieldBashEventCompleted?.Invoke();
    }

    public void TriggerShieldBash()
    {
        OnShieldBashEventPerformed?.Invoke();
    }

    public void TriggerShatterCry()
    {
        OnShatterCryTriggered?.Invoke();
    }
}
