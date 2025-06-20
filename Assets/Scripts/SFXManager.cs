using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : NetworkSingleton<SFXManager>
{
    [SerializeField] private AudioSource[] sources;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioLibrary soundLib;

    public enum AudioMode {
        ONESHOT, SINGLE, MUSIC
    } 
    public void PlaySound(string name, AudioMode mode = AudioMode.ONESHOT)
    {
        var sound = soundLib[name];

        var source = sources[0];
        foreach (var cur in sources)
        {
            if (!cur.isPlaying)
            {
                source = cur;
                break;
            }
        }
 
        if (sound != null)
        {
            switch (mode)
            {
                case AudioMode.ONESHOT:
                    source.PlayOneShot(sound);
                    break;
                case AudioMode.SINGLE:
                    foreach (var cur in sources)
                    {
                        if (cur.clip == sound)
                        {
                            source = cur;
                            break;
                        }
                    }

                    source.clip = sound;
                    source.Play();
                    break;
                case AudioMode.MUSIC:
                    musicSource.clip = sound;
                    musicSource.Play();
                    break;
            }

        }
        else
        {
            Debug.LogWarning($"Attempted to play sound with name {name}. Sound does not exist");
        }
    }
}
