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
                GameManager.Instance.GetPlayer().isDead = true;
                GetComponent<PolygonCollider2D>().enabled = false;
                GameManager.Instance.GetPlayer().animatePlayer.ResetAnimatonParameters();
                GameManager.Instance.GetPlayer().animator.SetBool(Settings.death, true);

                SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.deathSoundEffect);
                Destroy(gameObject, 1f);
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

            enemy.isDead = true;

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

                if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Treant || enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Galvanus ||
                    enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Centaur)
                {
                    enemy.animator.SetBool(Settings.cast, false);
                }

                if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Treant)
                {
                    foreach (Transform minion in EnemySpawner.Instance.transform)
                    {
                        if (minion.GetComponent<Enemy>().enemyDetails.enemyBehaviour == EnemyBehaviour.Treant) continue;
  
                        minion.GetComponent<DestroyedEvent>().CallDestroyedEvent(false);
                    }
                }
            }

            if (enemy.enemyDetails.enemyCategory == EnemyCategory.Skeleton)
            {
                enemy.animator.SetBool(Settings.block, false);
            }

            enemy.health.hitFXAnimator.SetTrigger(Settings.death);
            enemy.health.fxAnimatorPlayed = true;
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.enemyAI.isDashing = false;
            enemy.isDead = true;
            enemy.enemyAI.isAttacking = false;
            enemy.enemyAI.StopAllCoroutines();
            enemy.enemyAI.enabled = false;
            enemy.health.StopAllCoroutines();
            enemy.health.ResetStatusInCaseOfDeath();
            enemy.health.enabled = false;
            enemy.rb2D.mass = 5000;
            enemy.rb2D.linearVelocity = Vector2.zero;

            if (!enemy.enemyDetails.isEnemyBoss)
            {
                enemy.patrol.enabled = false;
                enemy.aiDestinationSetter.enabled = false;
                enemy.aiLerp.canMove = false;
            }

            enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
            enemy.animateEnemy.SetDeathAnimationParameters();
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
        if (player.currentLevel > levelBeforeKillingEnemy)
        {
            player.levelUpAnimator.SetTrigger(Settings.levelUp);
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.levelUpSoundEffect);
            player.currentBuildPoints++;
            StaticEventHandler.CallLevelUp();
            player.health.SetMaximumHealth(player.health.GetMaximumHealth());
            player.UpdatePlayerHealth(1, true, true);
        }
    }

    public void DeathProcessAfterAnimationCompleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
