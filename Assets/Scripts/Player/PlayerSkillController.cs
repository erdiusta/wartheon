using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerSkillController : MonoBehaviour
{
    Player player;

    public Animator activeSkillTypeOneAnimator;
    public Animator activeSkillTypeTwoAnimator;
    public Animator activeSkillTypeThreeAnimator;
    public Animator activeSkillTypeFourAnimator;

    [HideInInspector] public Coroutine unstealthRoutine;
    Coroutine destroySlamObjectRoutine;
    Coroutine teleportParticleRoutine;

    float unstealthImmunityTime = 2f;
    bool particlePlayed;

    AnimationEventHelperMainHand animationEventHelperMainHand;

    private void Awake()
    {
        player = GetComponent<Player>();
        animationEventHelperMainHand = GetComponent<AnimationEventHelperMainHand>();
    }

    private void OnEnable()
    {
        animationEventHelperMainHand.OnSeismicSlamTriggered.AddListener(PerformSeismicSlam);
        animationEventHelperMainHand.OnShatterCryTriggered.AddListener(PerformShatterCry);
    }

    private void OnDisable()
    {
        animationEventHelperMainHand.OnSeismicSlamTriggered.RemoveListener(PerformSeismicSlam);
        animationEventHelperMainHand.OnShatterCryTriggered.RemoveListener(PerformShatterCry);
    }

    /// <summary>
    /// Execute Seismic Slam special move
    /// </summary>
    public void SeismicSlamProcess()
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.firstActiveSkillDetails.activeUniqueSkillSoundEffectTwo);
        player.animator.SetTrigger("seismicSlam");

        // Make Caelion unpushable
        SetUnpushable(true);

    }

    private void PerformSeismicSlam()
    {
        if (destroySlamObjectRoutine == null)
        {
            destroySlamObjectRoutine = StartCoroutine(DestroySlamEffectObject(1f));
        }
    }

    IEnumerator DestroySlamEffectObject(float duration)
    {
        GameObject slamEffectObject = Instantiate(activeSkillTypeThreeAnimator.gameObject, transform.position, Quaternion.identity);
        slamEffectObject.GetComponent<Animator>().SetTrigger("slam");

        yield return new WaitForSeconds(duration);

        // Restore pushable state
        SetUnpushable(false);

        destroySlamObjectRoutine = null;
        Destroy(slamEffectObject); // Destroy slam object after animation completed
    }

    private void SetUnpushable(bool state)
    {
        player.isSeismicSlamActive = state;

        if (state)
        {
            player.rb2D.bodyType = RigidbodyType2D.Kinematic; // ignores external forces
            player.rb2D.linearVelocity = Vector2.zero; // prevent sliding
        }
        else
        {
            player.rb2D.bodyType = RigidbodyType2D.Dynamic; // back to normal
        }
    }

    /// <summary>
    /// Execute Valor special move
    /// </summary>
    public void Valor(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isValorActive)
            {
                activeSkillTypeOneAnimator.SetBool("valor", true);
                player.healthEvent.CallValorSpecialMoveEvent(); // This is for displaying valor icon
                player.isValorActive = true;
                StartCoroutine(ValorRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator ValorRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isValorActive = false;
        player.healthEvent.CallValorWoreOffEvent();
        activeSkillTypeOneAnimator.SetBool("valor", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute BreakTheLine special move
    /// </summary>
    public void BreakTheLine(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            int originalMinDamage = player.currentMainHandMinDamageValue;
            int originalMaxDamage = player.currentMainHandMaxDamageValue;

            // ENABLE SKILL EFFECTS
            switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
            {
                case 1:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.4f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.4f);
                    break;
                case 2:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.5f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.5f);
                    break;
                case 3:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.6f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.6f);
                    break;
                default:
                    break;
            }

            player.additionalSpeedModifier += 2; // Increase speed

            Weapon droppedShield = player.activeWeapon.GetCurrentOffHandWeapon();
            player.UpdateDamageValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();

            player.healthEvent.CallBreakTheLineSpecialMoveEvent(); // This is for displaying valor icon

            if (!player.isBreakTheLineActive)
            {
                player.isBreakTheLineActive = true;
                StartCoroutine(BreakTheLineRoutine(slotIndex, droppedShield, originalMinDamage, originalMaxDamage, activeSkillData));
            }
        }
    }

    IEnumerator BreakTheLineRoutine(int slotIndex, Weapon droppedShield, int originalMinDamage, int originalMaxDamage, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        // DISABLE SKILL EFFECTS
        player.additionalSpeedModifier -= 2; // Reset speed
        player.currentMainHandMinDamageValue = originalMinDamage;
        player.currentMainHandMinDamageValue = originalMaxDamage;
        player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(droppedShield, player.currentWeaponSlotSetIndex, false, false);
        player.UpdateDamageValues();
        player.UpdateSpeedValue();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.isBreakTheLineActive = false;
        player.healthEvent.CallBreakTheLineWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }


    /// <summary>
    /// Execute GuardedOath special move
    /// </summary>
    public void GuardedOath(int slotIndex)
    {
        player.isGuardedOathActive = true;
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        activeSkillTypeTwoAnimator.SetBool("oath", true);

        player.originalShieldArmorModifier = player.additionalShieldArmorModifier;
        player.originalBlockModifier = player.additionalBlockModifier;

        // EFFECTS
        switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
        {
            case 1:
                player.additionalShieldArmorModifier += 1f;
                player.additionalBlockModifier += 0.2f;
                break;
            case 2:
                player.additionalShieldArmorModifier += 1.2f;
                player.additionalBlockModifier += 0.25f;
                break;
            case 3:
                player.additionalShieldArmorModifier += 1.5f;
                player.additionalBlockModifier += 0.3f;
                break;
            default:
                break;
        }

        player.additionalPhysicalDamageModifer -= 0.1f;
        player.additionalSpeedModifier -= 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        // ENABLE SKILL EFFECTS
        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI
        player.healthEvent.CallGuardedOathSpecialMoveEvent(); // This is for displaying guarded oath icon
    }

    /// <summary>
    /// Remove GuardedOath effects
    /// </summary>
    public void RemoveGuardedOathEffects(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        player.isGuardedOathActive = false;

        activeSkillTypeTwoAnimator.SetBool("oath", false);

        // EFFECTS WORE OFF
        player.additionalShieldArmorModifier = player.originalShieldArmorModifier;
        player.additionalBlockModifier = player.originalBlockModifier;

        player.additionalPhysicalDamageModifer += 0.1f;
        player.additionalSpeedModifier += 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

        player.mana.ResetReservedMana(activeSkillData.manaReserveCost);
        player.healthEvent.CallGuardedOathSpecialMoveEndEvent(); // This is for displaying guarded oath icon
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Umbral Mist special move
    /// </summary>
    public void UmbralMist(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.healthEvent.CallUmbralMistSpecialMoveEvent(); // This is for displaying umbral mist icon

            if (!player.isUmbralMistActive)
            {
                player.isUmbralMistActive = true;
                StartCoroutine(UmbralMistRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator UmbralMistRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        GameObject umbralMistObject = Instantiate(activeSkillTypeTwoAnimator.gameObject, transform.position, Quaternion.identity);

        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isUmbralMistActive = false;

        player.healthEvent.CallUmbralMistWoreOffEvent();
        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(umbralMistObject, 2f); // Destroy mist object after animation completed
    }

    /// <summary>
    /// Execute Stealth special move
    /// </summary>
    public void Stealth(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            // EFFECTS
            player.health.isDamageable = false;

            player.healthEvent.CallStealthSpecialMoveEvent(); // This is for displaying stealth icon

            // Set player's stealth status to true
            if (!player.isStealthActive)
            {
                player.isStealthActive = true;
            }
        }

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Set the alpha value to 0.3 (30% opacity)
        currentColor.a = 0.3f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Start the coroutine to maintain the alpha value during stealth
        StartCoroutine(StealthRoutine(slotIndex, activeSkillData));
    }

    IEnumerator StealthRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        float stealthDuration = activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier);

        yield return new WaitForSeconds(stealthDuration);

        Unstealth();

    }

    /// <summary>
    /// Unstealth from special move
    /// </summary>
    public void Unstealth()
    {
        if (unstealthRoutine != null) return;

        int slotIndex = -1;

        foreach (KeyValuePair<int, ActiveUniqueSkillDetailsSO> skill in player.currentlyUsedActiveUniqueSkills)
        {
            if (skill.Value.activeSkill == ActiveSkill.Stealth)
            {
                slotIndex = skill.Key;
            }
        }

        // Trigger cooldown and ui components
        player.healthEvent.CallStealthWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        unstealthRoutine = StartCoroutine(UnstealthRoutine());
    }

    IEnumerator UnstealthRoutine()
    {
        // Set immunity
        player.health.isDamageable = false;

        // Set player's stealth status to false
        player.isStealthActive = false;

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;

        // Set the alpha value back to 0.7 (70% opacity)
        currentColor.a = 0.7f;
        player.spriteRenderer.color = currentColor;

        yield return new WaitForSeconds(unstealthImmunityTime);

        // Set the alpha value back to 1 (100% opacity)
        currentColor.a = 1f;
        player.spriteRenderer.color = currentColor;

        player.health.isDamageable = true;
        unstealthRoutine = null;
    }

    /// <summary>
    /// Execute Blood Drain speical move
    /// </summary>
    public void BloodDrain()
    {
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Swing, MeleeHand.MainHand, isBloodDrain: true);
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentOffHandWeapon(), AttackShape.Swing, MeleeHand.OffHand, isBloodDrain: true);
    }

    /// <summary>
    /// Execute Shadowstep special move
    /// </summary>
    public void ShadowStep(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isShadowStepActive)
            {
                player.isShadowStepActive = true;
                activeSkillTypeThreeAnimator.SetBool("shadowStep", true);
                player.healthEvent.CallShadowStepSpecialMoveEvent(); // This is for displaying shadow step icon
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

                float modifier = 0f;

                //EFFECTS
                switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
                {
                    case 1: modifier = 2; break;
                    case 2: modifier = 3; break;
                    case 3: modifier = 4; break;
                    default: break;
                }

                player.additionalSpeedModifier += modifier;
                player.UpdateSpeedValue();
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
                StartCoroutine(ShadowStepRoutine(slotIndex, activeSkillData, modifier));
            }
        }
    }

    IEnumerator ShadowStepRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, float modifier)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isShadowStepActive = false;
        activeSkillTypeThreeAnimator.SetBool("shadowStep", false);

        //EFFECTS ENDED
        player.additionalSpeedModifier -= modifier;
        player.UpdateSpeedValue();

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
        player.healthEvent.CallShadowStepWoreOffEvent();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    /// <summary>
    /// Execute Cull the Meek special move
    /// </summary>
    public void CullTheMeek()
    {
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Thrust, MeleeHand.MainHand, false, false, true);
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentOffHandWeapon(), AttackShape.Thrust, MeleeHand.OffHand, false, false, true);
    }

    /// <summary>
    /// Execute Penetrate special move
    /// </summary>
    public void Penetrate(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isPenetrateActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;
        activeSkillTypeTwoAnimator.SetTrigger("penetrate");

        // Trigger fire weapon event
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.Default, new AttackContext { isPenetrationArrow = true }, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isPenetrateActive = false));
    }

    /// <summary>
    /// Execute Triple Threat special move
    /// </summary>
    public void TripleThreat(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isTripleThreatActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        activeSkillTypeThreeAnimator.SetTrigger("tripleThreat");

        // Trigger fire weapon event
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.Default, new AttackContext { isTripleThreat = true }, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isTripleThreatActive = false));
    }

    /// <summary>
    /// Execute Binding Arrow special move
    /// </summary>
    public void BindingArrow(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isBindingArrowActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.Default, new AttackContext { isBindingArrow = true }, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isBindingArrowActive = false));
    }

    /// <summary>
    /// Execute Arrow of the Seven Plagues special move
    /// </summary>
    public void ArrowOfTheSevenPlagues(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        player.isArrowOfTheSevenActive = true;
        player.healthEvent.CallSevenArrowsSpecialMoveEvent();
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        StartCoroutine(ArrowOfTheSevenPlaguesRoutine(slotIndex, activeSkillData));
    }

    IEnumerator ArrowOfTheSevenPlaguesRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isArrowOfTheSevenActive = false;
        player.healthEvent.CallSevenArrowsWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    /// <summary>
    /// Execute Hunter's Reach special move    
    /// </summary>
    public void HuntersReach(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isHuntersReachActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.Grapple, default, 0, belongingEnemy: null);
    }

    /// <summary>
    /// Execute Rage special move
    /// </summary>
    public void Rage(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.animator.SetTrigger("rage");
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.healthEvent.CallRageSpecialMoveEvent(); // This is for displaying rage icon
            player.isRageActive = true;
            StartCoroutine(RageRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator RageRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        float meleeDamageIncrease = 0f;
        float armorDecrease = 0f;

        switch (player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel())
        {
            case 1:
                meleeDamageIncrease = 0.5f;
                armorDecrease = 0.25f;
                break;
            case 2:
                meleeDamageIncrease = 0.6f;
                armorDecrease = 0.2f;
                break;
            case 3:
                meleeDamageIncrease = 0.75f;
                armorDecrease = 0.15f;
                break;
            default:
                break;
        }

        player.additionalPhysicalDamageModifer += meleeDamageIncrease;
        player.currentArmorValue -= armorDecrease;
        player.UpdateDamageValues();
        player.UpdateArmorValues();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isRageActive = false;

        player.additionalPhysicalDamageModifer -= meleeDamageIncrease;
        player.currentArmorValue += armorDecrease;
        player.UpdateDamageValues();
        player.UpdateArmorValues();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.healthEvent.CallRageWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Shatter Cry special move
    /// </summary>
    public void ShatterCry(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.animator.SetTrigger("shatterCry");
            activeSkillTypeOneAnimator.gameObject.SetActive(true);

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.healthEvent.CallShatterCrySpecialMoveEvent(); // This is for displaying shatter cry icon
            player.isShatterCryActive = true;
            StartCoroutine(ShatterCryRoutine(slotIndex, activeSkillData));
        }
    }

    // Triggered By Animation Event
    private void PerformShatterCry()
    {
        CircleCollider2D circleCollider2D = activeSkillTypeOneAnimator.gameObject.GetComponent<CircleCollider2D>();

        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius);

        foreach (Collider2D col in colliders)
        {
            // Check if the collider belongs to an enemy or any other object you want to affect
            if (col.CompareTag(Settings.enemyTag))
            {
                // Apply damage to the enemy
                Enemy affectedEnemy = col.GetComponent<Enemy>();

                if (affectedEnemy.health.currentHealth > 0)
                {
                    IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);

                    // Apply effects
                    player.meleeAttackMainHand.CheckFearStatus(affectedEnemy, enemyCombatData, isShatterCry: true);
                }
            }
        }
    }

    IEnumerator ShatterCryRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isShatterCryActive = false;
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.healthEvent.CallShatterCryWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Whirlrend special move
    /// </summary>
    public void Whirlrend(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isWhirlrendActive)
            {
                player.animator.SetBool("whirlrend", true);
                activeSkillTypeTwoAnimator.gameObject.SetActive(true);
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                player.healthEvent.CallWhirlrendSpecialMoveEvent(); // This is for displaying rage icon
                player.isWhirlrendActive = true;
                StartCoroutine(WhirlrendRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator WhirlrendRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isWhirlrendActive = false;

        player.animator.SetBool("whirlrend", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.healthEvent.CallWhirlrendWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Axe throw special move
    /// </summary>
    public void AxeThrow(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        Weapon offhandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;
        player.isAxeThrowActive = true;

        player.playerSkillProjectile = player.playerDetails.throwingAxeDetails.projectilePrefabArray[0].GetComponentInChildren<Projectile>();
        offhandWeapon.isThrowingAxeWeapon = true;

        // Modify throwing axe's damage
        player.playerSkillProjectile.maxDamage = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
        {
            1 => player.currentMainHandMaxDamageValue,
            2 => player.currentMainHandMaxDamageValue * 2.5f,
            3 => player.currentMainHandMaxDamageValue * 3.5f,
            _ => player.currentMainHandMaxDamageValue
        };

        // OFF-HAND
        AttackShape offHandAttackType = player.playerControl.DetermineAttackType(offhandWeapon?.weaponDetails);
        player.meleeAttackEvent.CallAttackEvent(player.playerControl.aimDirection, offhandWeapon, offHandAttackType, MeleeHand.OffHand, false, false, false, false, false, false, isThrowingAxe: true);

        StartCoroutine(CloseThrowAxeState(activeSkillData));
    }

    IEnumerator CloseThrowAxeState(ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        yield return new WaitForSeconds(activeSkillData.cooldown * (1 - player.currentSkillCooldownReducer));

        player.isAxeThrowActive = false;
        player.AddNextWeaponToPlayer(ref DropItem.droppedThrowingAxe, true, false, false, false, equipOffHand: true);
        Destroy(DropOnAxeThrow.dropItemGameObject, 0.2f);
    }

    /// <summary>
    /// Execute Feast of War special move
    /// </summary>
    public void FeastOfWar(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isFeastOfWarActive)
            {
                player.healthEvent.CallFeastOfWarSpecialMoveEvent(); // This is for displaying valor icon
                player.isFeastOfWarActive = true;
                StartCoroutine(FeastOfWarRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator FeastOfWarRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isFeastOfWarActive = false;
        player.healthEvent.CallFeastOfWarWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Don't Blink special move
    /// </summary>
    public void DontBlink(int inputSlotNumber, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        StartCoroutine(DontBlinkRoutine(inputSlotNumber, activeSkillData));
    }

    IEnumerator DontBlinkRoutine(int inputSlotNumber, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        DontBlink dontBlink = activeSkillTypeOneAnimator.GetComponent<DontBlink>();

        // Get target position from mouse
        Vector3 mouseWorld = HelperUtilities.GetMouseWorldPosition(player);
        Vector2 targetPos = new Vector2(mouseWorld.x, mouseWorld.y);

        // Range Check
        if (!dontBlink.IsWithinRange(targetPos)) yield break;

        // Save original position
        Vector2 originalPos = player.rb2D.position;

        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Vector3Int pointerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(mouseWorld);

        // Check if the clicked tile is not marked as an obstacle
        if (!IsObstacleTile(currentRoom, pointerCellPosition))
        {
            player.health.isDamageable = false;

            player.cancelledDueToInvalidTile = false;
            int consumedMana = (int)(activeSkillData.manaCost * (1 - player.additionalManaReductionModifier));

            player.mana.ConsumeMana(consumedMana);

            // Teleport the character to the clicked tile
            player.rb2D.position = mouseWorld;

            // Disable movement + velocity
            player.rb2D.linearVelocity = Vector2.zero;

            // Trigger both hand attacks with DontBlink flag
            Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
            Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

            player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, mainHandWeapon, AttackShape.Thrust, MeleeHand.MainHand, false, false, false, false, isDontBlink: true);
            player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, offHandWeapon, AttackShape.Thrust, MeleeHand.OffHand, false, false, false, false, isDontBlink: true);

            // Wait for attack animation duration (must match animation event timing)
            yield return new WaitForSeconds(0.4f); // Adjust based on the animation that calls DetectColliders()

            player.isDontBlinkActive = false;

            // Teleport back to original position
            player.rb2D.linearVelocity = Vector2.zero;
            player.rb2D.position = originalPos;

            yield return new WaitForSeconds(0.6f);

            player.health.isDamageable = true;
        }
        else
        {
            player.cancelledDueToInvalidTile = true;
        }
    }

    /// <summary>
    /// Execute Venomous Ivy special move
    /// </summary>
    public void VenomousIvy(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);
            activeSkillTypeTwoAnimator.SetBool("venomousIvy", true);

            //SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isVenomousIvyActive = true;
            StartCoroutine(VenomousIvyRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator VenomousIvyRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isVenomousIvyActive = false;
        activeSkillTypeTwoAnimator.SetBool("venomousIvy", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Fade and Feed special move
    /// </summary>
    public void FadeAndFeed(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeThreeAnimator.gameObject.SetActive(true);
            activeSkillTypeThreeAnimator.SetBool("fadeAndFeed", true);

            // ENABLE SKILL EFFECTS
            player.healthEvent.CallFadeAndFeedSpecialMoveEvent(); // This is for displaying guarded oath icon

            // EFFECTS
            int gainedHealth = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 8,
                2 => 10,
                3 => 12,
                _ => 8
            };

            player.health.AddHealth(gainedHealth);

            player.UpdateDamageValues();
            player.UpdateBlockValue();
            player.UpdateArmorValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isFadeAndFeedActive = true;
            StartCoroutine(FadeAndFeedRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator FadeAndFeedRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        // ENABLE SKILL EFFECTS
        player.healthEvent.CallFadeAndFeedSpecialMoveEndEvent(); // This is for displaying fade and feed icon

        player.isFadeAndFeedActive = false;
        activeSkillTypeThreeAnimator.SetBool("fadeAndFeed", false);
        activeSkillTypeThreeAnimator.gameObject.SetActive(false);

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Blade Dash special move
    /// </summary>
    public void BladeDash(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData, bool isInitialCast)
    {
        float dashForce = 6f;     // Tune this based on weight/mass
        float dashDuration = 0.2f; // Very short, explosive
        player.playerControl.isDashing = true;

        // Get target position from mouse
        Vector3 mouseWorld = HelperUtilities.GetMouseWorldPosition(player);
        Vector2 targetPos = new Vector2(mouseWorld.x, mouseWorld.y);

        Vector2 direction = (targetPos - player.rb2D.position).normalized;

        player.animator.SetBool("bladeDash", true);
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        StartCoroutine(BladeDashRoutine(direction, dashForce, dashDuration, activeSkillData, isInitialCast));
    }

    IEnumerator BladeDashRoutine(Vector2 direction, float dashForce, float dashDuration, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, bool isInitialCast)
    {
        LayerMask originalMask = player.polygonCollider2D.forceReceiveLayers;
        player.polygonCollider2D.forceReceiveLayers = LayerMask.GetMask(); // Nothing

        player.isBladeAndDashActive = true;
        player.bladeAndDashOnRecast = !isInitialCast;

        float elapsed = 0f;
        float dashSpeed = dashForce / dashDuration;

        while (elapsed < dashDuration)
        {
            Vector2 moveStep = direction * dashSpeed * Time.fixedDeltaTime;
            player.rb2D.MovePosition(player.rb2D.position + moveStep);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        activeSkillTypeFourAnimator.gameObject.SetActive(false);

        // Reset collision
        player.polygonCollider2D.forceReceiveLayers = originalMask;

        player.playerControl.isDashing = false;
        player.animator.SetBool("bladeDash", false);
        player.isBladeAndDashActive = false;
    }

    /// <summary>
    /// Execute Shiruken special move
    /// </summary>
    public void Shiruken(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;
        player.isShirukenActive = true;

        player.playerSkillProjectile = player.playerDetails.shirukenDetails.projectilePrefabArray[0].GetComponentInChildren<Projectile>();

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.Shiruken, default, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isShirukenActive = false));
    }

    /// <summary>
    /// Execute Blizzard special move
    /// </summary>
    public void Blizzard(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isBlizzardActive)
            {
                player.isBlizzardActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(BlizzardRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator BlizzardRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeOneAnimator.gameObject.SetActive(true);

        GameObject blizzardObject = Instantiate(activeSkillTypeOneAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(player), Quaternion.identity);

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isBlizzardActive = false;

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", false);
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(blizzardObject, 2f); // Destroy blizzard object after animation completed
    }

    /// <summary>
    /// Execute Mycara's Seal special move
    /// </summary>
    public void MycarasSeal(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("mycarasSeal", true);
            player.healthEvent.CallMycarasSealSpecialMoveEvent(); // This is for displaying valor icon
            player.isMycarasSealActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            StartCoroutine(MycarasSealRoutine(slotIndex, activeSkillData));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator MycarasSealRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isMycarasSealActive = false;
        player.healthEvent.CallMycarasSealWoreOffEvent();
        activeSkillTypeTwoAnimator.SetBool("mycarasSeal", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Sheer Cold special move
    /// </summary>
    public void SheerCold(int slotIndex)
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Cone, MeleeHand.MainHand,
            false, false, false, true);
    }

    /// <summary>
    /// Execute Ice Breaker special move
    /// </summary>
    public void IceBreaker(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isIceBreakerActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.IceBreaker, default, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isIceBreakerActive = false));
    }

    /// <summary>
    /// Execute Absolute Zero special move
    /// </summary>
    public void AbsoluteZero(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isAbsoluteZeroActive)
            {
                player.isAbsoluteZeroActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(AbsoluteZeroRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator AbsoluteZeroRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        GameObject absoluteZeroObject = Instantiate(activeSkillTypeFourAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(player), Quaternion.identity);

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isAbsoluteZeroActive = false;

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", false);
        activeSkillTypeFourAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(absoluteZeroObject, 2f); // Destroy blizzard object after animation completed
    }

    /// <summary>
    /// Execute Fire Blast special move
    /// </summary>
    public void FireBlast(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isFireBlastActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.FireBlast, default, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isFireBlastActive = false));
    }

    /// <summary>
    /// Execute Molten Rift special move
    /// </summary>
    public void MoltenRift(int slotIndex)
    {
        player.isMoltenRiftActive = true;

        // Start playing teleport particle system
        if (teleportParticleRoutine != null)
        {
            StopCoroutine(teleportParticleRoutine);
        }
        teleportParticleRoutine = StartCoroutine(ParticleSystemRoutine());

        // Wait for mouse click to teleport the character
        InputManager.Instance.pointerPosition.action.performed += OnMoltenRiftInput;

        // Play special move sound effect
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        StartCoroutine(EnableInvincibility());
    }

    /// <summary>
    /// Enable Invincibility
    /// </summary>
    IEnumerator EnableInvincibility()
    {
        player.health.isDamageable = false;

        // 4 is duration of invincibility
        int iterations = Mathf.RoundToInt(4 / Health.spriteFlashInterval / 2);

        // Flash effect
        while (iterations > 0)
        {
            player.health.flashManager.WhiteFlashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            player.health.flashManager.UnflashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            iterations--;

            yield return null;
        }

        player.isMoltenRiftActive = false;
        player.health.isDamageable = true;
    }

    public void OnMoltenRiftInput(InputAction.CallbackContext context)
    {
        if (particlePlayed)
        {
            // Get current room and its bounds
            Room room = GameManager.Instance.GetCurrentRoom();

            // Get mouse world position and teleport the character
            Vector3 pointerWorldPosition = HelperUtilities.GetMouseWorldPosition(player);
            Vector3Int pointerCellPosition = room.instantiatedRoom.grid.WorldToCell(pointerWorldPosition);

            // Check if the clicked tile is not marked as an obstacle
            if (!IsObstacleTile(room, pointerCellPosition))
            {
                // Teleport the character to the clicked tile
                player.rb2D.position = pointerWorldPosition;

                // Stop playing teleport particle system
                player.specialMoveParticlesSystem.Stop();
                particlePlayed = false;

                // Unsubscribe from the event to prevent multiple teleports
                InputManager.Instance.pointerPosition.action.performed -= OnMoltenRiftInput;
            }
        }
    }

    /// <summary>
    /// Execute Flame Lotus special move
    /// </summary>
    public void FlameLotus(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isFlameLotusActive)
            {
                player.isFlameLotusActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(FlameLotusRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator FlameLotusRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeOneAnimator.gameObject.SetActive(true);

        GameObject flameLotusObject = Instantiate(activeSkillTypeOneAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(player), Quaternion.identity);

        flameLotusObject.GetComponent<Animator>().SetBool("flameLotus", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isFlameLotusActive = false;

        flameLotusObject.GetComponent<Animator>().SetBool("flameLotus", false);
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(flameLotusObject, 2f); // Destroy flame lotus object after animation completed
    }

    /// <summary>
    /// Execute Kynara's Embrace special move
    /// </summary>
    public void KynarasEmbrace(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("kynarasEmbrace", true);
            player.healthEvent.CallKynarasEmbraceSpecialMoveEvent(); // This is for displaying Kynara's Embrace icon
            player.isKynarasEmbraceActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            StartCoroutine(KynarasEmbraceRoutine(slotIndex, activeSkillData));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator KynarasEmbraceRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        if (!player.isKynarasEmbraceShieldExploding)
        {
            player.isKynarasEmbraceShieldExploding = false;
            player.healthEvent.CallKynarasEmbraceWoreOffEvent();

            activeSkillTypeTwoAnimator.SetTrigger("explosion");
            Explosion(slotIndex);
        }

        yield return new WaitForSeconds(0.5f); // Explosion duration

        player.isKynarasEmbraceActive = false;
        activeSkillTypeTwoAnimator.SetBool("kynarasEmbrace", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion(int slotIndex)
    {
        player.filter.SetLayerMask(player.layerMask);
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectTwo);

        Collider2D[] results = new Collider2D[20];
        int hitCount = Physics2D.OverlapCircle(transform.position, player.kynarasEmbraceCircleRadius, player.filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            if (collider is PolygonCollider2D)
            {
                // Don't hit yourself if player is also in the collider list
                if (collider.tag == Settings.playerTag) continue;

                if (collider.tag == Settings.enemyTag)
                {
                    Enemy enemy = collider.GetComponent<Enemy>();

                    Weapon weapon = player.activeWeapon.GetCurrentMainHandWeapon();
                    float damageModifier = 0f;

                    if (player != null)
                    {
                        damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 1f,
                            2 => 1.1f,
                            3 => 1.2f,
                            _ => 1f
                        };
                    }

                    int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(enemy, weapon, MeleeHand.MainHand, damageModifier);

                    DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = enemy.transform.position };
                    ReceiveProjectileDamage receiveProjectileDamage = enemy.GetComponent<ReceiveProjectileDamage>();
                    receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                    {
                        // Knockback
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        float knockbackForce = 5f;
                        float dealDamageMass = 1f;

                        enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Execute Blazing Cyclone special move
    /// </summary>
    public void BlazingCyclone(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isBlazingCycloneActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            ProjectileKind.BlazingCyclone, default, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isBlazingCycloneActive = false));
    }

    /// <summary>
    /// Execute Mist of Disruption special move
    /// </summary>
    public void MistOfDisruption(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isMistOfDisruptionActive)
            {
                player.isMistOfDisruptionActive = true;
                StartCoroutine(MistOfDisruptionRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator MistOfDisruptionRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        GameObject mistOfDisruptionObject = Instantiate(activeSkillTypeOneAnimator.gameObject, transform.position, Quaternion.identity);

        mistOfDisruptionObject.GetComponent<Animator>().SetBool("mistOfDisruption", true);

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isMistOfDisruptionActive = false;

        mistOfDisruptionObject.GetComponent<Animator>().SetBool("mistOfDisruption", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(mistOfDisruptionObject, 2f); // Destroy mist object after animation completed
    }

    /// <summary>
    /// Execute Nymara's Windveil special move
    /// </summary>
    public void NymarasWindveil(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("nymarasWindveil", true);
            player.healthEvent.CallNymarasWindveilSpecialMoveEvent(); // This is for displaying Nymara'w Windveil icon
            player.isNymarasWindveilActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            // Effects
            float speedIncrease = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                _ => 0,
            };

            player.additionalSpeedModifier += speedIncrease;
            player.UpdateSpeedValue();
            StaticEventHandler.CallStatsChangedOnTheBookEvent();

            StartCoroutine(NymarasWindveilRoutine(slotIndex, activeSkillData, speedIncrease));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator NymarasWindveilRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, float speedIncrease)
    {
        float duration = activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier); // Duration
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Windveil(slotIndex);
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }

        // Effects wore off
        player.additionalSpeedModifier -= speedIncrease;
        player.UpdateSpeedValue();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.isNymarasWindveilActive = false;
        player.healthEvent.CallNymarasWindveilWoreOffEvent();
        activeSkillTypeTwoAnimator.SetBool("nymarasWindveil", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Windveil(int slotIndex)
    {
        CircleCollider2D circleCollider = player.forcefieldTransform.GetComponent<CircleCollider2D>();

        if (circleCollider == null)
        {
            Debug.LogWarning("CircleCollider2D not found on forcefieldTransform.");
            return;
        }

        // Use world position of the forcefield object
        Vector2 origin = player.forcefieldTransform.position;

        // Convert local radius to world-space radius (in case of scaling)
        float worldRadius = circleCollider.radius * Mathf.Max(player.forcefieldTransform.lossyScale.x, player.forcefieldTransform.lossyScale.y);

        // Perform overlap check
        player.filter.SetLayerMask(player.layerMask);
        Collider2D[] results = new Collider2D[20];

        int hitCount = Physics2D.OverlapCircle(origin, worldRadius, player.filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            // Match by tag instead of collider type
            if (collider.CompareTag(Settings.enemyTag))
            {
                Enemy enemy = collider.GetComponent<Enemy>();

                if (enemy != null && !enemy.isStatic)
                {
                    IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(enemy.gameObject);

                    player.meleeAttackMainHand.CheckStaticStatus(enemy, enemyCombatData, isNymarasWindveil: true, player.isConductiveTouchActive);
                }
            }
        }
    }

    /// <summary>
    /// Execute Chain Lightning special move
    /// </summary>
    public void ChainLightning(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isChainLightningActive = true;
        //Reset precharge for loading again
        player.playerControl.isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser, 
            ProjectileKind.ChainLightning, new AttackContext { chainLightningPhase = ChainLightningPhase.First}, 0, belongingEnemy: null);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isChainLightningActive = false));
    }

    /// <summary>
    /// Execute Eye of the storm special move
    /// </summary>
    public void EyeOfTheStorm(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.isEyeOfTheStormActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            StartCoroutine(EyeOfTheStormRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator EyeOfTheStormRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        GameObject eyeOfTheStormObject = Instantiate(activeSkillTypeFourAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(player), Quaternion.identity);

        eyeOfTheStormObject.GetComponent<Animator>().SetBool("eyeOfTheStorm", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isEyeOfTheStormActive = false;

        eyeOfTheStormObject.GetComponent<Animator>().SetBool("eyeOfTheStorm", false);
        activeSkillTypeFourAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended

        Destroy(eyeOfTheStormObject, 2f); // Destroy eye of the storm object after animation completed
    }

    /// <summary>
    /// Execute Ionic Rejuvenation special move
    /// </summary>
    public void IonicRejuvenation(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeThreeAnimator.gameObject.SetActive(true);
            activeSkillTypeThreeAnimator.SetBool("ionicRejuvenation", true);

            // ENABLE SKILL EFFECTS
            player.healthEvent.CallIonicRejuvenationSpecialMoveEvent(); // This is for displaying ionic rejuvenation icon

            // EFFECTS
            ClearNegativeStatusEffects();

            int restoredHealth = 0;

            if (player.isInMistOfDisruption)
            {
                restoredHealth = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 12,
                    2 => 14,
                    3 => 16,
                    _ => 0
                };
            }
            else
            {
                restoredHealth = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 6,
                    2 => 7,
                    3 => 8,
                    _ => 0
                };
            }

            player.health.AddHealth(restoredHealth);

            player.UpdateDamageValues();
            player.UpdateBlockValue();
            player.UpdateArmorValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isIonicRejuvenationActive = true;
            StartCoroutine(IonicRejuvenationRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator IonicRejuvenationRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        // ENABLE SKILL EFFECTS
        player.healthEvent.CallIonicRejuvenationWoreOffEvent(); // This is for displaying ionic rejuvenation icon

        player.isIonicRejuvenationActive = false;
        activeSkillTypeThreeAnimator.SetBool("ionicRejuvenation", false);
        activeSkillTypeThreeAnimator.gameObject.SetActive(false);

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    IEnumerator NullifyBooleanAfterTwoFrame(System.Action setFalseAction)
    {
        yield return null;
        yield return null;

        setFalseAction?.Invoke();
    }

    IEnumerator ParticleSystemRoutine()
    {
        player.specialMoveParticlesSystem.Play();

        yield return new WaitForSeconds(0.3f);

        particlePlayed = true;
    }

    public void ViciousMomentumCheck()
    {
        if (player.isViciousMomentumActive)
        {
            if (player.viciousMomentumTriggered)
            {
                player.viciousMomentumCooldownTimer += Time.deltaTime;

                if (!player.viciousMomentumAttackBonusObtained)
                {
                    // Damage Boost
                    player.additionalPhysicalDamageModifer += 0.05f;
                    player.UpdateDamageValues();
                    player.healthEvent.CallViciousMomentumEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumAttackBonusObtained = true;
                }

                if (player.viciousMomentumCooldownTimer > player.viciousMomentumDuration)
                {
                    // Damage Reset
                    player.additionalPhysicalDamageModifer -= 0.05f;
                    player.UpdateDamageValues();
                    player.healthEvent.CallViciousMomentumWoreOffEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumCooldownTimer = 0f;
                    player.viciousMomentumTriggered = false;
                    player.viciousMomentumAttackBonusObtained = false;
                }
            }
        }
    }

    public void CombatFocusCheck()
    {
        if (player.isCombatFocusActive)
        {
            if (player.combatFocusTriggered)
            {
                player.combatFocusCooldownTimer += Time.deltaTime;

                Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                float originalIncreaseAmount = 0f;

                if (!player.combatFocusCooldownBonusObtained)
                {
                    if (mainHandWeapon != null)
                    {
                        switch (mainHandWeapon.weaponDetails.weaponClass)
                        {
                            case WeaponClass.Sword:
                            case WeaponClass.Axe:
                            case WeaponClass.Hammer:
                            case WeaponClass.Spear:
                            case WeaponClass.Dagger:
                            case WeaponClass.Claw:
                                originalIncreaseAmount = 0.2f;
                                player.additionalAttackCoolDownModifier += originalIncreaseAmount;
                                break;
                            case WeaponClass.Staff:
                            case WeaponClass.Bow:
                            case WeaponClass.Crossbow:
                                originalIncreaseAmount = 0.1f;
                                player.additionalAttackCoolDownModifier += originalIncreaseAmount;
                                break;
                            case WeaponClass.Shield:
                                break;
                            default:
                                break;
                        }
                    }

                    player.healthEvent.CallCombatFocusEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumAttackBonusObtained = true;
                }

                if (player.combatFocusCooldownTimer > player.combatFocusDuration)
                {
                    if (mainHandWeapon != null)
                    {
                        switch (mainHandWeapon.weaponDetails.weaponClass)
                        {
                            case WeaponClass.Sword:
                            case WeaponClass.Axe:
                            case WeaponClass.Hammer:
                            case WeaponClass.Spear:
                            case WeaponClass.Dagger:
                            case WeaponClass.Claw:
                                player.additionalAttackCoolDownModifier -= originalIncreaseAmount; // Cooldown reset
                                break;
                            case WeaponClass.Staff:
                            case WeaponClass.Bow:
                            case WeaponClass.Crossbow:
                                player.additionalAttackCoolDownModifier -= originalIncreaseAmount;
                                break;
                            case WeaponClass.Shield:
                                break;
                            default:
                                break;
                        }
                    }

                    player.healthEvent.CallCombatFocusWoreOffEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.combatFocusCooldownTimer = 0f;
                    player.combatFocusTriggered = false;
                    player.combatFocusCooldownBonusObtained = false;
                }
            }
        }
    }

    public void TriadExecutionCheck()
    {
        if (player.isTriadExecutionActive)
        {
            Weapon mainHandWeapon = player?.activeWeapon?.GetCurrentMainHandWeapon();
            float originalIncreaseAmount = 0f;

            if (player.triadExecutionCounter >= 3 && !player.triadExecutionTriggered)
            {
                player.triadExecutionTriggered = true;
                player.triadExecutionCounter = 0;

                if (mainHandWeapon != null)
                {
                    switch (mainHandWeapon.weaponDetails.weaponClass)
                    {
                        case WeaponClass.Sword:
                        case WeaponClass.Axe:
                        case WeaponClass.Hammer:
                        case WeaponClass.Spear:
                        case WeaponClass.Dagger:
                        case WeaponClass.Claw:
                            originalIncreaseAmount = 0.2f;
                            player.additionalPhysicalDamageModifer -= originalIncreaseAmount; // Cooldown reset
                            break;
                        case WeaponClass.Staff:
                        case WeaponClass.Bow:
                        case WeaponClass.Crossbow:
                            originalIncreaseAmount = 0.1f;
                            player.additionalPhysicalDamageModifer -= originalIncreaseAmount;
                            break;
                        case WeaponClass.Shield:
                            break;
                        default:
                            break;
                    }
                }

                player.UpdateDamageValues();
                player.healthEvent.CallTriadExecutionEvent();
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
            else
            {
                if (player.triadExecutionTriggered)
                {
                    player.additionalPhysicalDamageModifer -= originalIncreaseAmount;
                    player.UpdateDamageValues();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    StartCoroutine(DisableImageForTriadExecutionRoutine());

                    player.triadExecutionTriggered = false;
                }
            }
        }
    }

    IEnumerator DisableImageForTriadExecutionRoutine()
    {
        yield return new WaitForSeconds(1f);

        player.healthEvent.CallTriadExecutionWoreOffEvent();
    }

    public void ClearNegativeStatusEffects()
    {
        player.moveStatus = MoveStatus.Idle;
        player.healthStatus = HealthStatus.Normal;

        player.isCursed = false;
        player.isFeared = false;
        player.isRevealed = false;
        player.isCursed = false;
        player.isBlind = false;
        player.isSlowed = false;

        player.healthEvent.CallCuredCompletelyEvent();
    }

    private bool IsObstacleTile(Room room, Vector3Int cellPosition)
    {
        // Convert cell position to adjusted position relative to room bounds
        Vector2Int adjustedCellPosition = new Vector2Int(cellPosition.x - room.templateLowerBounds.x,
            cellPosition.y - room.templateLowerBounds.y);

        // Check if the adjusted cell position is within the valid range
        if (adjustedCellPosition.x < 0 || adjustedCellPosition.y < 0 || adjustedCellPosition.x >= room.instantiatedRoom.
            aStarMovementPenalty.GetLength(0) || adjustedCellPosition.y >= room.instantiatedRoom.aStarMovementPenalty.GetLength(1))
        {
            // Cell position is outside the valid range (out of bounds)
            return true; // Treat it as an obstacle
        }

        // Check if the cell is marked as an obstacle
        return room.instantiatedRoom.aStarMovementPenalty[adjustedCellPosition.x, adjustedCellPosition.y] == 0;
    }
}
