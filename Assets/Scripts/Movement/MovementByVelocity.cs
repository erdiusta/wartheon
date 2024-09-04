using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
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

    Rigidbody2D rb2D;
    Player player;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    Coroutine stunPlayerRoutine;

    float marginDistance = 0f;
    Coroutine trailParticlesCoroutine;
    float trailDustDuration = 0.35f;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb2D = GetComponent<Rigidbody2D>();
        moveSpeed = movementDetails.GetMoveSpeed();
        dustTrailAnimator = dustTrailContainer.GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += NeutralizeStatus;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= NeutralizeStatus;
    }

    private void Start()
    {
        playerStartingSpeed = movementDetails.moveSpeed;
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

        // If stun coroutine is already running, do not start another one
        if (stunPlayerRoutine != null) return;

        // Second check if player is on stun status
        if (player.moveStatus == MoveStatus.Stun)
        {
            //StopTrailParticles();
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
    /// Get current move speed
    /// </summary>
    public int GetCurrentMoveSpeed()
    {
        return (int)moveSpeed;
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
            Debug.Log("Player's current armor value is " + player.health.currentArmorValue);
        }

        if (player.armorStatus == ArmorStatus.SilverArmor || player.armorStatus == ArmorStatus.GoldenArmor)
        {
            player.health.ResetArmorValue();
            player.healthEvent.CallArmorWoreOffEvent();
            Debug.Log("Player's current armor value is " + player.health.currentArmorValue);
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
        moveSpeed = movementDetails.GetMoveSpeed();
        player.moveStatus = MoveStatus.Idle;
        stunPlayerRoutine = null;
    }

    public void TriggerKnockback(Vector3 vector)
    {
        if (player.moveStatus == MoveStatus.Idle)
        {
            if (!player.isClone)
            {
                if (player.gameObject.activeSelf)
                {
                    StartCoroutine(Stagger(vector));
                }
            }
        }
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
        Vector3 playerDirection = (HelperUtilities.GetMouseWorldPosition() - transform.position).normalized;

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
