using UnityEngine;

public class EnemySeparation : MonoBehaviour
{
    Enemy enemy;

    public float separationRadius = 0.6f; // Adjust based on enemy size
    public float separationStrength = 5f; // Strength of push force

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        if (!enemy.isDead)
        {
            SeparateFromOthers();
        }
    }

    void SeparateFromOthers()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationRadius, LayerMask.GetMask("Enemy"));

        Vector2 separationForce = Vector2.zero;
        int count = 0;

        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy.gameObject == gameObject) continue; // Ignore self

            Vector2 directionAway = (Vector2)(transform.position - enemy.transform.position).normalized;
            separationForce += directionAway;
            count++;
        }

        if (count > 0)
        {
            separationForce /= count; // Average out the force

            if (enemy.isMaterializing || enemy.isDead) return;

            transform.position += (Vector3)(separationForce * separationStrength * Time.deltaTime);
        }
    }
}
