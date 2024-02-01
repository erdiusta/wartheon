using UnityEngine;

[RequireComponent(typeof(DestroyedEvent))]
[DisallowMultipleComponent]
public class Destroyed : MonoBehaviour
{
    DestroyedEvent destroyedEvent;

    private void Awake()
    {
        destroyedEvent = GetComponent<DestroyedEvent>();
    }

    private void OnEnable()
    {
        destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void OnDisable()
    {
        destroyedEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;
    }

    private void DestroyedEvent_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        if (destroyedEventArgs.playerDied)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (gameObject.tag == "Player")
            {
                SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.deathSoundEffect);
            }
            else
            {
                SoundEffectManager.Instance.PlaySoundEffect(gameObject.GetComponent<Enemy>().enemyDetails.deathSoundEffect);
                GetComponent<PolygonCollider2D>().enabled = false;
                GetComponent<EnemyMovementAI>().enabled = false;
                Destroy(gameObject, 1f);
            }
<<<<<<< Updated upstream
=======

            GetComponent<Enemy>().isDead = true;
            GetComponent<FireWeapon>().enabled = false;
            GetComponent<Knockback>().knockbackForce = 0f;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<EnemyMovementAI>().enabled = false;
            GetComponent<AnimateEnemy>().attackLayerIndex = 0;
            Destroy(gameObject, 0.6f);
>>>>>>> Stashed changes
        }
    }
}
