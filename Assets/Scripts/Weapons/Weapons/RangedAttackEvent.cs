using UnityEngine;
using UnityEngine.Events;

public class RangedAttackEvent : MonoBehaviour
{
    public UnityEvent OnAnimationRangedAttackAnimationTriggered;

    public void TriggerEventAtMainRangedHand()
    {
        Debug.Log("ANIMATION EVENT");

        OnAnimationRangedAttackAnimationTriggered?.Invoke();
    }
}