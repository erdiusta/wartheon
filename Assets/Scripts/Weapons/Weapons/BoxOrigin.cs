using UnityEngine;

public class BoxOrigin : MonoBehaviour
{
    public float boxLength = 1f;
    public float boxHeight = 1f;

    Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (GetComponentInParent<Projectile>() == null)
        {
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = this == null ? Vector3.zero : transform.position;

        Gizmos.DrawWireCube(position, new Vector3(boxLength, boxHeight, 0));

    }
}
