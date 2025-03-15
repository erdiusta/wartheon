using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxOrigin : MonoBehaviour
{
    public float boxLength = 1f;
    public float boxHeight = 1f;

    [SerializeField] private Transform weaponRotationPointTransform;

    private Player player;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true; // Ensure it's used for detection, not physics collision
    }

    private void Update()
    {
        if (GetComponentInParent<Projectile>() == null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                boxLength = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.boxLength;
                boxHeight = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.boxHeight;

                // Update collider size if needed
                Vector2 newSize = new Vector2(boxLength, boxHeight);
                if (boxCollider.size != newSize)
                {
                    boxCollider.size = newSize;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform ? transform.position : Vector3.zero;

        // Convert angle value from degrees to radians
        float angle = weaponRotationPointTransform.eulerAngles.z * Mathf.Deg2Rad;

        // Adjust box size based on rotation
        float cosAngle = Mathf.Abs(Mathf.Cos(angle));
        float sinAngle = Mathf.Abs(Mathf.Sin(angle));
        float adjustedLength = boxLength * cosAngle + boxHeight * sinAngle;
        float adjustedHeight = boxHeight * cosAngle + boxLength * sinAngle;

        Gizmos.DrawWireCube(position, new Vector3(adjustedLength, adjustedHeight, 0));
    }
}
