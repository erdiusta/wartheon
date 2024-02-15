using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip
    public MovementDetailsSO movementDetails;

    public Vector2 MovementInput { get; set; }
    public float moveSpeed;

    Rigidbody2D rb2D;
    Player player;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    Coroutine stunPlayerRoutine;

    Coroutine trailParticlesCoroutine;
    float trailParticlesDuration = 1f;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb2D = GetComponent<Rigidbody2D>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Check trail dust emittance based on move condition
        if (moveSpeed < 0.5f || (Mathf.Abs(MovementInput.x) < 0.1f && Mathf.Abs(MovementInput.y) < 0.1f))
        {
            StopTrailParticles();
        }
        else
        {
            if (trailParticlesCoroutine == null)
            {
                // If coroutine isn't running, start it to emit particles for a certain duration
                trailParticlesCoroutine = StartCoroutine(PlayTrailParticlesForDuration());
            }
        }

        // If stun coroutine is already running, do not start another one
        if (stunPlayerRoutine != null) return;

        // Second check if player is on stun status
        if (player.moveStatus == MoveStatus.Stun)
        {
            StopTrailParticles();
            stunPlayerRoutine = StartCoroutine(StunRoutine());
            return;
        }

        // Third check if enemy is on knockback status
        if (player.moveStatus == MoveStatus.Stagger)
        {
            rb2D.velocity = CalculateKnockback();
            return;
        }

        // If none of the above conditions are met, perform regular move
        rb2D.velocity = MovementInput * moveSpeed;
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    public void MoveRigidbody(Vector2 moveDirection, float moveSpeed)
    {
        // Ensure the rb collision detection is set to continuous
        rb2D.velocity = moveDirection * moveSpeed;
    }

    /// <summary>
    /// Stun routine
    /// </summary>
    IEnumerator StunRoutine()
    {
        moveSpeed = 0f;

        yield return new WaitForSeconds(3f);

        player.healthEvent.CallStunCuredEvent();
        player.animator.SetBool(Settings.isStunned, false);

        // Reset stun status and allow other stun coroutines to be started
        moveSpeed = movementDetails.GetMoveSpeed();
        player.moveStatus = MoveStatus.Idle;
        stunPlayerRoutine = null;
    }

    public void TriggerKnockback(Vector3 vector)
    {
        if (player.moveStatus == MoveStatus.Idle)
        {
            StartCoroutine(Stagger(vector));
        }
    }

    // Coroutine to play trail particles for a certain duration
    private IEnumerator PlayTrailParticlesForDuration()
    {
        PlayTrailParticles(); // Start trail particles emission
        yield return new WaitForSeconds(trailParticlesDuration); // Wait for specified duration

        StopTrailParticles(); // Stop trail particles emission
        trailParticlesCoroutine = null; // Reset coroutine reference
    }

    private void PlayTrailParticles()
    {
        // Calculate player direction
        Vector3 playerDirection = MovementInput;

        // Convert direction to rotation
        Quaternion rotation = Quaternion.LookRotation(playerDirection, Vector3.up);

        // Set start rotation of the particle system
        ParticleSystem.MainModule mainModule = player.particlesSystem.main;
        mainModule.startRotation = (rotation.eulerAngles.y * Mathf.Deg2Rad) - (90 * Mathf.Deg2Rad);

        player.particlesSystem.Play();
    }

    private void StopTrailParticles()
    {
        player.particlesSystem.Stop();
    }

    IEnumerator Stagger(Vector3 vector)
    {
        player.moveStatus = MoveStatus.Stagger;

        knockbackVector = vector;
        knockbackForce = player.knockback.knockbackForce;
        knockbackTimeWeight = player.knockback.knockbackTimeWeight;

        yield return new WaitForSeconds(knockbackTimeWeight);

        moveSpeed = movementDetails.GetMoveSpeed();
        player.moveStatus = MoveStatus.Idle;
    }

    private Vector2 CalculateKnockback()
    {
        if (knockbackTimeWeight > 0f)
        {
            knockbackTimeWeight -= Time.fixedDeltaTime;
        }

        return knockbackVector * knockbackForce * (knockbackTimeWeight > 0f ? knockbackTimeWeight : 0f);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
