using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    struct ChanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    int ratioValueTotal = 0;
    List<ChanceBoundaries> chanceBoundariesList = new List<ChanceBoundaries>();
    List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList;

    public RandomSpawnableObject(List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList)
    {
        this.spawnableObjectsByLevelList = spawnableObjectsByLevelList;
    }

    public T GetItem(WartheonRNG rng)
    {   
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        T spawnableObject = default(T);

        foreach (SpawnableObjectsByLevel<T> spawnableObjectsByLevel in spawnableObjectsByLevelList)
        {
            // Check for current level
            if (spawnableObjectsByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectsByLevel.spawnableObjectRatioList)
                {
                    int lowerBoundary = upperBoundary + 1;
                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;
                    ratioValueTotal += spawnableObjectRatio.ratio;

                    // Add spawnable object to list
                    chanceBoundariesList.Add(new ChanceBoundaries { spawnableObject = spawnableObjectRatio.dungeonObject, 
                        lowBoundaryValue = lowerBoundary, highBoundaryValue = upperBoundary});
                }
            }
        }

        if (chanceBoundariesList.Count == 0)
            return default(T);

        int lookUpValue = rng.Range(0, ratioValueTotal);

        if (InputManager.TutorialEnabled)
        {
            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat || TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
            {
                lookUpValue = 0;
            }
            else if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
            {
                lookUpValue = 1;
            }
        }

        // Loop through list to get seleted random spawnable object details
        foreach (ChanceBoundaries spawnChance in chanceBoundariesList)
        {
            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                spawnableObject = spawnChance.spawnableObject;
                break;
            }
        }

        return spawnableObject;
    }
}
