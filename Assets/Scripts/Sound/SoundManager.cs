using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    [Header("Infos")]
    [SerializeField] bool muteMusic = false;
    [SerializeField] bool muteSfx = false;
    [SerializeField] bool showDebug = false;

    [Header("Other")]
    [SerializeField] AudioMixerGroup masterGroup;

    [Header("Sounds")]
    [SerializeField] Sound[] musicSounds;
    [SerializeField] Sound[] sfxSounds;

    [Header("Sources")]
    [SerializeField][Range(1, 5)] int maxMusicSource = 1;
    [SerializeField] [Range(1, 15)] int maxSfxSource = 3;

    public static SoundManager Instance { get; private set; }

    private AudioSource[] musicSources;
    private AudioSource[] sfxSources;
    //private AudioSource sfxSourcePlayOneShot;

    private Dictionary<AudioSource, Coroutine> sourceCoroutine = new Dictionary<AudioSource, Coroutine>();

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        musicSources = new AudioSource[maxMusicSource];
        sfxSources = new AudioSource[maxSfxSource];

        // Create music and sfx sources
        GameObject musicParent = new GameObject("MusicSources");
        musicParent.transform.parent = transform;
        for (int i = 0; i < maxMusicSource; i++)
        {
            GameObject musicGO = new GameObject($"MusicSource_#{i + 1}");
            musicGO.transform.parent = musicParent.transform;
            musicSources[i] = musicGO.AddComponent<AudioSource>();
        }

        GameObject sfxParent = new GameObject("SfxSources");
        sfxParent.transform.parent = transform;
        for (int i = 0; i < maxSfxSource; i++)
        {
            GameObject sfxGO = new GameObject($"SfxSource_#{i + 1}");
            sfxGO.transform.parent = sfxParent.transform;
            sfxSources[i] = sfxGO.AddComponent<AudioSource>();
        }

        // Create Sfx source for sfx using the function PlayOneShot
        //GameObject sfxPlayOneGO = new GameObject("SfxSourcePlayOneShot");
        //sfxPlayOneGO.transform.parent = transform;
        //sfxSourcePlayOneShot = sfxPlayOneGO.AddComponent<AudioSource>();

        PlayMusic("main theme");
    }

    private void Start()
    {
        CheckMute();
    }

    private void OnEnable()
    {
        Events.OnGamePause += MusicLowPass;
        Events.OnVolumeChange += ChangeMasterVolume;
    }

    private void OnDisable()
    {
        Events.OnGamePause -= MusicLowPass;
        Events.OnVolumeChange -= ChangeMasterVolume;
    }

    private void OnValidate()
    {
        CheckMute();
    }

    private void ChangeMasterVolume(float volume)
    {
        masterGroup.audioMixer.SetFloat("MasterVolume", volume);
    }

    private void MusicLowPass(bool isPause)
    {
        if(isPause)
        {
            foreach(AudioSource source in musicSources)
            {
                if(source.isPlaying)
                {
                    source.outputAudioMixerGroup.audioMixer.SetFloat("MusicLowPassFilter", 3000);
                }
            }
        }
        else
        {
            foreach (AudioSource source in musicSources)
            {
                if (source.isPlaying)
                {
                    source.outputAudioMixerGroup.audioMixer.SetFloat("MusicLowPassFilter", 22000);
                }
            }
        }
    }

    #region Play
    public void PlayMusic(string musicName)
    {
        PlaySound(musicName, musicSources, musicSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Play a sound just once and with no 3D settings.
    /// </summary>
    /// <param name="sfxName">The name of the sfx sound to play.</param>
    public void PlaySfx(string sfxName)
    {
        PlaySound(sfxName, sfxSources, sfxSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Play a sound just once with 3D settings.
    /// </summary>
    /// <param name="sfxName">The name of the sfx sound to play.</param>
    /// <param name="position">The position where to play the sound.</param>
    public void PlaySfx(string sfxName, Vector3 position)
    {
        PlaySound(sfxName, sfxSources, sfxSounds, position, true);
    }

    /// <summary>
    /// Play a music sound in loop with a given time between loops.
    /// </summary>
    /// <param name="musicName">Name of the music to play.</param>
    /// <param name="timeLoop">Time (in seconds) between each loop.</param>
    public void PlayMusicControlLoop(string musicName, float timeLoop)
    {
        PlaySoundControlLoop(musicName, timeLoop, musicSources, musicSounds, transform, false);
    }

    /// <summary>
    /// Play a sfx sound in loop with a given time between loops with no 3D properties.
    /// </summary>
    /// <param name="sfxName">Name of the sfx to play.</param>
    /// <param name="timeLoop">Time (in seconds) between each loop.</param>
    public void PlaySfxControlLoop(string sfxName, float timeLoop)
    {
        PlaySoundControlLoop(sfxName, timeLoop, sfxSources, sfxSounds, transform, false);
    }

    /// <summary>
    /// Play a sfx sound in loop with a given time between loops with 3D properties.
    /// </summary>
    /// <param name="sfxName">Name of the sfx to play.</param>
    /// <param name="timeLoop">Time (in seconds) between each loop.</param>
    /// <param name="transform">Transform of the object to follow while looping</param>
    public void PlaySfxControlLoop(string sfxName, float timeLoop, Transform transform)
    {
        PlaySoundControlLoop(sfxName, timeLoop, sfxSources, sfxSounds, transform, true);
    }

    /// <summary>
    /// Play a sound in loop with a given time between loops.
    /// </summary>
    /// <param name="soundName">Name of the sound to play.</param>
    /// <param name="timeLoop">Time (in seconds) between each loop.</param>
    /// <param name="sourcesAvailable">Where to search an audio source to play the sound.</param>
    /// <param name="soundsBank">Where to search the sound to play.</param>
    /// <param name="transform">Transform of the object to follow while looping</param>
    /// <param name="is3D">True if the sound use 3D properties, otherwise false.</param>
    private void PlaySoundControlLoop(string soundName, float timeLoop, AudioSource[] sourcesAvailable, Sound[] soundsBank, Transform transform, bool is3D)
    {
        AudioSource source = PlaySound(soundName, sourcesAvailable, soundsBank, transform.position, is3D);

        if (source != null)
        {
            RemoveCoroutine(source);
            sourceCoroutine.Add(source, StartCoroutine(ControlLoopCoroutine(source, timeLoop, soundName, soundsBank, is3D, transform)));
        }
    }

    private IEnumerator ControlLoopCoroutine(AudioSource source, float timeLoop, string soundName, Sound[] soundBank, bool is3D, Transform target)
    {
        float currTime = 0;
        Sound sound = FindSound(soundName, soundBank);

        while(true)
        {
            yield return null;

            currTime += Time.deltaTime;
            if (currTime >= timeLoop)
            {
                // Force to replay on the same audio source and get a new random audio clip
                ForcePlaySound(source, sound);
                currTime = 0;
            }

            if (is3D)
            {
                source.transform.position = target.position;
            }
        }
    }

    /// <summary>
    /// Play a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    /// <param name="sourcesAvailable">The sources where the sound can be played.</param>
    /// <param name="soundsBank">The bank of sound where to search the sound by its name.</param>
    /// <param name="position">The position where to put the sound. If it don't use 3D, just pass Vector3.zero.</param>
    /// <param name="is3D">True if the sound is positioned in 3D space, otherwise false.</param>
    /// <returns>The audio source where the sound is played.</returns>
    private AudioSource PlaySound(string soundName, AudioSource[] sourcesAvailable, Sound[] soundsBank, Vector3 position, bool is3D)
    {
        Sound sound = FindSound(soundName, soundsBank);

        if (sound == null)
        {
            Debug.LogError($"Can't find {soundName.ToUpper()} sound in {soundsBank}.");
        }
        else
        {
            AudioSource source = FindAudioSource(sourcesAvailable, sound);

            if (source == null && showDebug)
            {
                Debug.Log($"Can't play the sound {sound.name} because there is no more available audio sources and the priority of this sound is lesser than the others.");
            }
            else
            {
                SetupSource(source, sound);
                if(is3D)
                {
                    SetSourceTo3D(source, position);
                }

                RemoveCoroutine(source);
                source.Play();
            }

            return source;
        }

        return null;
    }
    #endregion

    #region Fade
    /// <summary>
    /// Play a music sound with a fade in effect.
    /// </summary>
    /// <param name="musicName">Name of the music to play.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade in.</param>
    public void PlayFadeMusic(string musicName, float fadeTime)
    {
        PlayFade(musicName, fadeTime, musicSources, musicSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Play a sfx sound with a fade in effect with no 3D properties.
    /// </summary>
    /// <param name="sfxName">Name of the sfx to play.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade in.</param>
    public void PlayFadeSfx(string sfxName, float fadeTime)
    {
        PlayFade(sfxName, fadeTime, sfxSources, sfxSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Play a sfx sound with fade in effect and 3D properties.
    /// </summary>
    /// <param name="sfxName">Name of the sfx to play.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade in.</param>
    /// <param name="position">Where to play the sound.</param>
    public void PlayFadeSfx(string sfxName, float fadeTime, Vector3 position)
    {
        PlayFade(sfxName, fadeTime, sfxSources, sfxSounds, position, true);
    }

    /// <summary>
    /// Play a sound with a fade in effect.
    /// </summary>
    /// <param name="soundName">Name of the sound to play.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade in.</param>
    /// <param name="sources">Audio sources that can play the sound.</param>
    /// <param name="soundBank">Where the sound is located.</param>
    /// <param name="position">Where to play the sound.</param>
    /// <param name="is3D">True if the sound use 3D properties, otherwise false.</param>
    private void PlayFade(string soundName, float fadeTime, AudioSource[] sources, Sound[] soundBank, Vector3 position, bool is3D)
    {
        // Try to play the sound and get the audio source
        AudioSource source = PlaySound(soundName, sources, soundBank, position, is3D);

        // If the sound successfuly played
        if (source != null)
        {
            // Remove any left coroutine on this audio source
            RemoveCoroutine(source);
            sourceCoroutine.Add(source, StartCoroutine(FadeCoroutine(source, fadeTime, true)));
        }
    }

    /// <summary>
    /// Stop a music sound with fade out effect.
    /// </summary>
    /// <param name="musicName">Name of the music to stop.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade out.</param>
    public void StopFadeMusic(string musicName, float fadeTime)
    {
        StopFade(musicName, fadeTime, musicSources);
    }

    /// <summary>
    /// Stop a sfx sound with fade out effect.
    /// </summary>
    /// <param name="sfxName">Name of the sfx to stop.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade out.</param>
    public void StopFadeSfx(string sfxName, float fadeTime)
    {
        StopFade(sfxName, fadeTime, sfxSources);
    }

    /// <summary>
    /// Stop a sound with fade out effect.
    /// </summary>
    /// <param name="soundName">Name of the sound to stop.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade out.</param>
    /// <param name="sourcesToSearch">Where to search the sources to stop.</param>
    private void StopFade(string soundName, float fadeTime, AudioSource[] sourcesToSearch)
    {
        List<AudioSource> sources = FindSoundOnSource(soundName, sourcesToSearch, true);

        foreach(AudioSource source in sources)
        {
            RemoveCoroutine(source);
            sourceCoroutine.Add(source, StartCoroutine(FadeCoroutine(source, fadeTime, false)));
        }

        if (sources.Count == 0 && showDebug)
        {
            Debug.Log($"Can't find a sound named {soundName.ToUpper()} to stop.");
        }
    }

    /// <summary>
    /// Do a cross fade between two music sound.
    /// </summary>
    /// <param name="musicNameIn">Name of the music to fade in.</param>
    /// <param name="musicNameOut">Name of the music to fade out.</param>
    /// <param name="fadeTime">Duration (in seconds) of the cross fade.</param>
    public void CrossFadeMusic(string musicNameIn, string musicNameOut, float fadeTime)
    {
        CrossFade(musicNameIn, musicNameOut, fadeTime, musicSources, musicSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Do a cross fade between two sfx sound with no 3D properties.
    /// </summary>
    /// <param name="sfxNameIn">Name of the sfx to fade in.</param>
    /// <param name="sfxNameOut">Name of the sfx to fade out.</param>
    /// <param name="fadeTime">Duration (in seconds) of the cross fade.</param>
    public void CrossFadeSfx(string sfxNameIn, string sfxNameOut, float fadeTime)
    {
        CrossFade(sfxNameIn, sfxNameOut, fadeTime, sfxSources, sfxSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Do a cross fade between two sfx sound with 3D properties.
    /// </summary>
    /// <param name="sfxNameIn">Name of the sfx to fade in.</param>
    /// <param name="sfxNameOut">Name of the sfx to fade out.</param>
    /// <param name="fadeTime">Duration (in seconds) of the cross fade.</param>
    /// <param name="position">Where to place the audio source.</param>
    public void CrossFadeSfx(string sfxNameIn, string sfxNameOut, float fadeTime, Vector3 position)
    {
        CrossFade(sfxNameIn, sfxNameOut, fadeTime, sfxSources, sfxSounds, position, true);
    }

    /// <summary>
    /// Do a cross fade between two sound.
    /// </summary>
    /// <param name="soundNameIn">Name of the sound to fade in.</param>
    /// <param name="soundNameOut">Name of the sound to fade out.</param>
    /// <param name="fadeTime">Duration (in seconds) of the cross fade.</param>
    /// <param name="sources">Where to search the audio sources.</param>
    /// <param name="soundBank">Where to search the sounds to cross fade.</param>
    /// <param name="position">Where to place the audio source.</param>
    /// <param name="is3D">True if the sound use 3D properties, otherwise false.</param>
    private void CrossFade(string soundNameIn, string soundNameOut, float fadeTime, AudioSource[] sources, Sound[] soundBank, Vector3 position, bool is3D)
    {
        if (sources.Length < 2)
        {
            Debug.LogError($"You need to have at least 2 audio sources in {sources} to do a crossfade.");
            return;
        }

        PlayFade(soundNameIn, fadeTime, sources, soundBank, position, is3D);
        StopFade(soundNameOut, fadeTime, sources);
    }

    /// <summary>
    /// Apply a fade in or fade out effect on an audio source.
    /// </summary>
    /// <param name="source">Where to apply the fade effect.</param>
    /// <param name="fadeTime">Duration (in seconds) of the fade effect.</param>
    /// <param name="fadeIn">True for a fade in, false for a fade out.</param>
    /// <returns></returns>
    private IEnumerator FadeCoroutine(AudioSource source, float fadeTime, bool fadeIn)
    {
        float startVolume = source.volume;
        float finalVolume = source.volume;

        if (fadeIn)
        {
            source.volume = 0;
        }

        float currTime = 0;
        while (currTime < fadeTime)
        {
            yield return null;

            // Only add time when the sound is playing
            if (source.isPlaying)
            {
                currTime += Time.deltaTime;
            }
            
            if (currTime > fadeTime)
            {
                currTime = fadeTime;
            }

            if (fadeIn)
            {
                source.volume = Mathf.Lerp(0, finalVolume, currTime / fadeTime);
            }
            else
            {
                source.volume = Mathf.Lerp(startVolume, 0, currTime / fadeTime);
            }
        }

        if (!fadeIn)
        {
            source.Stop();
        }

        sourceCoroutine.Remove(source);
    }
    #endregion

    #region Restart
    /// <summary>
    /// Restart a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music to restart.</param>
    public void RestartMusic(string musicName)
    {
        RestartSound(musicName, musicSources, musicSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Restart a sfx sound with no 3D properties.
    /// </summary>
    /// <param name="sfxName">The name of the sfx sound to restart.</param>
    public void RestartSfx(string sfxName)
    {
        RestartSound(sfxName, sfxSources, sfxSounds, Vector3.zero, false);
    }

    /// <summary>
    /// Restart a sfx sound with 3D properties.
    /// </summary>
    /// <param name="sfxName">The name of the sfx sound to restart.</param>
    /// <param name="position">The position where to place the sfx sound.</param>
    public void RestartSfx(string sfxName, Vector3 position)
    {
        RestartSound(sfxName, sfxSources, sfxSounds, position, true);
    }

    /// <summary>
    /// Restart a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to restart.</param>
    /// <param name="sourcesToSearch">The audio sources where it's possible to restart the sound.</param>
    /// <param name="soundsBank">The sound bank where to search the sound to restart by its name.</param>
    /// <param name="position">The position where to place the sound.</param>
    /// <param name="is3D">True if the sound use 3D properties, otherwise false.</param>
    private void RestartSound(string soundName, AudioSource[] sourcesToSearch, Sound[] soundsBank, Vector3 position, bool is3D)
    {
        StopSound(soundName, sourcesToSearch);
        PlaySound(soundName, sourcesToSearch, soundsBank, position, is3D);
    }
    #endregion

    #region Stop
    /// <summary>
    /// Stop a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music to stop.</param>
    public void StopMusic(string musicName)
    {
        StopSound(musicName, musicSources);
    }

    /// <summary>
    /// Stop a sfx sound.
    /// </summary>
    /// <param name="sfxName">The name of the sound to stop.</param>
    public void StopSfx(string sfxName)
    {
        StopSound(sfxName, sfxSources);
    }

    /// <summary>
    /// Stop a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to stop.</param>
    /// <param name="sourcesToSearch">The audio sources where to search the sound to stop.</param>
    private void StopSound(string soundName, AudioSource[] sourcesToSearch)
    {
        List<AudioSource> sources = FindSoundOnSource(soundName, sourcesToSearch, true);

        foreach (AudioSource source in sources)
        {
            source.Stop();
            RemoveCoroutine(source);
        }

        if (sources.Count == 0 && showDebug)
        {
            Debug.Log($"Can't find any sound named {soundName.ToUpper()} to be stopped.");
        }
    }

    public void StopMusicControlLoop(string musicName)
    {
        StopSoundControlLoop(musicName, musicSources);
    }

    public void StopSfxControlLoop(string sfxName)
    {
        StopSoundControlLoop(sfxName, sfxSources);
    }

    private void StopSoundControlLoop(string soundName, AudioSource[] sourcesToSearch)
    {
        List<AudioSource> sources = FindSoundOnSource(soundName, sourcesToSearch, true);

        foreach (AudioSource source in sources)
        {
            //StartCoroutine(StopSoundControlLoopCoroutine(source));
            RemoveCoroutine(source);
        }

        if (sources.Count == 0 && showDebug)
        {
            Debug.Log($"Can't find any sound named {soundName.ToUpper()} to be stopped.");
        }
    }
    #endregion

    #region Pause
    /// <summary>
    /// Pause a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music to pause.</param>
    public void PauseMusic(string musicName)
    {
        PauseOrUnpauseSound(musicName, musicSources, true);
    }

    /// <summary>
    /// Unpause a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music to unpause.</param>
    public void UnpauseMusic(string musicName)
    {
        PauseOrUnpauseSound(musicName, musicSources, false);
    }

    /// <summary>
    /// Pause a sfx sound.
    /// </summary>
    /// <param name="sfxName">The name of the sound to pause.</param>
    public void PauseSfx(string sfxName)
    {
        PauseOrUnpauseSound(sfxName, sfxSources, true);
    }

    /// <summary>
    /// Unpause a sfx sound.
    /// </summary>
    /// <param name="sfxName">The name of the sfx to unpause.</param>
    public void UnpauseSfx(string sfxName)
    {
        PauseOrUnpauseSound(sfxName, sfxSources, false);
    }

    /// <summary>
    /// Pause a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to pause.</param>
    /// <param name="sourcesToSearch">The audio sources where to search the sound to pause.</param>
    /// <param name="sourcesToSearch">True if the sound must be pause, false if the sound must unpause.</param>
    private void PauseOrUnpauseSound(string soundName, AudioSource[] sourcesToSearch, bool pause)
    {
        List<AudioSource> sources = FindSoundOnSource(soundName, sourcesToSearch, false);

        foreach (AudioSource source in sources)
        {
            if(pause)
            {
                source.Pause();
            }
            else if (!pause)
            {
                source.UnPause();
            }
        }

        if (sources.Count == 0 && showDebug)
        {
            Debug.Log($"Can't find any sound named {soundName.ToUpper()} to be paused or unpaused.");
        }
    }
    #endregion

    #region Mute
    /// <summary>
    /// Mute a sfx sound.
    /// </summary>
    /// <param name="sfxName">The sound of the sfx to mute.</param>
    public void MuteSfx(string sfxName)
    {
        MuteOrUnmuteSound(sfxName, sfxSources, sfxSounds, true);
    }

    /// <summary>
    /// Unmute a sfx sound.
    /// </summary>
    /// <param name="sfxName">The name of the sfx to unmute.</param>
    public void UnmuteSfx(string sfxName)
    {
        MuteOrUnmuteSound(sfxName, sfxSources, sfxSounds, false);
    }

    /// <summary>
    /// Mute a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music sound to mute.</param>
    public void MuteMusic(string musicName)
    {
        MuteOrUnmuteSound(musicName, musicSources, musicSounds, true);
    }

    /// <summary>
    /// Unmute a music sound.
    /// </summary>
    /// <param name="musicName">The name of the music to unmute.</param>
    public void UnmuteMusic(string musicName)
    {
        MuteOrUnmuteSound(musicName, musicSources, musicSounds, false);
    }

    /// <summary>
    /// Mute a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to mute.</param>
    /// <param name="sourcesToSearch">The sources where to search the sound.</param>
    /// <param name="soundsBank">The sound bank where the sound is coming from.</param>
    /// <param name="mute">True if sound must be mute, false if the sound must be unmute.</param>
    private void MuteOrUnmuteSound(string soundName, AudioSource[] sourcesToSearch, Sound[] soundsBank, bool mute)
    {
        List<AudioSource> sources = FindSoundOnSource(soundName, sourcesToSearch, true);

        foreach (AudioSource source in sources)
        {
            if (mute)
            {
                source.volume = 0;
            }
            else
            {
                // Reset the volume to the sound volume
                Sound sound = FindSound(soundName, soundsBank);
                source.volume = sound.volume;
            }
        }

        if (sources.Count == 0 && showDebug)
        {
            Debug.Log($"Can't find any sound named {soundName.ToUpper()} to be muted.");
        }
    }

    /// <summary>
    /// Mute all the music sounds.
    /// </summary>
    public void MuteAllMusic()
    {
        muteMusic = true;
    }

    /// <summary>
    /// Unmute all the music sounds.
    /// </summary>
    public void UnmuteAllMusic()
    {
        muteMusic = false;
    }

    /// <summary>
    /// Mute all the sfx sounds.
    /// </summary>
    public void MuteAllSfx()
    {
        muteSfx = true;
    }

    /// <summary>
    /// Unmute all the sfx sounds.
    /// </summary>
    public void UnmuteAllSfx()
    {
        muteSfx = false;
    }

    /// <summary>
    /// Mute the music or the sfx by the value of the boolean in the inspector.
    /// </summary>
    private void CheckMute()
    {
        if (musicSources != null)
        {
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.mute = muteMusic;
            }
        }
        if (sfxSources != null)
        {
            foreach (AudioSource sfxSource in sfxSources)
            {
                sfxSource.mute = muteSfx;
            }
        }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Get all the sources where the given sound is.
    /// </summary>
    /// <param name="soundName">Name of the sound to look for.</param>
    /// <param name="sourcesToSearch">Where to search the sound.</param>
    /// <param name="onlyPlaying">True will return only the sources that are currently playing. False will return even those in pause or stopped.</param>
    /// <returns>A list of AudioSource where the given sound is.</returns>
    private List<AudioSource> FindSoundOnSource(string soundName, AudioSource[] sourcesToSearch, bool onlyPlaying)
    {
        List<AudioSource> sources = new List<AudioSource>();

        foreach (AudioSource source in sourcesToSearch)
        {
            if (source.clip != null)
            {
                if (source.clip.name.ToLower() == soundName.ToLower())
                {
                    if (onlyPlaying && source.isPlaying)
                    {
                        sources.Add(source);
                    }
                    else
                    {
                        sources.Add(source);
                    }
                }
            }
        }

        return sources;
    }

    /// <summary>
    /// Remove a fade coroutine from an audio source.
    /// </summary>
    /// <param name="source">The source where the fade coroutine is.</param>
    private void RemoveCoroutine(AudioSource source)
    {
        Coroutine coroutine;
        bool hasCoroutine = sourceCoroutine.TryGetValue(source, out coroutine);
        if (hasCoroutine)
        {
            StopCoroutine(coroutine);
            sourceCoroutine.Remove(source);
        }
    }
    /// <summary>
    /// Setup the audio source.
    /// </summary>
    /// <param name="source">The audio source to setup.</param>
    /// <param name="sound">The sound that will be played on the audio source.</param>
    private void SetupSource(AudioSource source, Sound sound)
    {
        if (sound.audioClips.Length == 0)
        {
            Debug.LogError($"There is no audio clip in {sound.name}.");
            return;
        }

        AudioClip soundToPlay;
        if (sound.audioClips.Length > 1)
        {
            // If there is more than one audio clip on the sound object, select a random one
            soundToPlay = sound.audioClips[UnityEngine.Random.Range(0, sound.audioClips.Length)];
        }
        else
        {
            soundToPlay = sound.audioClips[0];
        }
        source.clip = soundToPlay;

        source.clip.name = sound.name;
        source.volume = sound.volume;
        source.priority = sound.priority;
        source.panStereo = sound.pan;
        source.loop = sound.loop;
        source.outputAudioMixerGroup = sound.mixerGroup;
        source.spatialBlend = 0;
    }

    /// <summary>
    /// Find a free audio source to play a sound.
    /// </summary>
    /// <param name="sources">The possible audio sources to play.</param>
    /// <param name="sound">Sound we want to play.</param>
    /// <returns>A free audio source or null if the sound we want to play has not enough priority.</returns>
    private AudioSource FindAudioSource(AudioSource[] sources, Sound sound)
    {
        // Stock a free source
        AudioSource freeSource = null;

        // Search for an audio source that is not playing
        foreach(AudioSource source in sources)
        {
            // If sound is allowing multiple play, find the first source that is not playing a sound
            if(sound.allowMultiplePlay && !source.isPlaying)
            {
                return source;
            }
            // Else find where was played this sound
            else if (!sound.allowMultiplePlay && source.clip != null)
            {
                if (source.clip.name == sound.name)
                    return source;
            }
            // In case the sound is not in any source, save a free source
            else if (!source.isPlaying)
            {
                freeSource = source;
            }
        }

        // If the sound don't allow multiple play but was not in any source, give it a free source
        if (!sound.allowMultiplePlay && freeSource != null)
        {
            return freeSource;
        }

        // If no audio source available, find the one with the lowest priority
        int lowestPriority = 0;
        int indexLowestPriority = 0;
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i].priority > lowestPriority) // 256 is lesser than 0 for priority
            {
                lowestPriority = sources[i].priority;
                indexLowestPriority = i;
            }
        }

        // Check the sound we want to play has less priority than the lowest we found
        if(sound.priority < lowestPriority)
        {
            return sources[indexLowestPriority];
        }
        else
        { 
            return null; 
        }
    }

    /// <summary>
    /// Set the audio source to play a 3D sound.
    /// </summary>
    /// <param name="source">To source to setup.</param>
    /// <param name="position">The position of the sound.</param>
    private void SetSourceTo3D(AudioSource source, Vector3 position)
    {
        source.spatialBlend = 1;
        source.minDistance = 100;
        source.spread = 0;
        source.transform.position = position;
    }

    /// <summary>
    /// Find a sound in a sound array.
    /// </summary>
    /// <param name="soundName">Name of the sound to find.</param>
    /// <param name="soundBank">Where to search the sound.</param>
    /// <returns></returns>
    private Sound FindSound(string soundName, Sound[] soundBank)
    {
        if (soundBank.Length == 0)
        {
            Debug.LogError($"There is no Sound in {soundBank}.");
            return null;
        }

        foreach (Sound sound in soundBank)
        {
            if (sound == null)
            {
                Debug.LogError("There is at least one empty element in {soundBank}. Remove this empty element or put a sound in it.");
                return null;
            }
        }

        return Array.Find(soundBank, x => x.name.ToLower() == soundName.ToLower());
    }

    /// <summary>
    /// Force to play a sound on a given audio source.
    /// </summary>
    /// <param name="source">Where to play the sound.</param>
    /// <param name="sound">Sound to play.</param>
    private void ForcePlaySound(AudioSource source, Sound sound)
    {
        SetupSource(source, sound);
        source.Play();
    }
    #endregion
}
