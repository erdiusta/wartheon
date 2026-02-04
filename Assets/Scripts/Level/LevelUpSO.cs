using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelUpDetails_", menuName = "Scriptable Objects/Player/LevelUpDetails")]
public class LevelUpDetailsSO : ScriptableObject
{
    public List<LevelThresholdData> playerLevelDataList;
}

[System.Serializable]
public class LevelThresholdData
{
    public int playerLevel;
    public int levelUpExpPointForNextLevel;
}

