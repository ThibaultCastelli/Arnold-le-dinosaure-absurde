using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "DefaultSound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    public new string name;
    public AudioClip[] audioClips;
    [Range(0f, 1f)] public float volume = 1;
    [Range(0, 256)] public int priority = 128;
    [Range(-1f, 1f)] public float pan = 0;
    public bool loop;
    public AudioMixerGroup mixerGroup;
}
