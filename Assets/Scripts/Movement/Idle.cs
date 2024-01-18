using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IdleEvent))]
[DisallowMultipleComponent]
public class Idle : MonoBehaviour
{
    Rigidbody2D rb2D;
    IdleEvent idleEvent;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        idleEvent = GetComponent<IdleEvent>();
    }

    private void OnEnable()
    {
        idleEvent.OnIdle += IdleEvent_OnIdle;
    }

    private void OnDisable()
    {
        idleEvent.OnIdle -= IdleEvent_OnIdle;
    }

    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        MoveRigidbody();
    }

    /// <summary>
    /// Move the rigidbody component
    /// </summary>
    private void MoveRigidbody()
    {
        // Ensure the rb collision detection is set to continuous
        rb2D.velocity = Vector2.zero;
    }
}
