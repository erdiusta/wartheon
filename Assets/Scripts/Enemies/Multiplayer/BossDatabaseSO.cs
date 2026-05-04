using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossDatabase_", menuName = "Scriptable Objects/Dungeon/Boss Database")]
public class BossDatabaseSO : ScriptableObject
{
    [SerializeField] List<EnemyDetailsSO> bosses;

    Dictionary<EnemyCategory, EnemyDetailsSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<EnemyCategory, EnemyDetailsSO>();

        foreach (EnemyDetailsSO boss in bosses)
        {
            lookup.Add(boss.enemyCategory, boss);
        }
    }

    public EnemyDetailsSO GetBoss(EnemyCategory category)
    {
        return lookup[category];
    }
}
