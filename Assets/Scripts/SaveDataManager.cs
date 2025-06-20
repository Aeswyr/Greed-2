using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveDataManager
{
    private static string SAVE_PATH = Application.persistentDataPath + "\\saveData.txt";
    private static SaveData saveData;

    public static void Load()
    {
        Debug.Log("Data saved to:\n" + SAVE_PATH);
        string json = null;
        if (File.Exists(SAVE_PATH))
        {
            json = File.ReadAllText(SAVE_PATH);
        }

        if (json != null)
            saveData = JsonUtility.FromJson<SaveData>(json);
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(saveData);
        Directory.CreateDirectory(Application.persistentDataPath);
        if (File.Exists(SAVE_PATH))
        {
            StreamWriter writer = new StreamWriter(SAVE_PATH, false);
            writer.Write(json);
            writer.Close();
        }
        else
        {
            var writer = File.CreateText(SAVE_PATH);
            writer.Write(json);
            writer.Close();
        }
        
        
    }

    public static List<ProfileSaveData> GetProfiles()
    {
        if (saveData.profiles == null)
        {
            saveData.profiles = new();
        }
        return saveData.profiles;
    }

    public static void AddProfile(string name, string settings)
    {
        saveData.profiles.Add(new()
        {
            name = name,
            settings = settings
        });
        Save();
    }

    public static void UpdateProfile(string name, string settings)
    {
        for (int i = 0; i < saveData.profiles.Count; i++)
        {
            var profile = saveData.profiles[i];
            if (profile.name.Equals(name))
            {
                profile.settings = settings;
                saveData.profiles[i] = profile;
            }
        }
        Save();
    }

    public static void UpdateMasterVolume(float amt)
    {
        saveData.masterVolume = amt;
        saveData.masterVolumeDirty = true;
    }

    public static void UpdateGeneralVolume(float amt)
    {
        saveData.generalVolume = amt;
        saveData.generalVolumeDirty = true;
    }

    public static void UpdateMusicVolume(float amt)
    {
        saveData.musicVolume = amt;
        saveData.musicVolumeDirty = true;
    }

    public static float GetMasterVolume()
    {
        if (saveData.masterVolumeDirty)
            return saveData.masterVolume;
        return 1;
    }

    public static float GetGeneralVolume()
    {
        if (saveData.generalVolumeDirty)
            return saveData.generalVolume;
        return 0.5f;
    }

    public static float GetMusicVolume()
    {
        if (saveData.musicVolumeDirty)
            return saveData.musicVolume;
        return 0.5f;
    }

    [Serializable]
    private struct SaveData
    {
        public List<ProfileSaveData> profiles;

        public float masterVolume;
        public bool masterVolumeDirty;
        public float generalVolume;
        public bool generalVolumeDirty;
        public float musicVolume;
        public bool musicVolumeDirty;
    }
    
    
}

[Serializable] public struct ProfileSaveData {
    public string name;
    public string settings;
}