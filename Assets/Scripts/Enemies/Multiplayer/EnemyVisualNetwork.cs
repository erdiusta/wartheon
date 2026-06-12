using Mirror;
using System.Collections;
using UnityEngine;

public class EnemyVisualNetwork : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnMaterializeChanged))]
    bool isMaterializing;

    [SyncVar] float materializeDuration;

    [SyncVar] Color materalizeColor;

    Enemy enemy;
    Health health;
    EnemyNetwork enemyNetwork;
    IEnemyCombatData enemyCombatData;
    MaterializeEffect materializeEffect;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        health = GetComponent<Health>();
        enemyNetwork = GetComponent<EnemyNetwork>();
        enemyCombatData = GetComponent<IEnemyCombatData>();
        materializeEffect = GetComponent<MaterializeEffect>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        enemy.rb2D.simulated = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(!isServer) enemy.rb2D.simulated = false;
    }

    [Server]
    public void ServerInitMaterialize(EnemyDetailsSO details)
    {
        materializeDuration = details.enemyMaterializeTime;
        materalizeColor = details.enemyMaterializeColor;
        isMaterializing = true;

        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(enemy.gameObject);
        healthAuthority.IsDamageable = false;
        enemy.GetComponent<PolygonCollider2D>().enabled = false;
        EnableEnemy(false);


        StartCoroutine(ServerMaterializeRoutine());
    }

    [Server]
    IEnumerator ServerMaterializeRoutine()
    {
        yield return new WaitForSeconds(materializeDuration);

        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(enemy.gameObject);
        healthAuthority.IsDamageable = true;

        enemy.GetComponent<PolygonCollider2D>().enabled = true;

        isMaterializing = false;
        enemy.initializationCompleted = true;
        EnableEnemy(true);
    }

    private void OnMaterializeChanged(bool oldVal, bool newVal)
    {
        if (!newVal) return;
        if (!isClient) return;

        StartCoroutine(MaterializeSequence());
    }

    IEnumerator MaterializeSequence()
    {
        yield return null;

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(materalizeColor, materializeDuration, enemy.spriteRendererArray));
    }

    private void EnableEnemy(bool isEnabled)
    {
        if (enemyNetwork != null) enemyNetwork.enabled = isEnabled;

        if (enemy.aiRigidbody2D != null && enemyCombatData != null && !enemyCombatData.Isboss) enemy.aiRigidbody2D.enabled = isEnabled;
    }
}
