using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [HideInInspector] public Transform target;

    private void OnEnable()
    {
        target = GetComponentInParent<Player>().transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;
    }
}
