using UnityEngine;

[ExecuteAlways]
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

        UpdateColliderShape();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateColliderShape(); // Editor live preview
            return;
        }

        // Runtime logic
        if (GetComponentInParent<Projectile>() == null && player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            circleRadius = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.circleRadius;

            UpdateColliderShape();
        }
    }

    private void UpdateColliderShape()
    {
        // Sync the collider radius
        if (circleCollider.radius != circleRadius)
        {
            circleCollider.radius = circleRadius;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();

            UpdateColliderShape();
        }
    }
}
