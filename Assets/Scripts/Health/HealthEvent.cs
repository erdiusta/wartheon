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

    public event Action<HealthEvent> OnGraceOfTheUnscarredActive;

    // CAELION
    public void CallGraceOfTheUnscarredSpecialMoveEvent()
    {
        OnGraceOfTheUnscarredActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnGraceOfTheUnscarredEnded;

    public void CallGraceOfTheUnscarredSpecialMoveEndedEvent()
    {
        OnGraceOfTheUnscarredEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnValorActive;

    public void CallValorSpecialMoveEvent()
    {
        OnValorActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnValorEffectEnded;

    public void CallValorWoreOffEvent()
    {
        OnValorEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnBreakTheLineActive;

    public void CallBreakTheLineSpecialMoveEvent()
    {
        OnBreakTheLineActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnBreakTheLineEffectEnded;

    public void CallBreakTheLineWoreOffEvent()
    {
        OnBreakTheLineEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnGuardedOathActive;

    public void CallGuardedOathSpecialMoveEvent()
    {
        OnGuardedOathActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnGuardedOathEffectEnded;

    public void CallGuardedOathSpecialMoveEndEvent()
    {
        OnGuardedOathEffectEnded?.Invoke(this);
    }

    // MORVEN
    public event Action<HealthEvent> OnUmbralMistActive;

    public void CallUmbralMistSpecialMoveEvent()
    {
        OnUmbralMistActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnUmbralMistEffectEnded;

    public void CallUmbralMistWoreOffEvent()
    {
        OnUmbralMistEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnStealthActive;

    public void CallStealthSpecialMoveEvent()
    {
        OnStealthActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnStealthEffectEnded;

    public void CallStealthWoreOffEvent()
    {
        OnStealthEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnShadowStepActive;

    public void CallShadowStepSpecialMoveEvent()
    {
        OnShadowStepActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnShadowStepEffectEnded;

    public void CallShadowStepWoreOffEvent()
    {
        OnShadowStepEffectEnded?.Invoke(this);
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

