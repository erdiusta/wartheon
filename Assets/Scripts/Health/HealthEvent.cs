using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthEvent : MonoBehaviour
{
    public event Action<HealthEvent, HealthEventArgs> OnHealthChanged;

    public void CallHealthChangedEvent(int healthAmount, int damageAmount, MeleeHand hand)
    {
        OnHealthChanged?.Invoke(this, new HealthEventArgs { healthAmount = healthAmount, damageAmount = damageAmount, hand = hand});
    }

    public event Action<HealthEvent> GetPoisoned;

    public void CallGetPoisonedEvent()
    {
        GetPoisoned?.Invoke(this);
    }

    public event Action<HealthEvent> GetBurned;

    public void CallGetBurnEvent()
    {
        GetBurned?.Invoke(this);
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

    public event Action<HealthEvent> GetFrost;

    public void CallGetFrostEvent()
    {
        GetFrost?.Invoke(this);
    }

    public event Action<HealthEvent> GetShattered;

    public void CallGetShatteredEvent()
    {
        GetShattered?.Invoke(this);
    }

    public event Action<HealthEvent> GetCursed;

    public void CallGetCurseEvent()
    {
        GetCursed?.Invoke(this);
    }

    public event Action<HealthEvent> GetDeath;

    public void CallGetDeathEvent()
    {
        GetDeath?.Invoke(this);
    }

    public event Action<HealthEvent> GetBlind;

    public void CallGetBlindEvent()
    {
        GetBlind?.Invoke(this);
    }

    public event Action<HealthEvent> BurnCured;

    public void CallBurnCuredEvent()
    {
        BurnCured?.Invoke(this);
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

    public event Action<HealthEvent> FrostCured;

    public void CallFrostCuredEvent()
    {
        FrostCured?.Invoke(this);
    }

    public event Action<HealthEvent> CurseCured;

    public void CallCurseCuredEvent()
    {
        CurseCured?.Invoke(this);
    }

    public event Action<HealthEvent> BlindCured;

    public void CallBlindCuredEvent()
    {
        BlindCured?.Invoke(this);
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

    public event Action<HealthEvent> OnLightFeetActive;

    public void CallGetLightFeetEvent()
    {
        OnLightFeetActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnLightFeetWoreOff;

    public void CallLightFeetWoreOffEvent()
    {
        OnLightFeetWoreOff?.Invoke(this);
    }

    public event Action<HealthEvent> OnParried;

    public void CallParryEvent()
    {
        OnParried?.Invoke(this);

        SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.parrySoundEffect);
    }

    public event Action<HealthEvent> OnDodged;

    public void CallDodgeEvent()
    {
        OnDodged?.Invoke(this);

        SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.dodgeSoundEffect);
    }

    public event Action<HealthEvent> OnBlocked;

    public void CallBlockEvent()
    {
        OnBlocked?.Invoke(this);

        SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.blockSoundEffect);
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
    public int healthAmount;
    public int damageAmount;
    public MeleeHand hand;
}

