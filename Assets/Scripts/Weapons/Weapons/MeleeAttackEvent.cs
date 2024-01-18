using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent> OnMeleeAttack;

    public void CallMeleeAttackEvent()
    {
        OnMeleeAttack?.Invoke(this);
    }
}
