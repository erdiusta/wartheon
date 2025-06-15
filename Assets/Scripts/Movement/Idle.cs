using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class Idle : MonoBehaviour
{
    Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    public void StopVelocity()
    {
        if (TryGetComponent<Enemy>(out Enemy enemy))
        {
            if (!enemy.enemyDetails.isEnemyBoss)
            {
                enemy.aiRigidbody2D.canMove = false;
            }
        }

        // Ensure the rb collision detection is set to continuous
        rb2D.linearVelocity = Vector2.zero;
    }
}
