using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIconsUI : MonoBehaviour
{
    [Header("SPECIAL MOVES")]
    [Header("Caelion")]
    [SerializeField] Sprite graceOfTheUnscarredSpecialMoveSprite;
    [SerializeField] Sprite valorSpecialMoveSprite;
    [SerializeField] Sprite guardedOathSpecialMoveSprite;
    [SerializeField] Sprite breakTheLineSpecialMoveSprite;
    [Space(10)]

    [Header("Morven")]
    [SerializeField] Sprite umbralMistSpecialMoveSprite;
    [SerializeField] Sprite stealthSpecialMoveSprite;
    [SerializeField] Sprite shadowStepSpecialMoveSprite;
    [Space(10)]

    [Header("Nyveran")]
    [SerializeField] Sprite arrowsOfTheSevenPlaguesSpecialMoveSprite;
    [Space(10)]

    [Header("Mycara")]
    [SerializeField] Sprite mycarasSealSpecialMoveSprite;
    [Space(10)]

    [Header("Kynara")]
    [SerializeField] Sprite kynarasEmbraceSpecialMoveSprite;
    [Space(10)]

    [Header("Karnag")]
    [SerializeField] Sprite rageSpecialMoveSprite;
    [SerializeField] Sprite whirlrendSpecialMoveSprite;
    [SerializeField] Sprite feastOfWarSpecialMoveSprite;
    [Space(10)]

    [Header("INNER PATH")]
    [SerializeField] Sprite battleScarsSprite;
    [SerializeField] Sprite secondBreathSprite;
    [SerializeField] Sprite viciousMomentumSprite;
    [SerializeField] Sprite combatFocusSprite;
    [SerializeField] Sprite triadExecutionSprite;
    [SerializeField] Sprite reflexBarrierSprite;
    [Space(10)]

    [Header("Debuff")]
    [SerializeField] Sprite bleedingSprite;
    [SerializeField] Sprite stunSprite;
    [SerializeField] Sprite slowSprite;
    [SerializeField] Sprite poisonSprite;
    [SerializeField] Sprite rootedSprite;
    [SerializeField] Sprite acidSprite;
    [SerializeField] Sprite warmedSprite;
    [SerializeField] Sprite burnSprite;
    [SerializeField] Sprite chillSprite;
    [SerializeField] Sprite frostSprite;
    [SerializeField] Sprite staticSprite;
    [SerializeField] Sprite paralyzeSprite;
    [SerializeField] Sprite markSprite;
    [SerializeField] Sprite petrifiedSprite;
    [SerializeField] Sprite blindSprite;
    [SerializeField] Sprite revealedSprite;
    [SerializeField] Sprite curseSprite;
    [SerializeField] Sprite fearSprite;
    [SerializeField] Sprite deathSprite;

    [Space(10)]
    [Header("Positive status effects")]
    [SerializeField] Sprite hasteSprite;
    [SerializeField] Sprite thornedSprite;

    Player player;

    Dictionary<Sprite, GameObject> statusEffectsDictionary = new Dictionary<Sprite, GameObject>();

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        // SKILLS - ENABLE
        // Caelion
        player.healthEvent.OnGraceOfTheUnscarredActive += EnableGraceOfTheUnscarredSkillImage;
        player.healthEvent.OnValorActive += EnableValorSkillImage;
        player.healthEvent.OnBreakTheLineActive += EnableBreakTheLineSkillImage;
        player.healthEvent.OnGuardedOathActive += EnableGuardedOathSkillImage;

        // Morven
        player.healthEvent.OnUmbralMistActive += EnableUmbralMistSkillImage;
        player.healthEvent.OnStealthActive += EnableStealthSkillImage;
        player.healthEvent.OnShadowStepActive += EnableShadowStepSkillImage;

        // Nyveran
        player.healthEvent.OnSevenArrowsActive += EnableArrowsOfTheSevenPlaguesImage;

        // Mycara
        player.healthEvent.OnMycarasSealActive += EnableMycarasSealImage;

        // Kynara
        player.healthEvent.OnKynarasEmbraceActive += EnableKynarasEmbraceImage;

        // Karnag
        player.healthEvent.OnRageActive += EnableRageImage;
        player.healthEvent.OnWhirlrendActive += EnableWhirlrendImage;
        player.healthEvent.OnFeastOfWarActive += EnableFeastOfWarImage;

        // INNER PATH
        player.healthEvent.OnBattleScarsActive += EnableBattleScarsImage;
        player.healthEvent.OnSecondBreathActive += EnableSecondBreathImage;
        player.healthEvent.OnViciousMomentumActive += EnableViciousMomentumImage;
        player.healthEvent.OnCombatFocusActive += EnableCombatFocusImage;
        player.healthEvent.OnTriadExecutionActive += EnableTriadExecutionImage;
        player.healthEvent.OnFortifiedResolveActive += EnableReflexBarrierImage;

        player.healthEvent.GetBleeding += EnableBleedingImage;
        player.healthEvent.GetStun += EnableStunImage;
        player.healthEvent.GetSlow += EnableSlowImage;
        player.healthEvent.GetWarm += EnableWarmedImage;
        player.healthEvent.GetBurned += EnableBurnImage;
        player.healthEvent.GetPoisoned += EnablePoisonImage;
        player.healthEvent.GetAcid += EnableAcidImage;
        player.healthEvent.GetChill += EnableChillImage;
        player.healthEvent.GetFrost += EnableFrostImage;
        player.healthEvent.GetRoot += EnableRootImage;
        player.healthEvent.GetBlind += EnableBlindImage;
        player.healthEvent.GetCursed += EnableCurseImage;
        player.healthEvent.GetFeared += EnableFearImage;

        player.healthEvent.GetDeath += EnableDeathImage;
        player.healthEvent.OnDodged += HealthEvent_OnDodged;
        player.healthEvent.OnBlocked += HealthEvent_OnBlocked;
        player.healthEvent.OnParried += HealthEvent_OnParried;

        // SKILLS - DISABLE
        // Caelion
        player.healthEvent.OnGraceOfTheUnscarredEnded += DisableGraceOfTheUnscarredSkillImage;
        player.healthEvent.OnValorEffectEnded += DisableValorSkillImage;
        player.healthEvent.OnBreakTheLineEffectEnded += DisableBreakTheLineSkillImage;
        player.healthEvent.OnGuardedOathEffectEnded += DisableGuardedOathSkillImage;

        // Morven
        player.healthEvent.OnUmbralMistEffectEnded += DisableUmbralMistSkillImage;
        player.healthEvent.OnStealthEffectEnded += DisableStealthSkillImage;
        player.healthEvent.OnShadowStepEffectEnded += DisableShadowStepSkillImage;

        // Nyveran
        player.healthEvent.OnSevenArrowsEffectsEnded += DisableArrowsOfTheSevenPlaguesImage;

        // Mycara
        player.healthEvent.OnMycarasSealEffectsEnded += DisableMycarasSealImage;

        // Kynara
        player.healthEvent.OnKynarasEmbraceEffectsEnded += DisableKynarasEmbraceImage;

        // Karnag
        player.healthEvent.OnRageEffectEnded += DisableRageImage;
        player.healthEvent.OnWhirlrendEffectEnded += DisableWhirlrendImage;
        player.healthEvent.OnFeastOfWarEffectEnded += DisableFeastOfWarImage;

        // INNER PATH
        player.healthEvent.OnSecondBreathEffectEnded += DisableSecondBreathImage;
        player.healthEvent.OnBattleScarsEffectEnded += DisableBattleScarsImage;
        player.healthEvent.OnViciousMomentumEffectEnded += DisableViciousMomentImage;
        player.healthEvent.OnCombatFocusEffectEnded += DisableCombatFocusImage;
        player.healthEvent.OnTriadExecutionEffectEnded += DisableTriadExecutionImage;
        player.healthEvent.OnFortifiedResolveEffectEnded += DisableReflexBarrierImage;

        player.healthEvent.BleedingCured += DisableBleedingImage;
        player.healthEvent.StunCured += DisableStunImage;
        player.healthEvent.SlowCured += DisableSlowImage;
        player.healthEvent.WarmCured += DisableWarmedImage;
        player.healthEvent.BurnCured += DisableBurnImage;
        player.healthEvent.PoisonCured += DisablePoisonImage;
        player.healthEvent.AcidCured += DisableAcidImage;
        player.healthEvent.ChillCured += DisableChillImage;
        player.healthEvent.FrostCured += DisableFrostImage;
        player.healthEvent.RootCured += DisableRootImage;
        player.healthEvent.BlindCured += DisableBlindImage;
        player.healthEvent.CurseCured += DisableCurseImage;
        player.healthEvent.FearCured += DisabeFearImage;

    }

    private void OnDisable()
    {
        // SKILLS - ENABLE
        // Caelion
        player.healthEvent.OnGraceOfTheUnscarredActive -= EnableGraceOfTheUnscarredSkillImage;
        player.healthEvent.OnValorActive -= EnableValorSkillImage;
        player.healthEvent.OnBreakTheLineActive -= EnableBreakTheLineSkillImage;
        player.healthEvent.OnGuardedOathActive -= EnableGuardedOathSkillImage;

        // Morven
        player.healthEvent.OnUmbralMistActive -= EnableUmbralMistSkillImage;
        player.healthEvent.OnStealthActive -= EnableStealthSkillImage;
        player.healthEvent.OnShadowStepActive -= EnableShadowStepSkillImage;

        // Nyveran
        player.healthEvent.OnSevenArrowsActive -= EnableArrowsOfTheSevenPlaguesImage;

        // Mycara
        player.healthEvent.OnMycarasSealActive -= EnableMycarasSealImage;

        // Kynara
        player.healthEvent.OnKynarasEmbraceActive -= EnableKynarasEmbraceImage;

        // Karnag
        player.healthEvent.OnRageActive -= EnableRageImage;
        player.healthEvent.OnWhirlrendActive -= EnableWhirlrendImage;
        player.healthEvent.OnFeastOfWarActive -= EnableFeastOfWarImage;

        // INNER PATH
        player.healthEvent.OnBattleScarsActive -= EnableBattleScarsImage;
        player.healthEvent.OnSecondBreathActive -= EnableSecondBreathImage;
        player.healthEvent.OnViciousMomentumActive -= EnableViciousMomentumImage;
        player.healthEvent.OnCombatFocusActive -= EnableCombatFocusImage;
        player.healthEvent.OnTriadExecutionActive -= EnableTriadExecutionImage;
        player.healthEvent.OnFortifiedResolveActive -= EnableReflexBarrierImage;

        player.healthEvent.GetBleeding -= EnableBleedingImage;
        player.healthEvent.GetStun -= EnableStunImage;
        player.healthEvent.GetSlow -= EnableSlowImage;
        player.healthEvent.GetWarm -= EnableWarmedImage;
        player.healthEvent.GetBurned -= EnableBurnImage;
        player.healthEvent.GetPoisoned -= EnablePoisonImage;
        player.healthEvent.GetAcid -= EnableAcidImage;
        player.healthEvent.GetChill -= EnableChillImage;
        player.healthEvent.GetFrost -= EnableFrostImage;
        player.healthEvent.GetRoot -= EnableRootImage;
        player.healthEvent.GetBlind -= EnableBlindImage;
        player.healthEvent.GetCursed -= EnableCurseImage;
        player.healthEvent.GetFeared -= EnableFearImage;

        player.healthEvent.GetDeath -= EnableDeathImage;
        player.healthEvent.OnDodged -= HealthEvent_OnDodged;
        player.healthEvent.OnBlocked -= HealthEvent_OnBlocked;
        player.healthEvent.OnParried -= HealthEvent_OnParried;

        // SKILLS - DISABLE
        // Caelion
        player.healthEvent.OnGraceOfTheUnscarredEnded += DisableGraceOfTheUnscarredSkillImage;
        player.healthEvent.OnValorEffectEnded -= DisableValorSkillImage;
        player.healthEvent.OnBreakTheLineEffectEnded -= DisableBreakTheLineSkillImage;
        player.healthEvent.OnGuardedOathEffectEnded -= DisableGuardedOathSkillImage;

        //Morven
        player.healthEvent.OnUmbralMistEffectEnded -= DisableUmbralMistSkillImage;
        player.healthEvent.OnStealthEffectEnded -= DisableStealthSkillImage;
        player.healthEvent.OnShadowStepEffectEnded -= DisableShadowStepSkillImage;

        // Nyveran
        player.healthEvent.OnSevenArrowsEffectsEnded -= DisableArrowsOfTheSevenPlaguesImage;

        // Mycara
        player.healthEvent.OnMycarasSealEffectsEnded -= DisableMycarasSealImage;

        // Kynara
        player.healthEvent.OnKynarasEmbraceEffectsEnded -= DisableKynarasEmbraceImage;

        // Karnag
        player.healthEvent.OnRageEffectEnded -= DisableRageImage;
        player.healthEvent.OnWhirlrendEffectEnded -= DisableWhirlrendImage;
        player.healthEvent.OnFeastOfWarEffectEnded -= DisableFeastOfWarImage;

        // INNER PATH
        player.healthEvent.OnSecondBreathEffectEnded -= DisableSecondBreathImage;
        player.healthEvent.OnBattleScarsEffectEnded -= DisableBattleScarsImage;
        player.healthEvent.OnViciousMomentumEffectEnded -= DisableViciousMomentImage;
        player.healthEvent.OnCombatFocusEffectEnded -= DisableCombatFocusImage;
        player.healthEvent.OnTriadExecutionEffectEnded -= DisableTriadExecutionImage;
        player.healthEvent.OnFortifiedResolveEffectEnded -= DisableReflexBarrierImage;

        player.healthEvent.BleedingCured -= DisableBleedingImage;
        player.healthEvent.StunCured -= DisableStunImage;
        player.healthEvent.SlowCured -= DisableSlowImage;
        player.healthEvent.WarmCured -= DisableWarmedImage;
        player.healthEvent.BurnCured -= DisableBurnImage;
        player.healthEvent.PoisonCured -= DisablePoisonImage;
        player.healthEvent.AcidCured -= DisableAcidImage;
        player.healthEvent.ChillCured -= DisableChillImage;
        player.healthEvent.FrostCured -= DisableFrostImage;
        player.healthEvent.RootCured -= DisableRootImage;
        player.healthEvent.BlindCured -= DisableBlindImage;
        player.healthEvent.CurseCured -= DisableCurseImage;
        player.healthEvent.FearCured -= DisabeFearImage;
    }

    private void HealthEvent_OnParried(HealthEvent healthEvent)
    {

    }

    private void HealthEvent_OnDodged(HealthEvent healthEvent)
    {

    }

    private void HealthEvent_OnBlocked(HealthEvent healthEvent)
    {

    }

    private void EnableGraceOfTheUnscarredSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(graceOfTheUnscarredSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = graceOfTheUnscarredSpecialMoveSprite;
            statusEffectsDictionary.Add(graceOfTheUnscarredSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableValorSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(valorSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = valorSpecialMoveSprite;
            statusEffectsDictionary.Add(valorSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableBreakTheLineSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(breakTheLineSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = breakTheLineSpecialMoveSprite;
            statusEffectsDictionary.Add(breakTheLineSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableGuardedOathSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(guardedOathSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = guardedOathSpecialMoveSprite;
            statusEffectsDictionary.Add(guardedOathSpecialMoveSprite, statusIconContainer);
        }
    }


    private void EnableUmbralMistSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(umbralMistSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = umbralMistSpecialMoveSprite;
            statusEffectsDictionary.Add(umbralMistSpecialMoveSprite, statusIconContainer);
        }
    }


    private void EnableStealthSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(stealthSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = stealthSpecialMoveSprite;
            statusEffectsDictionary.Add(stealthSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableShadowStepSkillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(shadowStepSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = shadowStepSpecialMoveSprite;
            statusEffectsDictionary.Add(shadowStepSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableArrowsOfTheSevenPlaguesImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(arrowsOfTheSevenPlaguesSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = arrowsOfTheSevenPlaguesSpecialMoveSprite;
            statusEffectsDictionary.Add(arrowsOfTheSevenPlaguesSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableMycarasSealImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(mycarasSealSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = mycarasSealSpecialMoveSprite;
            statusEffectsDictionary.Add(mycarasSealSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableKynarasEmbraceImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(kynarasEmbraceSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = kynarasEmbraceSpecialMoveSprite;
            statusEffectsDictionary.Add(kynarasEmbraceSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableRageImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(rageSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = rageSpecialMoveSprite;
            statusEffectsDictionary.Add(rageSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableWhirlrendImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(whirlrendSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = whirlrendSpecialMoveSprite;
            statusEffectsDictionary.Add(whirlrendSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableFeastOfWarImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(feastOfWarSpecialMoveSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = feastOfWarSpecialMoveSprite;
            statusEffectsDictionary.Add(feastOfWarSpecialMoveSprite, statusIconContainer);
        }
    }

    private void EnableCombatFocusImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(combatFocusSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = combatFocusSprite;
            statusEffectsDictionary.Add(combatFocusSprite, statusIconContainer);
        }
    }

    private void EnableViciousMomentumImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(viciousMomentumSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = viciousMomentumSprite;
            statusEffectsDictionary.Add(viciousMomentumSprite, statusIconContainer);
        }
    }

    private void EnableSecondBreathImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(secondBreathSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = secondBreathSprite;
            statusEffectsDictionary.Add(secondBreathSprite, statusIconContainer);
        }
    }

    private void EnableBattleScarsImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(battleScarsSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = battleScarsSprite;
            statusEffectsDictionary.Add(battleScarsSprite, statusIconContainer);
        }
    }

    private void EnableTriadExecutionImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(triadExecutionSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = triadExecutionSprite;
            statusEffectsDictionary.Add(triadExecutionSprite, statusIconContainer);
        }
    }

    private void EnableReflexBarrierImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(reflexBarrierSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = reflexBarrierSprite;
            statusEffectsDictionary.Add(reflexBarrierSprite, statusIconContainer);
        }
    }


    private void EnableWarmedImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(warmedSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = warmedSprite;
            statusEffectsDictionary.Add(warmedSprite, statusIconContainer);
        }
    }


    private void EnableBurnImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(burnSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = burnSprite;
            statusEffectsDictionary.Add(burnSprite, statusIconContainer);
        }
    }

    private void EnablePoisonImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(poisonSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = poisonSprite;
            statusEffectsDictionary.Add(poisonSprite, statusIconContainer);
        }
    }

    private void EnableAcidImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(acidSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = acidSprite;
            statusEffectsDictionary.Add(acidSprite, statusIconContainer);
        }
    }

    private void EnableChillImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(chillSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = chillSprite;
            statusEffectsDictionary.Add(chillSprite, statusIconContainer);
        }
    }

    private void EnableFrostImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(frostSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = frostSprite;
            statusEffectsDictionary.Add(frostSprite, statusIconContainer);
        }
    }

    private void EnableBleedingImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(bleedingSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = bleedingSprite;
            statusEffectsDictionary.Add(bleedingSprite, statusIconContainer);
        }
    }

    private void EnableStunImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(stunSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = stunSprite;
            statusEffectsDictionary.Add(stunSprite, statusIconContainer);
        }
    }

    private void EnableSlowImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(slowSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = slowSprite;
            statusEffectsDictionary.Add(slowSprite, statusIconContainer);
        }
    }

    private void EnableRootImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(rootedSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = rootedSprite;
            statusEffectsDictionary.Add(rootedSprite, statusIconContainer);
        }
    }

    private void EnableBlindImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(blindSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = blindSprite;
            statusEffectsDictionary.Add(blindSprite, statusIconContainer);
        }
    }

    private void EnableCurseImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(curseSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = curseSprite;
            statusEffectsDictionary.Add(curseSprite, statusIconContainer);
        }
    }

    private void EnableFearImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(fearSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = fearSprite;
            statusEffectsDictionary.Add(fearSprite, statusIconContainer);
        }
    }

    private void EnableDeathImage(HealthEvent healthEvent)
    {
        if (!statusEffectsDictionary.ContainsKey(deathSprite))
        {
            GameObject statusIconContainer = Instantiate(GameResources.Instance.statusEffectPrefab, transform);
            statusIconContainer.GetComponent<Image>().sprite = deathSprite;
            statusEffectsDictionary.Add(deathSprite, statusIconContainer);
        }
    }


    private void DisableGraceOfTheUnscarredSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(graceOfTheUnscarredSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[graceOfTheUnscarredSpecialMoveSprite]);
            statusEffectsDictionary.Remove(graceOfTheUnscarredSpecialMoveSprite);
        }
    }

    private void DisableValorSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(valorSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[valorSpecialMoveSprite]);
            statusEffectsDictionary.Remove(valorSpecialMoveSprite);
        }
    }

    private void DisableBreakTheLineSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(breakTheLineSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[breakTheLineSpecialMoveSprite]);
            statusEffectsDictionary.Remove(breakTheLineSpecialMoveSprite);
        }
    }

    private void DisableGuardedOathSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(guardedOathSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[guardedOathSpecialMoveSprite]);
            statusEffectsDictionary.Remove(guardedOathSpecialMoveSprite);
        }
    }

    private void DisableUmbralMistSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(umbralMistSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[umbralMistSpecialMoveSprite]);
            statusEffectsDictionary.Remove(umbralMistSpecialMoveSprite);
        }
    }

    private void DisableStealthSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(stealthSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[stealthSpecialMoveSprite]);
            statusEffectsDictionary.Remove(stealthSpecialMoveSprite);
        }
    }

    private void DisableShadowStepSkillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(shadowStepSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[shadowStepSpecialMoveSprite]);
            statusEffectsDictionary.Remove(shadowStepSpecialMoveSprite);
        }
    }

    private void DisableArrowsOfTheSevenPlaguesImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(arrowsOfTheSevenPlaguesSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[arrowsOfTheSevenPlaguesSpecialMoveSprite]);
            statusEffectsDictionary.Remove(arrowsOfTheSevenPlaguesSpecialMoveSprite);
        }
    }

    private void DisableMycarasSealImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(mycarasSealSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[mycarasSealSpecialMoveSprite]);
            statusEffectsDictionary.Remove(mycarasSealSpecialMoveSprite);
        }
    }

    private void DisableKynarasEmbraceImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(kynarasEmbraceSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[kynarasEmbraceSpecialMoveSprite]);
            statusEffectsDictionary.Remove(kynarasEmbraceSpecialMoveSprite);
        }
    }

    private void DisableRageImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(rageSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[rageSpecialMoveSprite]);
            statusEffectsDictionary.Remove(rageSpecialMoveSprite);
        }
    }

    private void DisableWhirlrendImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(whirlrendSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[whirlrendSpecialMoveSprite]);
            statusEffectsDictionary.Remove(whirlrendSpecialMoveSprite);
        }
    }

    private void DisableFeastOfWarImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(feastOfWarSpecialMoveSprite))
        {
            Destroy(statusEffectsDictionary[feastOfWarSpecialMoveSprite]);
            statusEffectsDictionary.Remove(feastOfWarSpecialMoveSprite);
        }
    }

    // INNER PATH
    private void DisableCombatFocusImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(combatFocusSprite))
        {
            Destroy(statusEffectsDictionary[combatFocusSprite]);
            statusEffectsDictionary.Remove(combatFocusSprite);
        }
    }

    private void DisableViciousMomentImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(viciousMomentumSprite))
        {
            Destroy(statusEffectsDictionary[viciousMomentumSprite]);
            statusEffectsDictionary.Remove(viciousMomentumSprite);
        }
    }

    private void DisableBattleScarsImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(battleScarsSprite))
        {
            Destroy(statusEffectsDictionary[battleScarsSprite]);
            statusEffectsDictionary.Remove(battleScarsSprite);
        }
    }

    private void DisableSecondBreathImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(secondBreathSprite))
        {
            Destroy(statusEffectsDictionary[secondBreathSprite]);
            statusEffectsDictionary.Remove(secondBreathSprite);
        }
    }

    private void DisableTriadExecutionImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(triadExecutionSprite))
        {
            Destroy(statusEffectsDictionary[triadExecutionSprite]);
            statusEffectsDictionary.Remove(triadExecutionSprite);
        }
    }

    private void DisableReflexBarrierImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(reflexBarrierSprite))
        {
            Destroy(statusEffectsDictionary[reflexBarrierSprite]);
            statusEffectsDictionary.Remove(reflexBarrierSprite);
        }
    }

    private void DisableWarmedImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(warmedSprite))
        {
            Destroy(statusEffectsDictionary[warmedSprite]);
            statusEffectsDictionary.Remove(warmedSprite);
        }
    }

    private void DisableBurnImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(burnSprite))
        {
            Destroy(statusEffectsDictionary[burnSprite]);
            statusEffectsDictionary.Remove(burnSprite);
        }
    }

    private void DisablePoisonImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(poisonSprite))
        {
            Destroy(statusEffectsDictionary[poisonSprite]);
            statusEffectsDictionary.Remove(poisonSprite);
        }
    }

    private void DisableAcidImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(acidSprite))
        {
            Destroy(statusEffectsDictionary[acidSprite]);
            statusEffectsDictionary.Remove(acidSprite);
        }
    }

    private void DisableChillImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(chillSprite))
        {
            Destroy(statusEffectsDictionary[chillSprite]);
            statusEffectsDictionary.Remove(chillSprite);
        }
    }

    private void DisableFrostImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(frostSprite))
        {
            Destroy(statusEffectsDictionary[frostSprite]);
            statusEffectsDictionary.Remove(frostSprite);
        }
    }

    private void DisableBleedingImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(bleedingSprite))
        {
            Destroy(statusEffectsDictionary[bleedingSprite]);
            statusEffectsDictionary.Remove(bleedingSprite);
        }
    }

    private void DisableStunImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(stunSprite))
        {
            Destroy(statusEffectsDictionary[stunSprite]);
            statusEffectsDictionary.Remove(stunSprite);
        }
    }

    private void DisableSlowImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(slowSprite))
        {
            Destroy(statusEffectsDictionary[slowSprite]);
            statusEffectsDictionary.Remove(slowSprite);
        }
    }

    private void DisableRootImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(rootedSprite))
        {
            Destroy(statusEffectsDictionary[rootedSprite]);
            statusEffectsDictionary.Remove(rootedSprite);
        }
    }

    private void DisableBlindImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(blindSprite))
        {
            Destroy(statusEffectsDictionary[blindSprite]);
            statusEffectsDictionary.Remove(blindSprite);
        }
    }

    private void DisableCurseImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(curseSprite))
        {
            Destroy(statusEffectsDictionary[curseSprite]);
            statusEffectsDictionary.Remove(curseSprite);
        }
    }

    private void DisabeFearImage(HealthEvent healthEvent)
    {
        if (statusEffectsDictionary.ContainsKey(fearSprite))
        {
            Destroy(statusEffectsDictionary[fearSprite]);
            statusEffectsDictionary.Remove(fearSprite);
        }
    }


}
