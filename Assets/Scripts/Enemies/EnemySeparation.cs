using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemySeparation : MonoBehaviour
{
    Enemy enemy;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemy.enemyAI.isDashing) return;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (enemy.enemyAI.isDashing) return;

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (enemy.enemyAI.isDashing) return;
    }
}
