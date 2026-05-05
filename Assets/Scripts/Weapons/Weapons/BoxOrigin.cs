using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public class BoxOrigin : MonoBehaviour
{
    public float boxLength = 1f;
    public float boxHeight = 1f;
    public float directionAngle = 0f;
    public bool isMainHand;

    private Player player;
    private PolygonCollider2D polygonCollider;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        UpdateColliderShape();
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon += SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
    }

    private void SetActiveWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        // Runtime logic
        if (GetComponentInParent<Projectile>() == null && player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (isMainHand)
            {
                WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);

                boxLength = weaponDetails.boxLength;
                boxHeight = weaponDetails.boxHeight;
            }
        }
    }

    private void SetActiveWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        // Runtime logic
        if (GetComponentInParent<Projectile>() == null && player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (player.activeWeapon.GetCurrentOffHandWeapon() != null && !isMainHand)
            {
                WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.weaponTitle);

                boxLength = weaponDetails.boxLength;
                boxHeight = weaponDetails.boxHeight;
            }
        }
    }

    private void Update()
    {
        UpdateColliderShape();
        return;
    }

    private void UpdateColliderShape()
    {
        float angleRad = directionAngle * Mathf.Deg2Rad;

        // Local corner points of the box, centered at origin
        Vector2 topLeft = new Vector2(-boxLength / 2, boxHeight / 2);
        Vector2 topRight = new Vector2(boxLength / 2, boxHeight / 2);
        Vector2 bottomRight = new Vector2(boxLength / 2, -boxHeight / 2);
        Vector2 bottomLeft = new Vector2(-boxLength / 2, -boxHeight / 2);

        // Rotate each point manually
        Vector2[] points = new Vector2[]
        {
            RotatePoint(topLeft, angleRad),
            RotatePoint(topRight, angleRad),
            RotatePoint(bottomRight, angleRad),
            RotatePoint(bottomLeft, angleRad)
        };

        polygonCollider.SetPath(0, points);
    }

    private Vector3 RotatePoint(Vector2 point, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector3(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos,
            0f
        );
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
