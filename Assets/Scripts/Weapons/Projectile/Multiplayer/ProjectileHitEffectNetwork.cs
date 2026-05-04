using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class ProjectileHitEffectNetwork : NetworkBehaviour
{
    ProjectileHitEffect projectileHitFx;

    private void Awake()
    {
        projectileHitFx = GetComponent<ProjectileHitEffect>();
    }

    [ClientRpc]
    public void RpcInitializeProjectileFX(float duration, float startParticleSize, float startParticleSpeed, float startLifeTime, float effectGravity, int maxParticleNumber, int emissionRate, int burstParticleNumber,
        Vector3 velocityOverLifetimeMin, Vector3 velocityOverLifetimeMax, int projectileIndex)
    {
        // Host already initialized
        if (isServer) return;

        projectileHitFx.SetHitEffect(duration, startParticleSize, startParticleSpeed, startLifeTime, effectGravity, maxParticleNumber, emissionRate, burstParticleNumber, 
            velocityOverLifetimeMin, velocityOverLifetimeMax);
    }
}
