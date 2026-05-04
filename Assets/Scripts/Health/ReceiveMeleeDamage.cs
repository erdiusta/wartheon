using UnityEngine;

public class ReceiveMeleeDamage : MonoBehaviour
{
    public void TakeMeleeDamage(int damage, DamageContext ctx)
    {
        HealthAuthorityResolver.GetAuthority(gameObject).ApplyDamage(damage, ctx);
    }
}
