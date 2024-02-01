using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementToPosition : MonoBehaviour
{
    Rigidbody2D rb2D;

    private void Awake()
    {
        // Load components
        rb2D = GetComponent<Rigidbody2D>();
    }

    // On movement event
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs
        movementToPositionArgs)
    {
        MoveRigidbodyByPosition(movementToPositionArgs.movePosition, movementToPositionArgs.currentPosition, movementToPositionArgs.moveSpeed);
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    public void MoveRigidbodyByPosition(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);

        //rb2D.MovePosition(rb2D.position + (unitVector * moveSpeed * Time.fixedDeltaTime));
        rb2D.velocity = unitVector * moveSpeed;
    }
}
