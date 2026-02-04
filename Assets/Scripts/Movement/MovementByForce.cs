using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByForce : PlayerBoundBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip
    public MovementDetailsSO movementDetails;
    public Transform dustTrailContainer;

    [HideInInspector] public float playerStartingMinSpeed;
    [HideInInspector] public float playerStartingSpeed;
    [HideInInspector] public Animator dustTrailAnimator;

    public Vector2 MovementInput { get; set; }
    public float moveSpeed;
    private Vector2 lastVelocity;

    Rigidbody2D rb2D;

    Coroutine knockbackPlayerRoutine;
    Coroutine stunPlayerRoutine;

    float marginDistance = 0f;
    Coroutine trailParticlesCoroutine;
    float trailDustDuration = 0.35f;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb2D = GetComponent<Rigidbody2D>();
        dustTrailAnimator = dustTrailContainer.GetComponentInChildren<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Unsubscribe();
    }

    protected override void HandlePlayerReady(Player player, PlayerDetailsSO details)
    {
        Subscribe();

        moveSpeed = movementDetails.GetBaseMaxMoveSpeed() + player.CurrentAgilityValue;

        player.animator.speed = player.movementByForce.moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Subscribe()
    {
        if (player == null) return;

        StaticEventHandler.OnRoomChanged += NeutralizeStatus;
    }

    private void Unsubscribe()
    {
        if (player == null) return;

        StaticEventHandler.OnRoomChanged -= NeutralizeStatus;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        StopOnMinVelocity();
        Move();
    }

    public void Move()
    {
        if (stunPlayerRoutine != null || player.moveStatus == MoveStatus.Stun)
        {
            if (stunPlayerRoutine == null) stunPlayerRoutine = StartCoroutine(StunRoutine());    
            return;
        }

        if (player.moveStatus == MoveStatus.KnockedBack) return;

        // Move Process
        MovementInput = MovementInput.normalized;

        if (MovementInput.magnitude > Mathf.Epsilon)
        {
            StopOnDirectionChange();
            rb2D.AddForce(MovementInput * moveSpeed, ForceMode2D.Force);

            rb2D.linearVelocity = Vector2.ClampMagnitude(rb2D.linearVelocity, moveSpeed);
        }
        else
        {
            rb2D.linearVelocity = Vector2.Lerp(rb2D.linearVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
        }

        lastVelocity = MovementInput;

        HandleTrialParticles();
    }

    private void HandleTrialParticles()
    {
        // Check trail dust emittance based on move condition
        if (moveSpeed < 0.5f || (Mathf.Abs(MovementInput.x) < 0.1f && Mathf.Abs(MovementInput.y) < 0.1f))
        {
            //StopTrailParticles();
        }
        else
        {
            if (player.playerControl.movementTimer > trailDustDuration)
            {
                if (trailParticlesCoroutine == null)
                {
                    // If coroutine isn't running, start it to emit particles for a certain duration
                    trailParticlesCoroutine = StartCoroutine(PlayTrailParticlesForDuration());
                }

                player.playerControl.movementTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Makes the player stop moving in a certain direction if there is no input for that direction and their speed is less than the minimum in that direction
    /// </summary>
    private void StopOnMinVelocity()
    {
        if (Mathf.Abs(lastVelocity.x) < 0.1f)
        {
            if (Mathf.Abs(rb2D.linearVelocity.x) < movementDetails.baseMinMoveSpeed)
            {
                if (rb2D.bodyType == RigidbodyType2D.Dynamic)
                    rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);

            }
        }

        if (Mathf.Abs(lastVelocity.y) < 0.1f)
        {
            if (Mathf.Abs(rb2D.linearVelocity.y) < movementDetails.baseMinMoveSpeed)
            {
                if (rb2D.bodyType == RigidbodyType2D.Dynamic)
                    rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0);
            }

        }
    }

    private void StopOnDirectionChange()
    {
        if (lastVelocity.x > 0.1f && MovementInput.x < -0.1f)  //if direction changed
        {
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        }
        else if (lastVelocity.x < -0.1f && MovementInput.x > 0.1f)
        {
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        }

        if (lastVelocity.y > 0.1f && MovementInput.y < -0.1f) //if direction changed
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0);
        }
        else if (lastVelocity.y < -0.1f && MovementInput.y > 0.1f)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0);
        }
    }

    /// <summary>
    /// Neutralize Status on room changed
    /// </summary>
    private void NeutralizeStatus(RoomChangedEventArgs roomChangedEventArgs)
    {
        // MOVE STATUS CHECKS
        player.moveStatus = MoveStatus.Idle;

        // ARMOR STATUS CHECKS
        if (player.armorStatus == ArmorStatus.Acid)
        {
            player.health.ResetArmorValue();
            player.healthEvent.CallAcidCuredEvent();
        }

        player.armorStatus = ArmorStatus.Normal;
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
        moveSpeed = movementDetails.GetBaseMaxMoveSpeed() + player.CurrentAgilityValue * 0.25f;
        player.moveStatus = MoveStatus.Idle;
        stunPlayerRoutine = null;
    }

    public void ApplyKnockback(Vector2 direction, float force, float attackerMass)
    {
        // Don’t stack knockback
        if ((player.moveStatus & MoveStatus.KnockedBack) != 0) return;

        player.rb2D.linearVelocity = Vector2.zero; // Reset previous move velocity

        float scaledForce = force * (attackerMass / rb2D.mass); // scale based on mass ratio
        rb2D.AddForce(direction.normalized * scaledForce, ForceMode2D.Impulse);
        player.moveStatus |= MoveStatus.KnockedBack;

        if (knockbackPlayerRoutine != null) StopCoroutine(knockbackPlayerRoutine);

        knockbackPlayerRoutine = StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        float duration = 0.4f;
        float timer = 0f;

        while (timer < duration)
        {
            // Exit early if the velocity is almost zero (player stopped)
            if (rb2D.linearVelocity.magnitude < 0.05f) break;

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb2D.linearVelocity = Vector2.zero;
        player.moveStatus &= ~MoveStatus.KnockedBack;
        knockbackPlayerRoutine = null;
    }

    // Coroutine to play trail particles for a certain duration
    private IEnumerator PlayTrailParticlesForDuration()
    {
        EmitDustTrail(); // Start trail particles emission
        yield return new WaitForSeconds(trailDustDuration); // Wait for specified duration

        trailParticlesCoroutine = null; // Reset coroutine reference
    }

    private void EmitDustTrail()
    {
        if (dustTrailAnimator == null)
        {
            GameObject dustTrailObject = Instantiate(GameResources.Instance.dustTrailPrefab, dustTrailContainer);
            dustTrailAnimator = dustTrailObject.GetComponent<Animator>();

            if (dustTrailAnimator.transform.parent == null)
            {
                {
                    dustTrailAnimator.transform.SetParent(dustTrailContainer);
                }
            }
        }
        else
        {
            if (dustTrailAnimator.transform.parent == null)
            {
                {
                    GameObject dustTrailObject = Instantiate(GameResources.Instance.dustTrailPrefab, dustTrailContainer);
                    dustTrailAnimator = dustTrailObject.GetComponent<Animator>();

                    dustTrailAnimator.transform.SetParent(dustTrailContainer);
                }
            }
        }

        // Calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (HelperUtilities.GetMouseWorldPosition(player) - transform.position).normalized;

        // Get player to cursor angle
        float playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Determine the localScale based on the angle
        Vector3 newLocalScale = dustTrailContainer.localScale;

        // Adjust x-scale based on angle (flip on x-axis)
        newLocalScale.x = Mathf.Cos(playerAngleDegrees * Mathf.Deg2Rad) >= 0 ? 1 : -1;

        // Apply the new localScale to the DustTrailContainer
        dustTrailContainer.localScale = newLocalScale;

        // Adjust the position of the DustTrailContainer with margin distance
        dustTrailContainer.localPosition = playerDirection * marginDistance * -1;

        dustTrailAnimator.SetTrigger("trail");

        dustTrailAnimator.transform.SetParent(null);
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
