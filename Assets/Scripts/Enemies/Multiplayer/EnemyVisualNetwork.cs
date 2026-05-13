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

    //private void InitEmemyVisual_Client()
    //{
    //    if (enemy == null || health == null) return;

    //    health.spriteRenderer = enemy.spriteRendererArray[0];

    //    if (enemyNetwork.isImmuneAfterHit)
    //    {
    //        health.isImmuneAfterHit = true;
    //        health.immunityTime = enemyNetwork.hitImmunityTime;
    //    }
    //}

    [Server]
    public void ServerInitMaterialize(EnemyDetailsSO details)
    {
        materializeDuration = details.enemyMaterializeTime;
        materalizeColor = details.enemyMaterializeColor;
        isMaterializing = true;
        enemy.health.isDamageable = false;
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

        EnableEnemy(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(materalizeColor, materializeDuration, enemy.spriteRendererArray));

        if (isServer)
        {
            enemy.initializationCompleted = true;
            isMaterializing = false;
            enemy.health.isDamageable = true;
        }

        EnableEnemy(true);
    }

    private void EnableEnemy(bool isEnabled)
    {
        if (enemyNetwork != null) enemyNetwork.enabled = isEnabled;

        if (enemy.aiRigidbody2D != null && enemyCombatData != null && !enemyCombatData.Isboss) enemy.aiRigidbody2D.enabled = isEnabled;
    }
}
