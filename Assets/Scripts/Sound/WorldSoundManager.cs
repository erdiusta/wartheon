using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class WorldSoundManager : SingletonMonobehaviour<WorldSoundManager>
{
    public SoundEffect PlayWorldSound(SoundEffectSO soundEffect, Vector3 position, bool isMultiplayer = false)
    {
        GameObject soundPrefab = soundEffect.soundPrefab;

        GameObject playingSoundObject = Instantiate(soundPrefab, position, Quaternion.identity);
        SoundEffect sound = playingSoundObject.GetComponent<SoundEffect>();

        sound.SetSound(soundEffect, isMultiplayer);

        if (!sound.audioSource.loop) StartCoroutine(DestroySound(sound, sound.audioSource.clip.length));

        return sound;
    }

    public void StopSoundEffect(SoundEffectSO soundEffect)
    {
        if (soundEffect == null || soundEffect.activeSound == null) return;

        SoundEffect sound = soundEffect.activeSound;
        sound.audioSource.loop = false;

        float remainingTime = Mathf.Max(0f, sound.audioSource.clip.length, sound.audioSource.time);

        StartCoroutine(DestroySound(sound, remainingTime));

        soundEffect.activeSound = null;
    }

    /// <summary>
    /// Disable sound effect object after it has played thus returning it to the object pool
    /// </summary>
    IEnumerator DestroySound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);

        Destroy(sound.gameObject);
    }
}
