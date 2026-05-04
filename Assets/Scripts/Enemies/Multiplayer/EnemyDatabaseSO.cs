using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDatabase_", menuName = "Scriptable Objects/Dungeon/Enemy Database")]
public class EnemyDatabaseSO : ScriptableObject
{
    [SerializeField] List<EnemyDetailsSO> enemies;

    Dictionary<EnemyCategory, EnemyDetailsSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<EnemyCategory, EnemyDetailsSO>();

        foreach (EnemyDetailsSO enemy in enemies)
        {
            if (lookup.ContainsKey(enemy.enemyCategory))
            {
                Debug.LogError($"Duplicate EnemyCategory in database: {enemy.enemyCategory}");
                continue;
            }

            lookup.Add(enemy.enemyCategory, enemy);
        }
    }

    public EnemyDetailsSO GetEnemy(EnemyCategory category)
    {
        if (lookup == null)
        {
            Debug.LogError("EnemyDatabaseSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(category, out EnemyDetailsSO enemy))
        {
            Debug.LogError($"EnemyCategory not found in database {category}");
            return null;
        }

        return enemy;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var categories = new HashSet<EnemyCategory>();

        foreach (var enemy in enemies)
        {
            if (!categories.Add(enemy.enemyCategory))
            {
                Debug.LogError($"Duplicate enemy category: {enemy.enemyCategory}", enemy);
            }
        }
    }
#endif
}
