using UnityEngine;
using UnityEngine.Events;

public class MistOfDisruption : MonoBehaviour
{
    public UnityEvent OnMistActivated;
    public UnityEvent OnMistDeactivated;

    bool isMistEnabled;
    Player player;

    private void OnEnable()
    {
        OnMistActivated.AddListener(EnableMist);
        OnMistDeactivated.AddListener(DisableMist);
    }

    private void OnDisable()
    {
        OnMistActivated.RemoveListener(EnableMist);
        OnMistDeactivated.RemoveListener(DisableMist);
    }

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void ActivateMistEvent()
    {
        OnMistActivated?.Invoke();
    }

    public void DeactivateMistEvent()
    {
        OnMistDeactivated?.Invoke();
    }

    public void EnableMist() => isMistEnabled = true;

    public void DisableMist() => isMistEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.enemyTag))
        {
            Enemy affectedEnemy = collision.GetComponent<Enemy>();

            affectedEnemy.isDisoriented = isMistEnabled;
            affectedEnemy.disorientDuration = 5f;
        }

        if (collision.CompareTag(Settings.playerTag) && !player.isInMistOfDisruption)
        {
            player.isInMistOfDisruption = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag))
        {
            player.isInMistOfDisruption = false;
        }
    }
}
