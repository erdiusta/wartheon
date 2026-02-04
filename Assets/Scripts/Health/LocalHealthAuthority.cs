using UnityEngine;

public class LocalHealthAuthority: MonoBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void ApplyDamage(int amount, DamageContext ctx, GameObject target = null)
    {
        health.ApplyDamageInternal(amount, ctx);
        health.SyncHealthVisuals(amount, ctx);
    }
}
