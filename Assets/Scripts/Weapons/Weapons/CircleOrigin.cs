using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CircleOrigin : MonoBehaviour
{
    public float circleRadius;

    Player player;
    CircleCollider2D circleCollider;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true; // Ensure it's used for detection, not physics collision
    }

    private void Update()
    {
        if (GetComponentInParent<Projectile>() == null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                circleRadius = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.circleRadius;

                // Sync the collider radius
                if (circleCollider.radius != circleRadius)
                {
                    circleCollider.radius = circleRadius;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position;
        Gizmos.DrawWireSphere(position, circleRadius);
    }
}
