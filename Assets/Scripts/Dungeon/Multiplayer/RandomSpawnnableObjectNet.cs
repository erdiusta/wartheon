using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObjectNet
{
    struct ChanceBoundaries
    {
        public EnemyCategory category;
        public int low;
        public int high;
    }

    private readonly List<ChanceBoundaries> chanceList = new();
    private int totalRatio;

    public RandomSpawnableObjectNet(EnemiesByLevelNet[] enemiesByLevel, int dungeonLevelIndex)
    {
        BuildTable(enemiesByLevel, dungeonLevelIndex);
    }

    private void BuildTable(EnemiesByLevelNet[] enemiesByLevel, int dungeonLevelIndex)
    {
        chanceList.Clear();
        totalRatio = 0;

        foreach (EnemiesByLevelNet level in enemiesByLevel)
        {
            if (level.dungeonlevelIndex != dungeonLevelIndex) continue;

            int upper = -1;

            foreach (var entry in level.enemyRatios)
            {
                int lower = upper + 1;
                upper += entry.ratio;

                chanceList.Add(new ChanceBoundaries
                {
                    category = entry.enemyCategory,
                    low = lower,
                    high = upper
                });

                totalRatio += entry.ratio;
            }

            break;
        }
    }

    public EnemyCategory GetRandomCategory()
    {
        if (chanceList.Count == 0) return EnemyCategory.None;

        int roll = Random.Range(0, totalRatio);

        foreach (var chance in chanceList)
        {
            if (roll >= chance.low && roll <= chance.high) return chance.category;
        }

        return EnemyCategory.None;
    }
}
