using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "DefaultSound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    [Header("General infos")]
    public new string name;
    public SoundType type;

    [Header("Sounds")]
    public AudioClip[] audioClips;

    [Header("Properties")]
    [Range(0f, 1f)] public float volume = 1;
    [Range(0, 256)] public int priority = 128;
    [Range(-1f, 1f)] public float pan = 0;
    public bool loop;
    public AudioMixerGroup mixerGroup;

    /// <summary>
    /// Play the sound with no 3D properties.
    /// </summary>
    public void Play()
    {
        switch(type)
        {
            case SoundType.Music:
                SoundManager.Instance.PlayMusic(name);
                break;
            case SoundType.Sfx:
                SoundManager.Instance.PlaySfx(name);
                break;
        }
    }
}
