using UnityEngine;

public class CircleOrigin : MonoBehaviour
{
    public float circleRadius;

    Player player;
    Weapon currentWeapon;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        circleRadius = player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.circleRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position;
        Gizmos.DrawWireSphere(position, circleRadius);
    }
}
