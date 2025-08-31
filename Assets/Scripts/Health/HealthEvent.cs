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

    public event Action<HealthEvent> GetWarm;

    public void CallGetWarmedEvent()
    {
        GetWarm?.Invoke(this);
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

    public event Action<HealthEvent> GetBleeding;

    public void CallGetBleedingEvent()
    {
        GetBleeding?.Invoke(this);
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

    public event Action<HealthEvent> GetRoot;

    public void CallGetRootEvent()
    {
        GetRoot?.Invoke(this);
    }

    public event Action<HealthEvent> GetChill;

    public void CallGetChillEvent()
    {
        GetChill?.Invoke(this);
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


    public event Action<HealthEvent> GetStatic;

    public void CallGetStaticEvent()
    {
        GetStatic?.Invoke(this);
    }

    public event Action<HealthEvent> GetParalyzed;

    public void CallGetParalyzedEvent()
    {
        GetParalyzed?.Invoke(this);
    }

    public event Action<HealthEvent> GetCursed;

    public void CallGetCurseEvent()
    {
        GetCursed?.Invoke(this);
    }

    public event Action<HealthEvent> GetFeared;

    public void CallGetFearEvent()
    {
        GetFeared?.Invoke(this);
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

    public event Action<HealthEvent> CuredCompletely;

    public void CallCuredCompletelyEvent()
    {
        CuredCompletely?.Invoke(this);
    }

    public event Action<HealthEvent> WarmCured;

    public void CallWarmCuredEvent()
    {
        WarmCured?.Invoke(this);
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

    public event Action<HealthEvent> BleedingCured;

    public void CallBleedingCuredEvent()
    {
        BleedingCured?.Invoke(this);
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

    public event Action<HealthEvent> RootCured;

    public void CallRootCuredEvent()
    {
        RootCured?.Invoke(this);
    }

    public event Action<HealthEvent> ChillCured;

    public void CallChillCuredEvent()
    {
        ChillCured?.Invoke(this);
    }

    public event Action<HealthEvent> FrostCured;

    public void CallFrostCuredEvent()
    {
        FrostCured?.Invoke(this);
    }

    public event Action<HealthEvent> ShatterCured;

    public void CallShatterCuredEvent()
    {
        ShatterCured?.Invoke(this);
    }

    public event Action<HealthEvent> StaticCured;

    public void CallStaticCuredEvent()
    {
        StaticCured?.Invoke(this);
    }

    public event Action<HealthEvent> ParalyzeCured;

    public void CallParalyzeCuredEvent()
    {
        ParalyzeCured?.Invoke(this);
    }

    public event Action<HealthEvent> CurseCured;

    public void CallCurseCuredEvent()
    {
        CurseCured?.Invoke(this);
    }

    public event Action<HealthEvent> FearCured;

    public void CallFearCuredEvent()
    {
        FearCured?.Invoke(this);
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

    public event Action<HealthEvent> OnSevenArrowsActive;

    public void CallSevenArrowsSpecialMoveEvent()
    {
        OnSevenArrowsActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnSevenArrowsEffectsEnded;

    public void CallSevenArrowsWoreOffEvent()
    {
        OnSevenArrowsEffectsEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnKynarasEmbraceActive;

    public void CallKynarasEmbraceSpecialMoveEvent()
    {
        OnKynarasEmbraceActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnKynarasEmbraceEffectsEnded;

    public void CallKynarasEmbraceWoreOffEvent()
    {
        OnKynarasEmbraceEffectsEnded?.Invoke(this);
    }


    public event Action<HealthEvent> OnMycarasSealActive;

    public void CallMycarasSealSpecialMoveEvent()
    {
        OnMycarasSealActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnMycarasSealEffectsEnded;

    public void CallMycarasSealWoreOffEvent()
    {
        OnMycarasSealEffectsEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnRageActive;

    public void CallRageSpecialMoveEvent()
    {
        OnRageActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnRageEffectEnded;

    public void CallRageWoreOffEvent()
    {
        OnRageEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnShatterCryActive;

    public void CallShatterCrySpecialMoveEvent()
    {
        OnShatterCryActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnShatterCryEffectEnded;

    public void CallShatterCryWoreOffEvent()
    {
        OnShatterCryEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnWhirlrendActive;

    public void CallWhirlrendSpecialMoveEvent()
    {
        OnWhirlrendActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnWhirlrendEffectEnded;

    public void CallWhirlrendWoreOffEvent()
    {
        OnWhirlrendEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnFeastOfWarActive;

    public void CallFeastOfWarSpecialMoveEvent()
    {
        OnFeastOfWarActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnFeastOfWarEffectEnded;

    public void CallFeastOfWarWoreOffEvent()
    {
        OnFeastOfWarEffectEnded?.Invoke(this);
    }


    public event Action<HealthEvent> OnNyxasReflexActive;

    public void CallNyxasReflexSpecialMoveEvent()
    {
        OnNyxasReflexActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnNyxasReflexEffectEnded;

    public void CallNyxasReflexSpecialMoveEndEvent()
    {
        OnNyxasReflexEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnFadeAndFeedActive;

    public void CallFadeAndFeedSpecialMoveEvent()
    {
        OnFadeAndFeedActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnFadeAndFeedEffectEnded;

    public void CallFadeAndFeedSpecialMoveEndEvent()
    {
        OnFadeAndFeedEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnConductiveTouchActive;

    public void CallConductiveTouchSpecialMoveEvent()
    {
        OnConductiveTouchActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnConductiveTouchEffectEnded;

    public void CallConductiveTouchWoreOffEvent()
    {
        OnConductiveTouchEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnNymarasWindveilActive;

    public void CallNymarasWindveilSpecialMoveEvent()
    {
        OnNymarasWindveilActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnNymarasWindveilEffectEnded;

    public void CallNymarasWindveilWoreOffEvent()
    {
        OnNymarasWindveilEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnIonicRejuvenationActive;

    public void CallIonicRejuvenationSpecialMoveEvent()
    {
        OnIonicRejuvenationActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnIonicRejuvenationEffectEnded;

    public void CallIonicRejuvenationWoreOffEvent()
    {
        OnIonicRejuvenationEffectEnded?.Invoke(this);
    }

    // INNER PATH
    public event Action<HealthEvent> OnBattleScarsActive;

    public void CallBattleScarsEvent()
    {
        OnBattleScarsActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnBattleScarsEffectEnded;

    public void CallBattleScarsWoreOffEvent()
    {
        OnBattleScarsEffectEnded?.Invoke(this);
    }


    public event Action<HealthEvent> OnSecondBreathActive;

    public void CallSecondBreathEvent()
    {
        OnSecondBreathActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnSecondBreathEffectEnded;

    public void CallSecondBreathWoreOffEvent()
    {
        OnSecondBreathEffectEnded?.Invoke(this);
    }


    public event Action<HealthEvent> OnViciousMomentumActive;

    public void CallViciousMomentumEvent()
    {
        OnViciousMomentumActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnViciousMomentumEffectEnded;

    public void CallViciousMomentumWoreOffEvent()
    {
        OnViciousMomentumEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnCombatFocusActive;

    public void CallCombatFocusEvent()
    {
        OnCombatFocusActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnCombatFocusEffectEnded;

    public void CallCombatFocusWoreOffEvent()
    {
        OnCombatFocusEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnTriadExecutionActive;

    public void CallTriadExecutionEvent()
    {
        OnTriadExecutionActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnTriadExecutionEffectEnded;

    public void CallTriadExecutionWoreOffEvent()
    {
        OnTriadExecutionEffectEnded?.Invoke(this);
    }

    public event Action<HealthEvent> OnFortifiedResolveActive;

    public void CalllFortifiedResolveEvent()
    {
        OnFortifiedResolveActive?.Invoke(this);
    }

    public event Action<HealthEvent> OnFortifiedResolveEffectEnded;

    public void CallFortifiedResolveWoreOffEvent()
    {
        OnFortifiedResolveEffectEnded?.Invoke(this);
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
    public bool onStart;
}

