using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class Idle : MonoBehaviour
{
    Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    public void StopVelocity()
    {
        // Ensure the rb collision detection is set to continuous
        rb2D.velocity = Vector2.zero;
    }
}
