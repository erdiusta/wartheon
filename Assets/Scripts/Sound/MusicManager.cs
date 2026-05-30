using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : SingletonMonobehaviour<MusicManager>
{
    AudioSource musicAudioSource = null;
    AudioClip currentAudioClip = null;
    Coroutine fadeOutMusicCoroutine;
    Coroutine fadeInMusicCoroutine;

    public int musicVolume = 10;

    MusicType currentMusicType;
    int currentLevel = -1;
    bool hasMusicPlaying;

    protected override void Awake()
    {
        base.Awake();

        musicAudioSource = GetComponent<AudioSource>();

        // Start with music off
        GameResources.Instance.musicOffSnapshot.TransitionTo(0f);
    }

    private void Start()
    {
        // Check if volume levels have been saved in playerprefs - if so retrieve and set them
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            musicVolume = PlayerPrefs.GetInt("musicVolume");
        }

        SetMusicVolume(musicVolume);
    }

    private void OnDisable()
    {
        // Save volume settings in playerprefs
        PlayerPrefs.SetInt("musicVolume", musicVolume);
    }

    public void PlayMusic(MusicTrackSO musicTrack, float fadeOutTime = Settings.musicFadeOutTime, float fadeInTime = Settings.musicFadeInTime)
    {
        // Play music track
        StartCoroutine(PlayMusicRoutine(musicTrack, fadeOutTime, fadeInTime));
    }

    public void RefreshMusicMP(RoomNetData room)
    {
        if (room == null) return;

        MusicType targetMusicType = DetermineMusicType(room);
        int level = GameSessionManager.Instance.selectedDungeonLevelIndex;

        if (hasMusicPlaying && targetMusicType == currentMusicType && level == currentLevel) return;

        currentMusicType = targetMusicType;
        currentLevel = level;
        hasMusicPlaying = true;

        MusicTrackSO music = WartheonDatabase.Instance.GetMusic(level, targetMusicType);

        PlayMusic(music);
    }

    MusicType DetermineMusicType(RoomNetData room)
    {
        if (room.isShopRoom) return MusicType.Shop;

        if (room.isBossRoom)
        {
            if (room.isClearedOfEnemies) return MusicType.Ambient;

            return MusicType.Boss;
        }

        if (room.isCombatRoom)
        {
            if (room.isClearedOfEnemies) return MusicType.Ambient;

            return MusicType.Combat;
        }

        return MusicType.Ambient;
    }

    /// <summary>
    /// Play music for room routine
    /// </summary>
    IEnumerator PlayMusicRoutine(MusicTrackSO musicTrack, float fadeOutTime, float fadeInTime)
    {
        // If fade out routine already running then stop it
        if (fadeOutMusicCoroutine != null)
        {
            StopCoroutine(fadeOutMusicCoroutine);
        }

        // If fade in routine already running then stop it
        if (fadeInMusicCoroutine != null)
        {
            StopCoroutine(fadeInMusicCoroutine);
        }

        // If the music track has changed then play new music track
        if (musicTrack.musicClip != currentAudioClip)
        {
            currentAudioClip = musicTrack.musicClip;
            yield return fadeOutMusicCoroutine = StartCoroutine(FadeOutMusic(fadeOutTime));

            yield return fadeInMusicCoroutine = StartCoroutine(FadeInMusic(musicTrack, fadeInTime));
        }

        yield return null;
    }

    /// <summary>
    /// Fade out music routine
    /// </summary>
    IEnumerator FadeOutMusic(float fadeOutTime)
    {
        GameResources.Instance.musicLowSnapshot.TransitionTo(fadeOutTime);

        yield return new WaitForSeconds(fadeOutTime);
    }

    /// <summary>
    /// Fade in music routine
    /// </summary>
    IEnumerator FadeInMusic(MusicTrackSO musicTrack, float fadeInTime)
    {
        // Set clip & play
        musicAudioSource.clip = musicTrack.musicClip;
        musicAudioSource.volume = musicTrack.musicVolume;
        musicAudioSource.Play();

        GameResources.Instance.musicOnFullSnaphot.TransitionTo(fadeInTime);

        yield return new WaitForSeconds(fadeInTime);
    }

    public void SetVolume(int value)
    {
        int maxMusicVolume = 20;
        musicVolume = value;

        if (musicVolume >= maxMusicVolume) return;

        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Increase music volume
    /// </summary>
    public void IncreaseMusicVolume()
    {
        int maxMusicVolume = 20;

        if (musicVolume >= maxMusicVolume) return;

        musicVolume += 1;
        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Decrease music volume
    /// </summary>
    public void DecreaseMusicVolume()
    {
        if (musicVolume <= 0) return;

        musicVolume -= 1;
        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    private void SetMusicVolume(int musicVolume)
    {
        float muteDecibels = -80f;

        if (musicVolume == 0)
        {
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", muteDecibels);
        }
        else
        {
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", HelperUtilities.LinearToDecibels(musicVolume));
        }
    }

    public int GetMusicVolume() => musicVolume;
}

