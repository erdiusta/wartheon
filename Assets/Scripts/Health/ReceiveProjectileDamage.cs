using UnityEngine;

public class ReceiveProjectileDamage : MonoBehaviour
{
    Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeProjectileDamage(int damage, DamageContext ctx)
    {
        HealthAuthorityResolver.GetAuthority(gameObject).ApplyDamage(damage, ctx);
    }
}
