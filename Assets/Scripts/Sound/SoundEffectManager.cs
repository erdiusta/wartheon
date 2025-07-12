using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonobehaviour<SoundEffectManager>
{
    public int soundVolume = 8;

    SoundEffect playingSound;

    private void Start()
    {
        SetSoundVolume(soundVolume);
    }

    /// <summary>
    /// Play the sound effect
    /// </summary>
    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        // Play sound using a sound gameobject and component from the object pool
        playingSound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        playingSound.SetSound(soundEffect);
        playingSound.gameObject.SetActive(true);
        AudioClip selectedClip = soundEffect.soundEffectClips[Random.Range(0, soundEffect.soundEffectClips.Length)];
        StartCoroutine(DisableSound(selectedClip.length));
    }

    /// <summary>
    /// Stop the sound effect
    /// </summary>
    public void StopSoundEffect(SoundEffectSO soundEffect)
    {
        // Play sound using a sound gameobject and component from the object pool
        playingSound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        playingSound.gameObject.SetActive(false);
    }

    /// <summary>
    /// Disable sound effect object after it has played thus returning it to the object pool
    /// </summary>
    IEnumerator DisableSound(float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        playingSound.gameObject.SetActive(false);
    }

    public void SetVolume(int value)
    {
        int maxSoundVolume = 20;
        soundVolume = value;

        if (soundVolume >= maxSoundVolume) return;

        SetSoundVolume(soundVolume);
    }

    /// <summary>
    /// Increase sound volume
    /// </summary>
    public void IncreaseSoundVolume()
    {
        int maxSoundVolume = 20;

        if (soundVolume >= maxSoundVolume) return;

        soundVolume += 1;
        SetSoundVolume(soundVolume);
    }

    /// <summary>
    /// Decrease sound volume
    /// </summary>
    public void DecreaseSoundVolume()
    {
        if (soundVolume == 0) return;

        soundVolume -= 1;
        SetSoundVolume(soundVolume);
    }

    /// <summary>
    /// Set sounds volume
    /// </summary>
    private void SetSoundVolume(int soundVolume)
    {
        float muteDecibels = -80f;

        if (soundVolume == 0)
        {
            GameResources.Instance.soundMasterMixerGroup.audioMixer.SetFloat("soundVolume", muteDecibels);
        }
        else
        {
            GameResources.Instance.soundMasterMixerGroup.audioMixer.SetFloat("soundVolume", HelperUtilities.LinearToDecibels(soundVolume));
        }
    }

    public int GetSoundVolume() => soundVolume;
}
