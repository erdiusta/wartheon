using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DestroyedEvent))]
[DisallowMultipleComponent]
public class Destroyed : MonoBehaviour
{
    public UnityEvent OnDeathAnimationFinished;

    DestroyedEvent destroyedEvent;
    Enemy enemy;
    Player player;
    Decoy decoy;

    private void Awake()
    {
        destroyedEvent = GetComponent<DestroyedEvent>();
        enemy = GetComponent<Enemy>();
        player = GetComponent<Player>();
        decoy = GetComponent<Decoy>();
    }

    private void OnEnable()
    {
        destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;

        if (enemy != null)
        {
            OnDeathAnimationFinished.AddListener(DeathProcessAfterAnimationCompleted);
        }
    }

    private void OnDisable()
    {
        destroyedEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;

        if (enemy != null)
        {
            OnDeathAnimationFinished.RemoveListener(DeathProcessAfterAnimationCompleted);
        }
    }

    public void CallDeathAnimationFinish()
    {
        OnDeathAnimationFinished?.Invoke();
    }

    private void DestroyedEvent_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        // Remove any object from dynamic game objects in scene
        SceneObjectsManager.Instance.dynamicGameObjectsInScene.Remove(gameObject);

        if (destroyedEventArgs.playerDied)
        {
            player.polygonCollider2D.enabled = false;
            player.animatePlayer.ResetAnimatonParameters();
            player.animator.SetBool(Settings.death, true);
            player.animator.Play("Death", 0, 0);
            player.idle.StopVelocity();
            InputManager.glossaryDisabled = true; // Disable glossary

            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.deathSoundEffect);
        }
        else
        {
            if (decoy != null)
            {
                SoundEffectManager.Instance.PlaySoundEffect(decoy.activeItemDetails.activeItemImpactSoundEffect);
                Destroy(gameObject, 0.4f);
                return;
            }

            if (gameObject.GetComponent<Enemy>().enemyDetails?.deathSoundEffect != null)
            {
                SoundEffectManager.Instance.PlaySoundEffect(gameObject.GetComponent<Enemy>().enemyDetails.deathSoundEffect);
            }

            Player player = GameManager.Instance.GetPlayer();

            // Upon death, add experience points to player's related branch mastery points
            switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass)
            {
                case WeaponClass.Sword:
                    player.branchMastery.swordMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Axe:
                    player.branchMastery.axeMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Hammer:
                    player.branchMastery.hammerMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Shield:
                    break;
                case WeaponClass.Spear:
                    player.branchMastery.spearMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Staff:
                    player.branchMastery.staffMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Bow:
                    player.branchMastery.bowMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponClass.Dagger:
                    player.branchMastery.daggerMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                default:
                    break;
            }

            // Upon death, add experience points to player's related weapon mastery points
            switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponTitle)
            {
                case WeaponTitle.None:
                    break;
                case WeaponTitle.Gladius:
                    player.weaponMastery.gladiusMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Scimitar:
                    player.weaponMastery.scimitarMasterPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.SizzlingSword:
                    player.weaponMastery.sizzlingSwordMasterPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Hatchet:
                    player.weaponMastery.hatchetMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.PhalanxSpear:
                    player.weaponMastery.phalanxSpearMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.ClobberingTime:
                    player.weaponMastery.clobberingTimeMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.HolySword:
                    player.weaponMastery.holySwordMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.AncientKatana:
                    player.weaponMastery.ancientKatanaMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Carnage:
                    player.weaponMastery.carnageMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Crusher:
                    player.weaponMastery.crusherMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Dirk:
                    player.weaponMastery.dirkMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Gambit:
                    player.weaponMastery.gambitMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.CrudeBow:
                    player.weaponMastery.bowMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Crossbow:
                    player.weaponMastery.crossBowMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.OldStaff:
                    player.weaponMastery.staffMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.SolarFlare:
                    player.weaponMastery.solarFlareMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.HeavensGale:
                    player.weaponMastery.heavensGaleMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                default:
                    break;
            }

            // Gain Experience Upon Killing An Enemy
            int gainedExpFromEnemy = (int)(enemy.enemyDetails.experiencePoint * player.expGainModifier);
            player.currentGainedTotalExperiencePoints += gainedExpFromEnemy;
            StaticEventHandler.CallExpGained();

            // Check if player levels-after killing the enemy
            int levelBeforeKillingEnemy = player.currentLevel;
            LevelUpCheck(player, levelBeforeKillingEnemy);

            if (enemy.enemyDetails.isEnemyBoss)
            {
                EnemySpawner.Instance.isBossInstantiated = false;

                switch (enemy.enemyDetails.enemyBehaviour)
                {
                    case EnemyBehaviour.Pursuit:
                        enemy.enemyAI.StopAllCoroutines();
                        break;
                    case EnemyBehaviour.AimAndShoot:
                        enemy.GetComponent<EnemyAimAndShootAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.PrepareAndDash:
                        enemy.enemyAI.StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Centaur:
                        enemy.GetComponent<MoravelleAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Sylvarok:
                        enemy.GetComponent<TreantAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Galvanus:
                        enemy.GetComponent<GalvanusAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Sepharoth:
                        enemy.GetComponent<SepharothAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Cryothar:
                        enemy.GetComponent<FrostWrymAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Venomancer:
                        enemy.GetComponent<VenomancerAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Pyrothar:
                        enemy.GetComponent<FireWrymAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Moldran:
                        enemy.GetComponent<MoldranAI>().StopAllCoroutines();
                        break;
                    case EnemyBehaviour.Roaming:
                        enemy.enemyAI.StopAllCoroutines();
                        break;
                    default:
                        break;
                }

                enemy.animateEnemy.ResetAnimatonParameters();


                if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Sylvarok)
                {
                    foreach (Transform minion in EnemySpawner.Instance.transform)
                    {
                        if (minion.GetComponent<Enemy>().enemyDetails.enemyBehaviour == EnemyBehaviour.Sylvarok) continue;
  
                        minion.GetComponent<DestroyedEvent>().CallDestroyedEvent(false);
                    }
                }
            }

            if (InputManager.TutorialEnabled)
            {
                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
                {
                    player.health.isDamageable = true;
                    player.health.TakeDamage(20, Vector2.zero, player.transform.position, false, null, MeleeHand.None);
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }
                else if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }
                else if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }
                else if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.SpecialSkill)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }
            }

            if (enemy.enemyDetails.enemyCategory == EnemyCategory.MainSlime && !enemy.minionsSpawned)
            {
                enemy.minionsSpawned = true;
                StaticEventHandler.CallEnemyKilledEvent(enemy);
            }

            enemy.health.hitFXAnimator.SetTrigger(Settings.death);
            enemy.health.fxAnimatorPlayed = true;
            enemy.enemyAI.isDashing = false;
            enemy.enemyAI.isAttacking = false;
            enemy.enemyAI.enemyPhase = EnemyPhase.Death;
            enemy.health.ResetStatusInCaseOfDeath();
            enemy.rb2D.mass = 5000;
            enemy.rb2D.linearVelocity = Vector2.zero;

            if (!enemy.enemyDetails.isEnemyBoss)
            {
                enemy.patrol.enabled = false;
                enemy.aiDestinationSetter.enabled = false;
                enemy.aiRigidbody2D.canMove = false;
            }

            enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
            enemy.animateEnemy.SetDeathAnimationParameters();
            enemy.animator.Play("Death", 0, 0);
            enemy.fireWeapon.enabled = false;
            GetComponent<PolygonCollider2D>().enabled = false;
        }
    }

    private void LevelUpCheck(Player player, int levelBeforeKillingEnemy)
    {
        #region LevelUpExpThresholds
        for (int i = 0; i < player.levelUpDetails.playerLevelDataList.Count; i++)
        {
            if (player.currentLevel == player.levelUpDetails.playerLevelDataList[i].playerLevel &&
                player.currentGainedTotalExperiencePoints >= player.levelUpDetails.playerLevelDataList[i].levelUpExpPointForNextLevel)
            {
                if (GameManager.isDemo && player.currentLevel >= 4)
                {

                }
                else
                {
                    player.currentLevel++;
                }
            }
        }
        #endregion

        // If current level is more than level before killing enemy, it means char leveled up!
        if (player.currentLevel > levelBeforeKillingEnemy && !player.health.hasDied)
        {
            player.levelUpAnimator.SetTrigger(Settings.levelUp);
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.levelUpSoundEffect);
            player.currentSkillPoints++;
            player.currentStatPoints += 3;
            StaticEventHandler.CallStatPointChangedEvent();
            StaticEventHandler.CallLevelUp();
            player.health.SetMaximumHealth(player.health.maximumHealth, true);
            player.mana.AddMana(30);
            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
            player.manaEvent.CallManaChangedEvent(player.mana.currentMana);
        }
    }

    public void DeathProcessAfterAnimationCompleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
