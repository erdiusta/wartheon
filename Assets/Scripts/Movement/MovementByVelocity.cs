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
        if (player.moveStatus == MoveStatus.Stun)
        {
            StartCoroutine(StunRoutine());
        }
        else if (player.moveStatus == MoveStatus.Stagger)
        {
            rb2D.velocity = CalculateKnockback();
        }
        else
        {
            rb2D.velocity = MovementInput * moveSpeed;
        }
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
        moveSpeed = movementDetails.GetMoveSpeed();
        player.moveStatus = MoveStatus.Idle;
    }

    public void TriggerKnockback(Vector3 vector)
    {
        if (player.moveStatus == MoveStatus.Idle)
        {
            StartCoroutine(Stagger(vector));
        }
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
