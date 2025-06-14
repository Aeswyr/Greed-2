using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "ScriptableObjects/AudioLibrary", order = 1)]
public class AudioLibrary : ScriptableObject
{
	[SerializeField]
	private AudioEntry[] clips;
	private static Dictionary<string, AudioClip> clipLookup;

	public AudioClip this[string name]
	{
		get
		{
			if (clipLookup == null)
			{
				clipLookup = new();
				foreach (var clip in clips)
					clipLookup.Add(clip.name, clip.clip);
			}
			if (clipLookup.ContainsKey(name))
				return clipLookup[name];
			return null;
		}
	}

	public int Length => clips.Length;

	[Serializable] private struct AudioEntry {
		public string name;
		public AudioClip clip;
	}
}


