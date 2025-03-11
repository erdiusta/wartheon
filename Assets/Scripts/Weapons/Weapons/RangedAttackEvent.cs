using UnityEngine;
using UnityEngine.Events;

public class RangedAttackEvent : MonoBehaviour
{
    public UnityEvent OnAnimationRangedAttackAnimationTriggered;

    public void TriggerEventAtMainRangedHand()
    {
        OnAnimationRangedAttackAnimationTriggered?.Invoke();
    }
}