using System;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public GameObject poisonImage;
    public GameObject acidImage;
    public GameObject stunImage;
    public GameObject silverArmorImage;
    public GameObject deathImage;

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
            player.healthEvent.GetAcid += EnableAcidImage;
            player.healthEvent.GetStun += EnableStunImage;
            player.healthEvent.GetSilverArmor += EnableSilverArmorImage;
            player.healthEvent.GetDeath += EnableDeathImage;

            player.healthEvent.PoisonCured += DisablePoisonImage;
            player.healthEvent.AcidCured += DisableAcidImage;
            player.healthEvent.StunCured += DisableStunImage;
            player.healthEvent.ArmorWoreOff += DisableArmorImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned += EnablePoisonImage;
            enemy.healthEvent.GetAcid += EnableAcidImage;
            enemy.healthEvent.GetStun += EnableStunImage;
            enemy.healthEvent.GetSilverArmor += EnableSilverArmorImage;
            enemy.healthEvent.GetDeath += EnableDeathImage;

            enemy.healthEvent.PoisonCured += DisablePoisonImage;
            enemy.healthEvent.AcidCured += DisableAcidImage;
            enemy.healthEvent.StunCured += DisableStunImage;
            enemy.healthEvent.ArmorWoreOff += DisableArmorImage;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.healthEvent.GetPoisoned -= EnablePoisonImage;
            player.healthEvent.GetAcid -= EnableAcidImage;
            player.healthEvent.GetStun -= EnableStunImage;
            player.healthEvent.GetSilverArmor -= EnableSilverArmorImage;
            player.healthEvent.GetDeath -= EnableDeathImage;

            player.healthEvent.PoisonCured -= DisablePoisonImage;
            player.healthEvent.AcidCured -= DisableAcidImage;
            player.healthEvent.StunCured -= DisableStunImage;
            player.healthEvent.ArmorWoreOff -= DisableArmorImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned -= EnablePoisonImage;
            enemy.healthEvent.GetAcid -= EnableAcidImage;
            enemy.healthEvent.GetStun -= EnableStunImage;
            enemy.healthEvent.GetSilverArmor -= EnableSilverArmorImage;
            enemy.healthEvent.GetDeath -= EnableDeathImage;

            enemy.healthEvent.PoisonCured -= DisablePoisonImage;
            enemy.healthEvent.AcidCured -= DisableAcidImage;
            enemy.healthEvent.StunCured -= DisableStunImage;
            enemy.healthEvent.ArmorWoreOff -= DisableArmorImage;
        }
    }

    private void EnableSilverArmorImage(HealthEvent healthEvent)
    {
        silverArmorImage.SetActive(true);
    }

    private void EnablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(true);
    }

    private void EnableAcidImage(HealthEvent healthEvent)
    {
        acidImage.SetActive(true);
    }

    private void EnableStunImage(HealthEvent healthEvent)
    {
        stunImage.SetActive(true);
    }

    private void EnableDeathImage(HealthEvent @event)
    {
        deathImage.SetActive(true);
    }

    private void DisableArmorImage(HealthEvent healthEvent)
    {
        silverArmorImage.SetActive(false);
    }

    private void DisablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(false);
    }

    private void DisableAcidImage(HealthEvent healthEvent)
    {
        acidImage.SetActive(false);
    }

    private void DisableStunImage(HealthEvent healthEvent)
    {
        stunImage.SetActive(false);
    }
}
