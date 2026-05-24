using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CircleCollider2D))]
public class CircleOrigin : MonoBehaviour
{
    public float circleRadius;

    Player player;
    CircleCollider2D circleCollider;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

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

        if (player != null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }
        }

        // Runtime logic
        if (GetComponentInParent<Projectile>() == null && player != null && player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            circleRadius = currentWeaponDetails.circleRadius;

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
