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


    [Serializable] private struct SaveData
    {
        public List<ProfileSaveData> profiles;
    }
    
    
}

[Serializable] public struct ProfileSaveData {
    public string name;
    public string settings;
}