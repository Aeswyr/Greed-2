using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : NetworkSingleton<SFXManager>
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioLibrary soundLib;

    public enum AudioMode {
        ONESHOT, SINGLE, MUSIC
    } 
    public void PlaySound(string name, AudioMode mode = AudioMode.ONESHOT)
    {
        var sound = soundLib[name];
        if (sound != null)
        {
            switch (mode)
            {
                case AudioMode.ONESHOT:
                    source.PlayOneShot(sound);
                    break;
                case AudioMode.SINGLE:
                    source.clip = sound;
                    source.Play();
                    break;
                case AudioMode.MUSIC:
                    break;
            }
            
        }
        else
        {
            Debug.LogWarning($"Attempted to play sound with name {name}. Sound does not exist");
        }
    }
}
