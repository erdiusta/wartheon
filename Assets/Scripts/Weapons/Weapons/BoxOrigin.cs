using UnityEngine;


public class BoxOrigin : MonoBehaviour
{
    public float boxLength = 1f;
    public float boxHeight = 1f;

    [SerializeField] Transform weaponRotationPointTransform;

    Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (GetComponentInParent<Projectile>() == null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                boxLength = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.boxLength;
                boxHeight = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.boxHeight;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = this == null ? Vector3.zero : transform.position;

        // Convert angle value from degree to radians to use sin-cos methods
        float angle = weaponRotationPointTransform.eulerAngles.z * Mathf.Deg2Rad;

        // Use cos and sin to scale box dimensions smoothly
        float cosAngle = Mathf.Abs(Mathf.Cos(angle));
        float sinAngle = Mathf.Abs(Mathf.Sin(angle));

        // Adjust box size based on angle
        float adjustedLength = boxLength * cosAngle + boxHeight * sinAngle;
        float adjustedHeight = boxHeight * cosAngle + boxLength * sinAngle;

        Gizmos.DrawWireCube(position, new Vector3(adjustedLength, adjustedHeight, 0));
    }
}
