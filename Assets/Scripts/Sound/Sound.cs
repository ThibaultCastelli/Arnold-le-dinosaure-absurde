using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "DefaultSound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    public new string name;
    public AudioClip audioClip;
    [Range(0, 256)] public int priority = 128;
    public bool loop;
    public AudioMixerGroup mixerGroup;
}
