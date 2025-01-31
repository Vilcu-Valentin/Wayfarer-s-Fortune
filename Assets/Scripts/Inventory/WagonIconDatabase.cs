// WagonIconDatabase.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WagonIconDatabase", menuName = "Databases/WagonIconDatabase")]
public class WagonIconDatabase : ScriptableObject
{
    [Serializable]
    public class WagonIconEntry
    {
        public string prefabPath; // e.g., "SWagon"
        public Sprite icon;
    }

    public List<WagonIconEntry> entries = new List<WagonIconEntry>();

    private Dictionary<string, Sprite> prefabToIconMap;

    // Initialize the dictionary for quick lookup
    public void Initialize()
    {
        prefabToIconMap = new Dictionary<string, Sprite>();
        foreach (var entry in entries)
        {
            if (!prefabToIconMap.ContainsKey(entry.prefabPath))
            {
                prefabToIconMap.Add(entry.prefabPath, entry.icon);
            }
        }
    }

    // Retrieve the icon based on prefab path
    public Sprite GetIcon(string prefabPath)
    {
        if (prefabToIconMap == null)
            Initialize();

        if (prefabToIconMap.TryGetValue(prefabPath, out Sprite icon))
        {
            return icon;
        }
        Debug.LogWarning($"Icon not found for prefab path: {prefabPath}");
        return null; // Optionally, return a default icon
    }
}
