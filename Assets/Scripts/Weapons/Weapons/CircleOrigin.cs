using UnityEngine;

public class CircleOrigin : MonoBehaviour
{
    public float circleRadius;

    Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (GetComponentInParent<Projectile>() == null)
        {
            circleRadius = player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.circleRadius;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position;
        Gizmos.DrawWireSphere(position, circleRadius);
    }
}
