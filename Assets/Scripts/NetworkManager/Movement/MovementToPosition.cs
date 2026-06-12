using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementToPosition : MonoBehaviour
{
    Rigidbody2D rb2D;
    Enemy enemy;
    IEnemyMovementData enemyMovementData;
    MovementToPositionEvent movementToPositionEvent;

    Coroutine knockbackPlayerRoutine;

    private void Awake()
    {
        // Load components
        rb2D = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        enemyMovementData = EnemyDataResolver.Resolve<IEnemyMovementData>(gameObject);
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    private void OnEnable()
    {
        if (enemy == null)
        {
            movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;
        }
    }

    private void OnDisable()
    {
        if (enemy == null)
        {
            movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;
        }
    }

    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        MoveRigidbody(movementToPositionArgs.movePosition, movementToPositionArgs.currentPosition, movementToPositionArgs.moveSpeed);
    }

    public void ApplyKnockbackToEnemy(Vector2 direction, float force, float attackerMass)
    {
        // Don’t stack knockback
        if ((enemy.moveStatus & MoveStatus.KnockedBack) != 0) return;

        enemy.idle.StopVelocity(); // Reset previous move velocity

        float scaledForce = force * (attackerMass / rb2D.mass); // scale based on mass ratio
        rb2D.AddForce(direction.normalized * scaledForce, ForceMode2D.Impulse);
        enemy.moveStatus |= MoveStatus.KnockedBack;

        if (knockbackPlayerRoutine == null) knockbackPlayerRoutine = StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        float duration = 0.4f;
        float timer = 0f;

        while (timer < duration)
        {
            // Exit early if the velocity is almost zero (player stopped)
            if (rb2D.linearVelocity.magnitude < 0.08f) break;

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (TryGetComponent(out EnemyAINetwork enemyAINetwork)) enemyAINetwork.ResetEnemySpeed();
        else enemy.enemyAI.ResetEnemySpeed();

        rb2D.linearVelocity = Vector2.zero;
        enemy.moveStatus &= ~MoveStatus.KnockedBack;
        knockbackPlayerRoutine = null;
    }

    /// <summary>
    /// Move the rigidbody component - PLAYER FOR ROLL
    /// </summary>
    private void MoveRigidbody(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);
        rb2D.MovePosition(rb2D.position + (unitVector * moveSpeed * Time.fixedDeltaTime));
    }

}
