using UnityEngine;

public static class EnemyDataResolver
{
    public static T Resolve<T>(GameObject enemyGO) where T: class
    {
        // MP has priority
        if (enemyGO.TryGetComponent(out EnemyNetwork enemyNetwork) && enemyNetwork is T netData) return netData;

        // SP fallback
        if (enemyGO.TryGetComponent(out Enemy enemy) && enemy is T spData) return spData;

        return null;
    }
}
