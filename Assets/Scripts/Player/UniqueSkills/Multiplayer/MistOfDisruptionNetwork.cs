using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MistOfDisruptionNetwork : NetworkBehaviour
{
    public UnityEvent OnMistOfDisruptionActivated;
    public UnityEvent OnMistOfDisruptionDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isMistEnabled;

    Animator anim;
    Player owner;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnMistOfDisruptionActivated.AddListener(EnableMist);
        OnMistOfDisruptionDeactivated.AddListener(DisableMist);
    }

    private void OnDisable()
    {
        OnMistOfDisruptionActivated.RemoveListener(EnableMist);
        OnMistOfDisruptionDeactivated.RemoveListener(DisableMist);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isMistEnabled = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("mistOfDisruption", !undo);
    }

    public void ActivateMistOfDisruptionEvent() => OnMistOfDisruptionActivated?.Invoke();
    public void DeactivateMistOfDisruptionEvent() => OnMistOfDisruptionDeactivated?.Invoke();

    // Local Handlers
    public void EnableMist()
    {
        isMistEnabled = true;
    }

    public void DisableMist()
    {
        isMistEnabled = false;
    }

    IEnumerator Lifetime(float duration, int slotIndex, uint ownerNetId)
    {
        yield return new WaitForSeconds(duration);

        RpcPlayAnimation(undo: true);

        owner.NetAuth.TargetMistOfDisruptionFinished(owner.NetAuth.connectionToClient, slotIndex, ownerNetId);

        yield return new WaitForSeconds(0.5f); // animation fade

        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isServer) return;
        if (!isMistEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null && affectedPlayer.NetAuth.netId == owner.NetAuth.netId && !owner.isInMistOfDisruption)
        {
            owner.isInMistOfDisruption = true;
        }

        if (affectedEnemy != null)
        {
            affectedEnemy.isDisoriented = isMistEnabled;
            affectedEnemy.disorientDuration = 5f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isServer) return;

        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null && affectedPlayer.NetAuth.netId == owner.NetAuth.netId)
        {
            owner.isInMistOfDisruption = false;
        }
    }
}
