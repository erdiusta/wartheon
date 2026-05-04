using Mirror;
using UnityEngine;

public class ProjectileAnimSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnProjectileFired))]
    public bool isFired;

    [SyncVar(hook = nameof(OnProjectileImpacted))]
    public bool isImpacted;

    Animator projectileAnimator;

    public override void OnStartClient()
    {
        base.OnStartClient();

        projectileAnimator = GetComponent<Animator>();
    }

    private void OnProjectileFired(bool oldValue, bool newValue)
    {

    }

    private void OnProjectileImpacted(bool oldValue, bool newValue)
    {
        if(projectileAnimator != null) GetComponent<Animator>().SetTrigger("impact");
    }


    [Server]
    public void UpdateProjectileFire(bool firing)
    {
        isFired = firing;
    }

    [Server]
    public void UpdateProjectileImpact(bool impact)
    {
        isImpacted = impact;
    }
}
