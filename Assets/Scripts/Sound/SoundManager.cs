using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private AudioSource sfxSourcePlayOneShot;

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
        GameObject sfxPlayOneGO = new GameObject("SfxSourcePlayOneShot");
        sfxPlayOneGO.transform.parent = transform;
        sfxSourcePlayOneShot = sfxPlayOneGO.AddComponent<AudioSource>();

        PlayMusic("Main theme");
    }

    private void Start()
    {
        CheckMute();
    }

    private void OnValidate()
    {
        CheckMute();
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

    public void PlayMusic(string soundName)
    {
        if (musicSounds.Length == 0)
        {
            Debug.LogError("There is no Sound in Music Sounds.");
            return;
        }

        Sound sound = Array.Find(musicSounds, x => x.name.ToLower() == soundName.ToLower());

        if (sound == null)
        {
            Debug.LogError($"Can't find {soundName} sound in MusicSounds.");
        }
        else
        {
            AudioSource source = FindAudioSource(musicSources, sound.priority);

            if (source == null)
            {
                Debug.Log($"Can't play the sound {sound.name} because there is no more available audio sources and the priority of this sound is lesser than the others.");
            }
            else
            {
                SetupSource(source, sound);
                source.Play();
            }
        }
    }

    /// <summary>
    /// Play a sound just once and with no 3D settings.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    public void PlaySfx(string soundName)
    {
        if (sfxSounds.Length == 0)
        {
            Debug.LogError("There is no Sound in Sfx Sounds.");
            return;
        }

        Sound sound = Array.Find(sfxSounds, x => x.name.ToLower() == soundName.ToLower());

        if (sound == null)
        {
            Debug.LogError($"Can't find {soundName} sound in SfxSounds.");
        }
        else
        {
            AudioSource source = FindAudioSource(sfxSources, sound.priority);

            if (source == null)
            {
                Debug.Log($"Can't play the sound {sound.name} because there is no more available audio sources and the priority of this sound is lesser than the others.");
            }
            else
            {
                SetupSource(source, sound);
                source.Play();
            }
        }
    }

    /// <summary>
    /// Play a sound just once with 3D settings.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    /// <param name="position">The position where to play the sound.</param>
    public void PlaySfx(string soundName, Vector3 position)
    {
        if (sfxSounds.Length == 0)
        {
            Debug.LogError("There is no Sound in Sfx Sounds.");
            return;
        }

        Sound sound = Array.Find(sfxSounds, x => x.name.ToLower() == soundName.ToLower());

        if (sound == null)
        {
            Debug.LogError($"Can't find {soundName} sound in SfxSounds.");
        }
        else
        {
            AudioSource source = FindAudioSource(sfxSources, sound.priority);

            if (source == null)
            {
                Debug.Log($"Can't play the sound {sound.name} because there is no more available audio sources and the priority of this sound is lesser than the others.");
            }
            else
            {
                SetupSource(source, sound);
                SetSourceTo3D(source, position);
                source.Play();
            }
        }
    }

    /// <summary>
    /// Setup the audio source.
    /// </summary>
    /// <param name="source">The audio source to setup.</param>
    /// <param name="sound">The sound that will be played on the audio source.</param>
    private void SetupSource(AudioSource source, Sound sound)
    {
        source.clip = sound.audioClip;
        source.priority = sound.priority;
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
}
