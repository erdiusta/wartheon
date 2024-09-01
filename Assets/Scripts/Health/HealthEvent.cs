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

    public void CallGetPoisonedEvent()
    {
        GetPoisoned?.Invoke(this);
    }

    public event Action<HealthEvent> GetBleeding;

    public void CallGetBleedingEvent()
    {
        GetBleeding?.Invoke(this);
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

    public event Action<HealthEvent> GetSlow;

    public void CallGetSlowEvent()
    {
        GetSlow?.Invoke(this);
    }



    public event Action<HealthEvent> GetDeath;

    public void CallGetDeathEvent()
    {
        GetDeath?.Invoke(this);
    }

    public event Action<HealthEvent> PoisonCured;

    public void CallPoisonCuredEvent()
    {
        PoisonCured?.Invoke(this);
    }

    public event Action<HealthEvent> BleedingCured;

    public void CallBleedingCuredEvent()
    {
        BleedingCured?.Invoke(this);
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

    public event Action<HealthEvent> SlowCured;

    public void CallSlowCuredEvent()
    {
        SlowCured?.Invoke(this);
    }

    public event Action<HealthEvent> GetBlockSpecialMove;

    public void CallGetBlockSpecialMoveEvent()
    {
        GetBlockSpecialMove?.Invoke(this);
    }

    public event Action<HealthEvent> BlockSpecialMoveDurationEnded;

    public void CallArmorWoreOffEvent()
    {
        BlockSpecialMoveDurationEnded?.Invoke(this);
    }

    public event Action<HealthEvent> GetGemSkinSpecialMove;

    public void CallGetGemSkinSpecialMoveEvent()
    {
        GetGemSkinSpecialMove?.Invoke(this);
    }

    public event Action<HealthEvent> OnGemSkinSpecialMoveEnded;

    public void CallGemSkinSpecialMoveEndEvent()
    {
        OnGemSkinSpecialMoveEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnCriticalHit;

    public void CallCriticalHitEvent()
    {
        OnCriticalHit?.Invoke(this);
    }

    public event Action<HealthEvent> OnHeadShot;

    public void CallHeadShotEvent()
    {
        OnHeadShot?.Invoke(this);
    }
}

public class HealthEventArgs : EventArgs
{
    public float healthPercent;
    public int healthAmount;
    public int damageAmount;
}

