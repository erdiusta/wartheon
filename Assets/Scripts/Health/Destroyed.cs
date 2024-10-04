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
                    Debug.Log("Player's new gladius mastery point is " + player.weaponMastery.gladiusMasteryPoints);
                    break;
                case WeaponTitle.Scimitar:
                    player.weaponMastery.scimitarMasterPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.SizzlingSword:
                    player.weaponMastery.sizzlingSwordMasterPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.Hatchet:
                    player.weaponMastery.hatchetMasteryPoints += enemy.enemyDetails.experiencePoint;
                    Debug.Log("Player's new hatchet mastery point is " + player.weaponMastery.hatchetMasteryPoints);
                    break;
                case WeaponTitle.PhalanxSpear:
                    player.weaponMastery.phalanxSpearMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.ClobberingTime:
                    player.weaponMastery.clobberingTimeMasteryPoints += enemy.enemyDetails.experiencePoint;
                    break;
                case WeaponTitle.HolySword:
                    player.weaponMastery.holySwordMasteryPoints += enemy.enemyDetails.experiencePoint;
                    Debug.Log("Player's new holy sword mastery point is " + player.weaponMastery.holySwordMasteryPoints);
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
                case WeaponTitle.Bow:
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

            if (enemy.enemyDetails.isEnemyBoss)
            {
                EnemySpawner.Instance.isBossInstantiated = false;
            }

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

    public void DeathProcessAfterAnimationCompleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
