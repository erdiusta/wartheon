using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DestroyedEvent))]
[DisallowMultipleComponent]
public class Destroyed : MonoBehaviour
{
    public UnityEvent OnDeathAnimationFinished;

    DestroyedEvent destroyedEvent;
    Enemy enemy;
    IEnemyCombatData enemyCombatData;
    Player player;
    Dummy decoy;
    bool deathSoundPlayed;
    private void Awake()
    {
        destroyedEvent = GetComponent<DestroyedEvent>();
        enemy = GetComponent<Enemy>();
        player = GetComponent<Player>();
        decoy = GetComponent<Dummy>();
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
        SceneObjectsManager.dynamicGameObjectsInScene.Remove(gameObject);

        if (destroyedEventArgs.playerDied)
        {
            if (!deathSoundPlayed)
            {
                player.polygonCollider2D.enabled = false;
                player.animatePlayer.ResetAnimatonParameters();
                player.animator.SetBool(Settings.death, true);
                player.animator.Play("Death", 0, 0);
                GetComponent<PolygonCollider2D>().enabled = false;
                player.idle.StopVelocity();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

                InputManager.glossaryDisabled = true; // Disable glossary

                if (!NetworkServer.active && !NetworkClient.active) WorldSoundManager.Instance.PlayWorldSound(player.playerDetails.deathSoundEffect, transform.position);
                else if (NetworkClient.active)
                {
                    GameSessionManager.Instance.SetGameState(GameState.gameLost);

                    NetworkSoundManager.Instance.CmdPlaySound(SoundName.PlayerDeath, transform.position);
                }
                    
                 deathSoundPlayed = true;
            }
        }
        else
        {
            if (decoy != null)
            {
                Destroy(gameObject, 0.4f);
                return;
            }

            Player killerPlayer;
            enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(gameObject);

            if (!NetworkServer.active && !NetworkClient.active)
            {
                if (gameObject.GetComponent<Enemy>().enemyDetails?.deathSoundEffect != null)
                {
                    SoundEffectManager.Instance.PlaySoundEffect(gameObject.GetComponent<Enemy>().enemyDetails.deathSoundEffect);
                }

                killerPlayer = GameManager.Instance.GetLocalPlayer();
            }
            else
            {
                killerPlayer = ResolveKiller(enemy.health.LastDamageDealerNetId);
            }

            if (killerPlayer != null)
            {
                // Upon death, add experience points to player's related branch mastery points
                switch (killerPlayer.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass)
                {
                    case WeaponClass.Sword:
                        killerPlayer.branchMastery.swordMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Axe:
                        killerPlayer.branchMastery.axeMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Hammer:
                        killerPlayer.branchMastery.hammerMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Shield:
                        break;
                    case WeaponClass.Spear:
                        killerPlayer.branchMastery.spearMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Staff:
                        killerPlayer.branchMastery.staffMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Bow:
                        killerPlayer.branchMastery.bowMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    case WeaponClass.Dagger:
                        killerPlayer.branchMastery.daggerMasteryPoints += enemyCombatData.ExperiencePoints;
                        break;
                    default:
                        break;
                }

                // Gain Experience Upon Killing An Enemy
                int gainedExpFromEnemy = (int)(enemyCombatData.ExperiencePoints * killerPlayer.expGainModifier);
                killerPlayer.currentGainedTotalExperiencePoints += gainedExpFromEnemy;
                StaticEventHandler.CallExpGained();

                // Check if player levels-after killing the enemy
                int levelBeforeKillingEnemy = killerPlayer.currentLevel;
                LevelUpCheck(killerPlayer, levelBeforeKillingEnemy);

                if (enemyCombatData.Isboss)
                {
                    switch (enemyCombatData.EnemyBehaviour)
                    {
                        case EnemyBehaviour.Pursuit:
                            if (NetworkServer.active) enemy.enemyAINetwork.StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.enemyAI.StopAllCoroutines();
                            break;
                        case EnemyBehaviour.AimAndShoot:
                            if (NetworkServer.active) enemy.GetComponent<EnemyAimAndShootAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<EnemyAimAndShootAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.PrepareAndDash:
                            if (NetworkServer.active) enemy.enemyAINetwork.StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.enemyAI.StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Moravelle:
                            if (NetworkServer.active) enemy.GetComponent<MoravelleAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<MoravelleAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Sylvarok:
                            if (NetworkServer.active) enemy.GetComponent<SylvarokAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<SylvarokAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Galvanus:
                            if (NetworkServer.active) enemy.GetComponent<GalvanusAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<GalvanusAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Sepharoth:
                            if (NetworkServer.active) enemy.GetComponent<SepharothAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<SepharothAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Cryothar:
                            if (NetworkServer.active) enemy.GetComponent<CryotharAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<CryotharAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Venomancer:
                            if (NetworkServer.active) enemy.GetComponent<VenomancerAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<VenomancerAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Pyrothar:
                            if (NetworkServer.active) enemy.GetComponent<PyrotharAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<PyrotharAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Moldran:
                            if (NetworkServer.active) enemy.GetComponent<MoldranAINetwork>().StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.GetComponent<MoldranAI>().StopAllCoroutines();
                            break;
                        case EnemyBehaviour.Roaming:
                            if (NetworkServer.active) enemy.enemyAINetwork.StopAllCoroutines();
                            else if (!NetworkServer.active && !NetworkClient.active) enemy.enemyAI.StopAllCoroutines();
                            break;
                        default:
                            break;
                    }

                    enemy.animateEnemy.ResetAnimatonParameters();
                    enemy.enemyAnimSync?.ResetAllAnimations();

                    if (enemyCombatData.EnemyBehaviour == EnemyBehaviour.Sylvarok)
                    {
                        if (!NetworkServer.active && !NetworkClient.active)
                        {
                            foreach (Transform minion in GameManager.Instance.GetCurrentRoom().instantiatedRoom.enemySpawner.transform)
                            {
                                if (EnemyDataResolver.Resolve<IEnemyCombatData>(minion.gameObject).EnemyBehaviour == EnemyBehaviour.Sylvarok) continue;

                                DestroyUtility.Destroy(minion.gameObject, false, minion.GetComponent<Health>().LastDamageDealerNetId);
                            }
                        }
                        else
                        {
                            // Multiplayer Logic
                            if (!NetworkServer.active) return;

                            enemy.GetComponent<SylvarokAINetwork>().KillAllSummons();

                        }
                    }
                }

                if (InputManager.TutorialEnabled)
                {
                    if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
                    {
                        killerPlayer.health.healthAuthority.IsDamageable = true;

                        DamageContext ctx = new DamageContext { receiverPosition = killerPlayer.transform.position };
                        HealthAuthorityResolver.GetAuthority(killerPlayer.gameObject).ApplyDamage(20, ctx);

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

                if (enemyCombatData.EnemyCategory == EnemyCategory.MainSlime && !enemy.minionsSpawned)
                {
                    enemy.minionsSpawned = true;
                    StaticEventHandler.CallEnemyKilledEvent(enemy);
                }

                if (killerPlayer != null && killerPlayer.isViciousMomentumActive)
                {
                    killerPlayer.viciousMomentumCooldownTimer = 0f;
                    killerPlayer.viciousMomentumTriggered = true;
                }

                if (killerPlayer != null && killerPlayer.isCombatFocusActive)
                {
                    killerPlayer.combatFocusCooldownTimer = 0f;
                    killerPlayer.combatFocusTriggered = true;
                }
            }

            EnemyRegistry.ActiveEnemies.Remove(enemy);
            enemy.health.fxAnimatorPlayed = true;

            if (NetworkServer.active)
            {
                enemy.enemyAINetwork.isDashing = false;
                enemy.enemyAINetwork.isAttacking = false;
                enemy.enemyAINetwork.enemyPhase = EnemyPhase.Death;
            }
            else if(!NetworkServer.active && !NetworkClient.active)
            {
                enemy.enemyAI.isDashing = false;
                enemy.enemyAI.isAttacking = false;
                enemy.enemyAI.enemyPhase = EnemyPhase.Death;
            }

            enemy.rb2D.mass = 5000;
            enemy.rb2D.linearVelocity = Vector2.zero;

            if (!enemyCombatData.Isboss)
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
            player.currentStatPoints += 2;
            StaticEventHandler.CallStatPointChangedEvent();
            StaticEventHandler.CallLevelUp();
            player.health.SetMaximumHealth(player.health.maximumHealth, true);
            player.mana.AddMana(30);
            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
            player.manaEvent.CallManaChangedEvent(player.mana.currentMana);
        }
    }

    private Player ResolveKiller(uint killerNetId)
    {
        if (killerNetId == 0) return null;

        foreach (Player p in GameSessionManager.Instance.ServerPlayers)
        {
            if (p.NetAuth.netId == killerNetId) return p;
        }

        return null;
    }

    public void DeathProcessAfterAnimationCompleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
