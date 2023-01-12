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
    [SerializeField] bool muteMusic = false;
    [SerializeField] bool muteSfx = false;

    [SerializeField] Sound[] musicSounds;
    [SerializeField] Sound[] sfxSounds;

    [SerializeField][Range(1, 5)] int maxMusicSource = 1;
    [SerializeField] [Range(1, 15)] int maxSfxSource = 3;

    public static SoundManager Instance;

    private AudioSource[] musicSources;
    private AudioSource[] sfxSources;
    //private AudioSource sfxSourcePlayOneShot;

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

        StartCoroutine(TestCoroutine());
    }

    IEnumerator TestCoroutine()
    {
        PlayMusic("main theme");
        yield return new WaitForSeconds(1);
        RestartMusic("main theme");
    }

    private void Start()
    {
        CheckMute();
    }

    private void OnValidate()
    {
        CheckMute();
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
    /// Play a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    /// <param name="sourcesAvailable">The sources where the sound can be played.</param>
    /// <param name="soundsBank">The bank of sound where to search the sound by its name.</param>
    /// <param name="position">The position where to put the sound. If it don't use 3D, just pass Vector3.zero.</param>
    /// <param name="is3D">True if the sound is positioned in 3D space, otherwise false.</param>
    private void PlaySound(string soundName, AudioSource[] sourcesAvailable, Sound[] soundsBank, Vector3 position, bool is3D)
    {
        if (soundsBank.Length == 0)
        {
            Debug.LogError($"There is no Sound in {soundsBank}.");
            return;
        }

        Sound sound = Array.Find(soundsBank, x => x.name.ToLower() == soundName.ToLower());

        if (sound == null)
        {
            Debug.LogError($"Can't find {soundName} sound in {soundsBank}.");
        }
        else
        {
            AudioSource source = FindAudioSource(sourcesAvailable, sound.priority);

            if (source == null)
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
                source.Play();
            }
        }
    }
    #endregion

    #region Fade

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
        List<AudioSource> sources = new List<AudioSource>();

        // Find all the sources that currently playing this sound
        foreach (AudioSource source in sourcesToSearch)
        {
            if (source.clip.name.ToLower() == soundName.ToLower() && source.isPlaying)
            {
                sources.Add(source);
            }
        }

        foreach (AudioSource source in sources)
        {
            source.Stop();
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
        PauseSound(musicName, musicSources);
    }

    /// <summary>
    /// Pause a sfx sound.
    /// </summary>
    /// <param name="sfxName">The name of the sound to pause.</param>
    public void PauseSfx(string sfxName)
    {
        PauseSound(sfxName, sfxSources);
    }

    /// <summary>
    /// Pause a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to pause.</param>
    /// <param name="sourcesToSearch">The audio sources where to search the sound to pause.</param>
    private void PauseSound(string soundName, AudioSource[] sourcesToSearch)
    {
        List<AudioSource> sources = new List<AudioSource>();

        // Find all the sources that currently playing this sound
        foreach (AudioSource source in sourcesToSearch)
        {
            if (source.clip.name.ToLower() == soundName.ToLower() && source.isPlaying)
            {
                sources.Add(source);
            }
        }

        foreach (AudioSource source in sources)
        {
            source.Pause();
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
        List<AudioSource> sources = new List<AudioSource>();

        // Find all the sources that currently playing this sound
        foreach (AudioSource source in sourcesToSearch)
        {
            if (source.clip.name.ToLower() == soundName.ToLower() && source.isPlaying)
            {
                sources.Add(source);
            }
        }

        foreach (AudioSource source in sources)
        {
            if (mute)
            {
                source.volume = 0;
            }
            else
            {
                // Reset the volume to the sound volume
                Sound sound = Array.Find(soundsBank, x => x.name.ToLower() == soundName.ToLower());
                source.volume = sound.volume;
            }
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
    /// Setup the audio source.
    /// </summary>
    /// <param name="source">The audio source to setup.</param>
    /// <param name="sound">The sound that will be played on the audio source.</param>
    private void SetupSource(AudioSource source, Sound sound)
    {
        source.clip = sound.audioClip;
        source.clip.name = sound.name;
        source.volume = sound.volume;
        source.priority = sound.priority;
        source.panStereo = sound.pan;
        source.loop = sound.loop;
        source.outputAudioMixerGroup = sound.mixerGroup;
    }

    /// <summary>
    /// Find a free audio source to play a sound.
    /// </summary>
    /// <param name="sources">The possible audio sources to play.</param>
    /// <param name="priority">The priority of the sound we want to play.</param>
    /// <returns>A free audio source or null if the sound we want to play has not enough priority.</returns>
    private AudioSource FindAudioSource(AudioSource[] sources, int priority)
    {
        // Search for an audio source that is not playing
        foreach(AudioSource source in sources)
        {
            if(!source.isPlaying)
            {
                return source;
            }
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
        if(priority < lowestPriority)
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
    #endregion
}
