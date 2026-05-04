using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyMovementNetwork : NetworkBehaviour
{
    MovementToPosition movement;
    Rigidbody2D rb2D;

    private void Awake()
    {
        movement = GetComponent<MovementToPosition>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        rb2D.simulated = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isServer) rb2D.simulated = false;
    }

    [Server]
    public void ServerApplyKnockback(Vector2 dir, float force, float attackerMass)
    {
        movement.ApplyKnockbackToEnemy(dir, force, attackerMass);
    }
}
