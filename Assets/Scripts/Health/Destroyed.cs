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
        }
    }
}
