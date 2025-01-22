using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChestsBasedOnSpawnableObjectsByLevel<T>
{
    public GameObject chestObject;
    public List<SpawnableObjectsByLevel<T>> spawnableObjectByLevelList;
}
