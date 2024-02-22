using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthEvent : MonoBehaviour
{
    public event Action<HealthEvent, HealthEventArgs> OnHealthChanged;

    public void CallHealthChangedEvent(float healthPercent, int healthAmount, int damageAmount)
    {
        OnHealthChanged?.Invoke(this, new HealthEventArgs { healthPercent = healthPercent, healthAmount = healthAmount, damageAmount = damageAmount });
    }

    public event Action<HealthEvent> GetPoisoned;

    public void CallGetPosionedEvent()
    {
        GetPoisoned?.Invoke(this);
    }

    public event Action<HealthEvent> GetAcid;

    public void CallGetAcidEvent()
    {
        GetAcid?.Invoke(this);
    }

    public event Action<HealthEvent> GetStun;

    public void CallGetStunEvent()
    {
        GetStun?.Invoke(this);
    }

    public event Action<HealthEvent> PoisonCured;

    public void CallPoisonCuredEvent()
    {
        PoisonCured?.Invoke(this);
    }

    public event Action<HealthEvent> AcidCured;

    public void CallAcidCuredEvent()
    {
        AcidCured?.Invoke(this);
    }

    public event Action<HealthEvent> StunCured;

    public void CallStunCuredEvent()
    {
        StunCured?.Invoke(this);
    }

    public event Action<HealthEvent> OnCriticalHit;

    public void CallCriticalHitEvent()
    {
        OnCriticalHit?.Invoke(this);
    }
}

public class HealthEventArgs : EventArgs
{
    public float healthPercent;
    public int healthAmount;
    public int damageAmount;
}

