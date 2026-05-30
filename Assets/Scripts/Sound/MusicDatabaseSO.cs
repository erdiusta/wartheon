using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicDatabase", menuName = "Scriptable Objects/Music/Music Database")]
public class MusicDatabaseSO : ScriptableObject
{
    [SerializeField] List<LevelMusicSet> levelMusics;

    Dictionary<int, LevelMusicSet> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<int, LevelMusicSet>();

        for (int i = 0; i < levelMusics.Count; i++)
        {
            lookup.Add(i, levelMusics[i]);
        }
    }

    public MusicTrackSO GetMusic(int level, MusicType musicType)
    {
        LevelMusicSet musicSet = GetLevelMusic(level);

        switch (musicType)
        {
            case MusicType.Ambient: return musicSet.ambientMusic;
            case MusicType.Combat: return musicSet.combatMusic;
            case MusicType.Shop: return musicSet.shopMusic;
            case MusicType.Boss: return musicSet.bossMusic;
            default: return null;
        }
    }

    private LevelMusicSet GetLevelMusic(int level)
    {
        if (lookup == null)
        {
            Debug.LogError("MusicDatabase not initialized!");
            return default;
        }

        if (!lookup.TryGetValue(level, out LevelMusicSet musicSet))
        {
            Debug.LogError($"Music not found in database {level}");
            return default;
        }

        return musicSet;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < levelMusics.Count; i++)
        {
            if (levelMusics[i].ambientMusic == null) Debug.LogWarning($"Level {i + 1} Ambient Music is missing.", this);
            if (levelMusics[i].combatMusic == null) Debug.LogWarning($"Level {i + 1} Combat Music is missing.", this);
            if (levelMusics[i].shopMusic == null) Debug.LogWarning($"Level {i + 1} Shop Music is missing.", this);
            if (levelMusics[i].bossMusic == null) Debug.LogWarning($"Level {i + 1} Boss Music is missing.", this);
        }
    }
#endif
}

[System.Serializable]
public struct LevelMusicSet
{
    public MusicTrackSO ambientMusic;
    public MusicTrackSO combatMusic;
    public MusicTrackSO shopMusic;
    public MusicTrackSO bossMusic;
}
