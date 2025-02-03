using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DestroyedEvent))]
[DisallowMultipleComponent]
public class Destroyed : MonoBehaviour
{
    public UnityEvent OnDeathAnimationFinished;

    DestroyedEvent destroyedEvent;
    Enemy enemy;

    private void Awake()
    {
        destroyedEvent = GetComponent<DestroyedEvent>();
        enemy = GetComponent<Enemy>();
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
        if (destroyedEventArgs.playerDied)
        {
            if (destroyedEventArgs.isClone)
            {
                Destroy(GameManager.Instance.GetPlayer().playerCloneObject);
            }
            else
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                gameObject.SetActive(false);
                SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.deathSoundEffect);
            }
        }
        else
        {
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
                case WeaponTitle.Staff:
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

            enemy.isDead = true;

            // Gain Experience Upon Killing An Enemy
            int gainedExpFromEnemy = (int)(enemy.enemyDetails.experiencePoint * player.expGainModifier);
            player.currentGainedTotalExperiencePoints += gainedExpFromEnemy;

            // Check if player levels-after killing the enemy
            int levelBeforeKillingEnemy = player.currentLevel;
            LevelUpCheck(player, levelBeforeKillingEnemy);

            if (enemy.enemyDetails.isEnemyBoss)
            {
                EnemySpawner.Instance.isBossInstantiated = false;
            }

            if (enemy.enemyDetails.enemyName == "Skeleton")
            {
                enemy.animator.SetBool(Settings.block, false);
            }

            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.enemyAI.isDashing = false;
            enemy.enemyAI.isAttacking = false;
            enemy.enemyAI.StopAllCoroutines();
            enemy.enemyAI.enabled = false;
            enemy.health.StopAllCoroutines();
            enemy.health.ResetStatusInCaseOfDeath();
            enemy.health.enabled = false;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            enemy.animateEnemy.SetDeathAnimationParameters();
            enemy.fireWeapon.enabled = false;
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    private void LevelUpCheck(Player player, int levelBeforeKillingEnemy)
    {
        #region LevelUpExpThresholds
        if (player.currentGainedTotalExperiencePoints >= 21825)
        {
            player.currentLevel = 19;
        }
        else if (player.currentGainedTotalExperiencePoints >= 19550)
        {
            player.currentLevel = 18;
        }
        else if (player.currentGainedTotalExperiencePoints >= 17400)
        {
            player.currentLevel = 17;
        }
        else if (player.currentGainedTotalExperiencePoints >= 15375)
        {
            player.currentLevel = 16;
        }
        else if (player.currentGainedTotalExperiencePoints >= 13475)
        {
            player.currentLevel = 15;
        }
        else if (player.currentGainedTotalExperiencePoints >= 11700)
        {
            player.currentLevel = 14;
        }
        else if (player.currentGainedTotalExperiencePoints >= 10050)
        {
            player.currentLevel = 13;
        }
        else if (player.currentGainedTotalExperiencePoints >= 8525)
        {
            player.currentLevel = 12;
        }
        else if (player.currentGainedTotalExperiencePoints >= 7125)
        {
            player.currentLevel = 11;
        }
        else if (player.currentGainedTotalExperiencePoints >= 5850)
        {
            player.currentLevel = 10;
        }
        else if (player.currentGainedTotalExperiencePoints >= 4700)
        {
            player.currentLevel = 9;
        }
        else if (player.currentGainedTotalExperiencePoints >= 3675)
        {
            player.currentLevel = 8;
        }
        else if (player.currentGainedTotalExperiencePoints >= 2775)
        {
            player.currentLevel = 7;
        }
        else if (player.currentGainedTotalExperiencePoints >= 2000)
        {
            player.currentLevel = 6;
        }
        else if (player.currentGainedTotalExperiencePoints >= 1350)
        {
            player.currentLevel = 5;
        }
        else if (player.currentGainedTotalExperiencePoints >= 825)
        {
            player.currentLevel = 4;
        }
        else if (player.currentGainedTotalExperiencePoints >= 425)
        {
            player.currentLevel = 3;
        }
        else if (player.currentGainedTotalExperiencePoints >= 150)
        {
            player.currentLevel = 2;
        }
        else
        {
            player.currentLevel = 1;
        }
        #endregion

        // If current level is more than level before killing enemy, it means char leveled up!
        if (player.currentLevel > levelBeforeKillingEnemy)
        {
            player.levelUpAnimator.SetTrigger(Settings.levelUp);
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.levelUpSoundEffect);
            player.currentBuildPoints++;
            StaticEventHandler.CallBuildPointsGained();
            player.health.SetMaximumHealth(player.health.GetMaximumHealth());
        }
    }

    public void DeathProcessAfterAnimationCompleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
