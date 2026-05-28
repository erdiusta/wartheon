using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class SoundEffect : MonoBehaviour
{
    [HideInInspector] public AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Set the sound effect to play 
    /// </summary>
    public void SetSound(SoundEffectSO soundEffect, bool isMultiplayer)
    {
        audioSource.Stop();

        audioSource.spatialBlend = isMultiplayer ? soundEffect.spatialBlendMP : soundEffect.spatialBlendSP;

        audioSource.minDistance = soundEffect.minDistance;
        audioSource.maxDistance = soundEffect.maxDistance;

        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.dopplerLevel = 0f;

        audioSource.volume = soundEffect.soundEffectVolume;
        audioSource.pitch = Random.Range(soundEffect.soundEffectPitchRandomVariationMin, soundEffect.soundEffectPitchRandomVariationMax);

        audioSource.panStereo = 0;
        audioSource.loop = false;
        audioSource.spatialize = false;
        audioSource.reverbZoneMix = 1f;

        AudioClip selectedClip = soundEffect.soundEffectClips[Random.Range(0, soundEffect.soundEffectClips.Length)];
        audioSource.clip = selectedClip;

        audioSource.Play();
    }
}
