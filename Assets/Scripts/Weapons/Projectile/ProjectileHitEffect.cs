using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileHitEffect : MonoBehaviour
{
    public Gradient colorGradient;
    public Sprite sprite;

    [HideInInspector] public float duration = 0f;
    [HideInInspector] public float startParticleSize = 0f;
    [HideInInspector] public float startParticleSpeed = 0f;
    [HideInInspector] public float startLifeTime = 0f;
    [HideInInspector] public float effectGravity = 0f;
    [HideInInspector] public int maxParticleNumber = 1;
    [HideInInspector] public int emissionRate = 0;
    [HideInInspector] public int burstParticleNumber = 1;
    [HideInInspector] public Vector3 velocityOverLifetimeMin;
    [HideInInspector] public Vector3 velocityOverLifetimeMax;

    ParticleSystem projectileHitEffectParticleSystem;
    ProjectileDetailsSO projectileDetails;

    private void Awake()
    {
        projectileHitEffectParticleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Set Projectile Hit Effect from passed in ProjectileHitEffectSO details
    /// </summary>
    public void SetHitEffect(float duration, float startParticleSize, float startParticleSpeed, float startLifeTime, float effectGravity, int maxParticleNumber, int emissionRate, int burstParticleNumber,
        Vector3 velocityOverLifetimeMin, Vector3 velocityOverLifetimeMax)
    {
        // Stop First
        projectileHitEffectParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Set hit effect color gradient
        SetHitEffectColorGradient(colorGradient);

        // Set hit effect particle system starting values
        SetHitEffectParticleStartingValues(duration, startParticleSize, startParticleSpeed,
            startLifeTime, effectGravity, maxParticleNumber);

        // Set hit effect particle system particle burst particle number
        SetHitEffectParticleEmission(emissionRate, burstParticleNumber);

        // Set hit effect particle sprite
        SetHitEffectParticleSprite(sprite);

        // Set hit effect lifetime min and max velocities
        SetHitEffectVelocityOverLifeTime(velocityOverLifetimeMin, velocityOverLifetimeMax);

        StartCoroutine(DestroyProcess());
    }

    /// <summary>
    /// Set the hit effect particle system color gradient
    /// </summary>
    private void SetHitEffectColorGradient(Gradient gradient)
    {
        // Set colour gradient
        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = projectileHitEffectParticleSystem.colorOverLifetime;
        colorOverLifetimeModule.color = gradient;
    }

    /// <summary>
    /// Set hit effect particle system starting values
    /// </summary>
    private void SetHitEffectParticleStartingValues(float duration, float startParticleSize, float startParticleSpeed, float startLifetime, 
        float effectGravity, int maxParticles)
    {
        ParticleSystem.MainModule mainModule = projectileHitEffectParticleSystem.main;

        // Set particle system duration
        mainModule.duration = duration;

        // Set particle start size
        mainModule.startSize = startParticleSize;

        // Set particle start speed
        mainModule.startSpeed = startParticleSpeed;

        // Set particle start lifetime
        mainModule.startLifetime = startLifetime;

        // Set particle starting gravity
        mainModule.gravityModifier = effectGravity;

        // Set max particles
        mainModule.maxParticles = maxParticles;
    }

    /// <summary>
    /// Set hit effect particle system particle burst particle number
    /// </summary>
    private void SetHitEffectParticleEmission(int emissionRate, float burstParticleNumber)
    {
        ParticleSystem.EmissionModule emissionModule = projectileHitEffectParticleSystem.emission;

        // Set particle burst number
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstParticleNumber);
        emissionModule.SetBurst(0, burst);

        // Set particle emission rate
        emissionModule.rateOverTime = emissionRate;
    }

    /// <summary>
    /// Set hit effect particle system sprite
    /// </summary>
    private void SetHitEffectParticleSprite(Sprite sprite)
    {
        // Set particle burst number
        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = projectileHitEffectParticleSystem.textureSheetAnimation;

        textureSheetAnimationModule.SetSprite(0, sprite);
    }

    /// <summary>
    /// Set the hit effect velocity over lifetime
    /// </summary>
    private void SetHitEffectVelocityOverLifeTime(Vector3 minVelocity, Vector3 maxVelocity)
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = projectileHitEffectParticleSystem.velocityOverLifetime;

        // Define min max X velocity
        ParticleSystem.MinMaxCurve minMaxCurveX = new ParticleSystem.MinMaxCurve();
        minMaxCurveX.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveX.constantMin = minVelocity.x;
        minMaxCurveX.constantMax = maxVelocity.x;
        velocityOverLifetimeModule.x = minMaxCurveX;

        // Define min max Y velocity
        ParticleSystem.MinMaxCurve minMaxCurveY = new ParticleSystem.MinMaxCurve();
        minMaxCurveY.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveY.constantMin = minVelocity.y;
        minMaxCurveY.constantMax = maxVelocity.y;
        velocityOverLifetimeModule.y = minMaxCurveY;

        // Define min max Z velocity
        ParticleSystem.MinMaxCurve minMaxCurveZ = new ParticleSystem.MinMaxCurve();
        minMaxCurveZ.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveZ.constantMin = minVelocity.z;
        minMaxCurveZ.constantMax = maxVelocity.z;
        velocityOverLifetimeModule.z = minMaxCurveZ;
    }

    IEnumerator DestroyProcess()
    {
        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);
    }
}
