using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public class TriangleOrigin : MonoBehaviour
{
    public float coneLength = 1f;
    public float coneAngle = 45f;
    public float directionAngle = 0f;

    private Player player;
    private PolygonCollider2D polygonCollider;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        polygonCollider = GetComponent<PolygonCollider2D>();
         
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
            WeaponDetailsSO weapon = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails;
            coneLength = weapon.coneLength;
            coneAngle = weapon.coneAngle;

            UpdateColliderShape();
        }
    }

    private void UpdateColliderShape()
    {
        float halfAngle = coneAngle * 0.5f * Mathf.Deg2Rad;
        float rotationRad = directionAngle * Mathf.Deg2Rad;

        Vector2 pointA = Vector2.zero;

        Vector2 localB = new Vector2(Mathf.Sin(-halfAngle), -Mathf.Cos(-halfAngle)) * coneLength;
        Vector2 localC = new Vector2(Mathf.Sin(halfAngle), -Mathf.Cos(halfAngle)) * coneLength;

        Vector2 pointB = RotatePoint(localB, rotationRad);
        Vector2 pointC = RotatePoint(localC, rotationRad);

        polygonCollider.SetPath(0, new Vector2[] { pointA, pointB, pointC });
    }

    private Vector2 RotatePoint(Vector2 point, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (polygonCollider == null) polygonCollider = GetComponent<PolygonCollider2D>();

            UpdateColliderShape();
        }
    }
}
