using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public GameObject poisonImage;

    Player player;
    Enemy enemy;

    void Awake()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.healthEvent.GetPoisoned += EnablePoisonImage;
            player.healthEvent.PoisonCured += DisablePoisonImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned += EnablePoisonImage;
            enemy.healthEvent.PoisonCured += DisablePoisonImage;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.healthEvent.GetPoisoned -= EnablePoisonImage;
            player.healthEvent.PoisonCured -= DisablePoisonImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned -= EnablePoisonImage;
            enemy.healthEvent.PoisonCured -= DisablePoisonImage;
        }
    }

    private void EnablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(true);
    }

    private void DisablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(false);
    }
}
