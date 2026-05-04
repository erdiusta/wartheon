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
        // MP
        if (TryGetComponent(out EnemyNetwork enemyNetwork))
        {
            if (!enemyNetwork.Isboss)
            {
                enemyNetwork.GetComponent<Enemy>().aiRigidbody2D.canMove = false;
            }

            goto skipSPLogic;
        }

        // SP
        if (TryGetComponent(out Enemy enemy))
        {
            if (!enemy.Isboss)
            {
                enemy.aiRigidbody2D.canMove = false;
            }
        }

    skipSPLogic:

        // Ensure the rb collision detection is set to continuous
        rb2D.linearVelocity = Vector2.zero;
    }
}
