using UnityEngine;
using UnityEngine.Events;

public class SeismicSlam : MonoBehaviour
{
    public UnityEvent OnSlamSoundPlayed;
    public UnityEvent OnSlamActivated;
    public UnityEvent OnSlamDeactivated;

    bool isSlamEnabled;
    bool isSlamUsed;
    Player player;
    CircleCollider2D circleCollider2D;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        circleCollider2D.enabled = false;
        OnSlamActivated.AddListener(EnableSlam);
        OnSlamDeactivated.AddListener(DisableSlam);
        OnSlamSoundPlayed.AddListener(PlaySlamSound);
    }

    private void OnDisable()
    {
        OnSlamActivated.RemoveListener(EnableSlam);
        OnSlamDeactivated.RemoveListener(DisableSlam);
        OnSlamSoundPlayed.RemoveListener(PlaySlamSound);
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    public void ActivatePlaySoundEvent()
    {
        OnSlamSoundPlayed?.Invoke();
    }

    public void ActivateSlamEvent()
    {

        OnSlamActivated?.Invoke();
    }

    public void DeactivateSlamEvent()
    {
        OnSlamDeactivated?.Invoke();
    }

    public void EnableSlam()
    {
        if (!isSlamEnabled)
        {
            isSlamEnabled = true;
            circleCollider2D.enabled = true;

            // Get all colliders within the radius of the seismic slam
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius);

            StaticEventHandler.CallCameraShakeEvent(player.playerDetails.shakeIntensity, player.playerDetails.shakeDuration);

            foreach (Collider2D col in colliders)
            {
                // Check if the collider belongs to an enemy or any other object you want to affect
                if (col.CompareTag(Settings.enemyTag))
                {
                    // Apply damage to the enemy
                    Enemy enemy = col.GetComponent<Enemy>();

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemy.health.currentHealth > 0)
                    {
                        // Knockback
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        float knockbackForce = 9f; // Seismic slam hit force
                        float dealDamageMass = 1f;

                        enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                    }

                    if (enemy.health != null)
                    {
                        enemy.health.TakeDamage(player.seismicSlamDamage, transform.position, enemy.health.transform.position, null, MeleeHand.None);
                    }
                }
            }
        }
    }

    public void DisableSlam() => isSlamEnabled = false;

    private void PlaySlamSound() => SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.firstActiveSkillDetails.activeUniqueSkillSoundEffectOne);
}
