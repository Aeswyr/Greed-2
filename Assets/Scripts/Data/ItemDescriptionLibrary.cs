using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ItemDescriptionLibrary", menuName = "ScriptableObjects/ItemDescriptionLibrary", order = 1)]
public class ItemDescriptionLibrary : ScriptableObject
{
    [SerializeField] private ItemDesc[] items;
    private static Dictionary<string, string> nameLookup;
    private static Dictionary<string, string> descLookup;

    public string GetName(PickupType type)
    {
        if (nameLookup == null)
        {
            nameLookup = new();
            foreach (var item in items)
                nameLookup.Add(item.type, item.name);
        }
        if (nameLookup.ContainsKey(type.ToString()))
            return nameLookup[type.ToString()];
        return null;
    }

    public string GetDesc(PickupType type)
    {
        if (descLookup == null)
        {
            descLookup = new();
            foreach (var item in items)
                descLookup.Add(item.type, item.desc);
        }
        if (descLookup.ContainsKey(type.ToString()))
            return descLookup[type.ToString()];
        return null;
    }

    [Serializable] private struct ItemDesc
    {
        public string type;
        public string name;
        public string desc;
    }
}
