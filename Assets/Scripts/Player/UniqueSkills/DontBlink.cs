using UnityEngine;

public class DontBlink : MonoBehaviour
{
    public float circleRadius = 5f;
    public Color gizmoColor = new Color(1f, 0.2f, 0.2f, 0.25f);

    /// <summary>
    /// Returns true if a position is within blink range.
    /// </summary>
    public bool IsWithinRange(Vector2 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition) <= circleRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, circleRadius);
    }
}
