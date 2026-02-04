using UnityEngine;

public static class EnemyCombatDataResolver
{
    public static IEnemyCombatData Resolve(GameObject enemyGO)
    {
        // MP has priority
        if (enemyGO.TryGetComponent(out EnemyNetwork enemyNetwork)) return enemyNetwork;

        // SP fallback
        if (enemyGO.TryGetComponent(out Enemy enemy)) return enemy;

        return null;
    }
}
