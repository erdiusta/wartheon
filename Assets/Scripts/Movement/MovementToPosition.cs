using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementToPosition : MonoBehaviour
{
    Rigidbody2D rb2D;
    Enemy enemy;

    private void Awake()
    {
        // Load components
        rb2D = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
    }

    /// <summary>
    /// Move the rigidbody component - Chase
    /// </summary>
    public void ChaseMoveRigidbodyByPosition(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);
        rb2D.velocity = unitVector * moveSpeed;
    }

    /// <summary>
    /// Move the rigidbody component - Patrol
    /// </summary>
    public void PatrolMoveRigidbodyByPosition(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(unitVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
        enemy.animateEnemy.InitializeAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
        rb2D.velocity = unitVector * moveSpeed;
    }

    /// <summary>
    /// Move the rigidbody component - Attack Move
    /// </summary>
    public void AttackMoveRigidbodyByPosition(Vector3 unitVector, float moveSpeed)
    {
        Vector2 unitVector2D = new Vector2(unitVector.x, unitVector.y);

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(unitVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
        enemy.animateEnemy.InitializeAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
        enemy.animateEnemy.SetAttackAnimationParameters();

        rb2D.MovePosition(rb2D.position + (unitVector2D * moveSpeed * Time.fixedDeltaTime));
        rb2D.velocity = unitVector * moveSpeed;
    }
}
