using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Scriptable Objects/Sound/Sound Database")]
public class SoundDatabaseSO : ScriptableObject
{
    [SerializeField] List<SoundEffectSO> soundEffects;

    Dictionary<SoundName, SoundEffectSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<SoundName, SoundEffectSO>();

        foreach (SoundEffectSO sound in soundEffects)
        {
            if (lookup.ContainsKey(sound.soundName))
            {
                Debug.LogError($"Duplicate SoundEffect in database: {sound.soundName}");
                continue;
            }

            lookup.Add(sound.soundName, sound);
        }
    }

    public SoundEffectSO GetSound(SoundName soundName)
    {
        if (lookup == null)
        {
            Debug.LogError("SoundEffectSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(soundName, out SoundEffectSO sound))
        {
            Debug.LogError($"SoundEffectSO not found in database {soundName}");
            return null;
        }

        return sound;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var soundNames = new HashSet<SoundName>();

        foreach (var sound in soundEffects)
        {
            if (!soundNames.Add(sound.soundName))
            {
                Debug.LogError($"Duplicate sound name: {sound.soundName}", sound);
            }
        }
    }
#endif
}
