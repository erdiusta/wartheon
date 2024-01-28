using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip
    [SerializeField] MovementDetailsSO movementDetails;

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
        rb2D.velocity = MovementInput * moveSpeed + CalculateKnockback();
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    public void MoveRigidbody(Vector2 moveDirection, float moveSpeed)
    {
        // Ensure the rb collision detection is set to continuous
        rb2D.velocity = moveDirection * moveSpeed;
    }

    public void Knockback(Vector3 vector, float force, float timeWeight)
    {
        player.playerStatus = Status.Stagger;
        knockbackVector = vector;
        knockbackForce = force;
        knockbackTimeWeight = timeWeight;
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
