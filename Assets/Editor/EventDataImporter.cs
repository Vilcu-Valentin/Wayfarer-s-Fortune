using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

[Serializable]
public class EventImportData
{
    public string Name;
    public string Description;
    public int Duration;
    public int LeadTime;
    public int DeathTime;
    public float[] SeasonalModifiers;
    public Dictionary<string, int> AffectedGoods;
}

public class EventDataImporter : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/Events/EventData.json";
    private string outputFolder = "Assets/Resources/Events";
    private string logFilePath = "Assets/Editor/EventImportLog.txt";

    [MenuItem("Tools/Import Event Data")]
    public static void ShowWindow()
    {
        GetWindow<EventDataImporter>("Import Event Data");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Event Data from JSON", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // JSON File Path
        GUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75)))
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select Event JSON File", "Assets/Resources/Events", "json");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    jsonFilePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a file within the Assets folder.", "OK");
                }
            }
        }
        GUILayout.EndHorizontal();

        // Output Folder
        GUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets/Resources/Events", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a folder within the Assets folder.", "OK");
                }
            }
        }
        GUILayout.EndHorizontal();

        // Log File Path
        GUILayout.BeginHorizontal();
        logFilePath = EditorGUILayout.TextField("Log File Path", logFilePath);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75)))
        {
            string selectedPath = EditorUtility.SaveFilePanel("Select Log File Path", "Assets/Editor", "EventImportLog", "txt");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    logFilePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a path within the Assets folder.", "OK");
                }
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Import Events"))
        {
            ImportEvents();
        }
    }

    private void ImportEvents()
    {
        if (!File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"JSON file not found at path: {jsonFilePath}", "OK");
            return;
        }

        string jsonContent = File.ReadAllText(jsonFilePath);

        List<EventImportData> events;
        try
        {
            events = JsonConvert.DeserializeObject<List<EventImportData>>(jsonContent);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to parse JSON:\n{ex.Message}", "OK");
            return;
        }

        // Create output folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parentFolder = "Assets/Resources";
            string newFolderName = "Events";
            string guid = AssetDatabase.CreateFolder(parentFolder, newFolderName);
            outputFolder = AssetDatabase.GUIDToAssetPath(guid);
        }

        // Prepare log
        StringBuilder logBuilder = new StringBuilder();
        logBuilder.AppendLine("Event Import Log");
        logBuilder.AppendLine("================");
        logBuilder.AppendLine($"Import Date: {DateTime.Now}");
        logBuilder.AppendLine();

        foreach (var evt in events)
        {
            List<string> missingItems = new List<string>();

            // Create new EventData ScriptableObject
            EventData eventData = ScriptableObject.CreateInstance<EventData>();
            eventData.name = evt.Name; // 'name' field
            eventData.description = evt.Description;
            eventData.duration = evt.Duration;
            eventData.leadTime = evt.LeadTime;
            eventData.deathTime = evt.DeathTime;
            eventData.seasonalModifiers = evt.SeasonalModifiers.ToArray();
            eventData.affectedGoods = new List<EventEffectData>();

            foreach (var goodsEntry in evt.AffectedGoods)
            {
                string itemName = goodsEntry.Key;
                int strength = goodsEntry.Value;

                // Attempt to find the ItemData asset by name
                string sanitizedItemName = SanitizeFileName(itemName);
                ItemData itemData = Resources.Load<ItemData>($"Items/{sanitizedItemName}");

                if (itemData == null)
                {
                    // Try to find by item_name directly (case-insensitive)
                    ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
                    foreach (var itm in allItems)
                    {
                        if (string.Equals(itm.item_name, itemName, StringComparison.OrdinalIgnoreCase))
                        {
                            itemData = itm;
                            break;
                        }
                    }
                }

                if (itemData != null)
                {
                    // Create EventEffectData
                    EventEffectData effectData = new EventEffectData
                    {
                        item = itemData,
                        strength = Mathf.Clamp(strength, -10, 10) // Ensuring strength is within the specified range
                    };
                    eventData.affectedGoods.Add(effectData);
                }
                else
                {
                    missingItems.Add(itemName);
                }
            }

            // Generate a unique asset path
            string assetName = SanitizeFileName(evt.Name);
            string assetPath = Path.Combine(outputFolder, $"{assetName}.asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            // Create the asset
            AssetDatabase.CreateAsset(eventData, assetPath);

            // Log missing items if any
            if (missingItems.Count > 0)
            {
                logBuilder.AppendLine($"Event: {evt.Name}");
                logBuilder.AppendLine("Missing Items:");
                foreach (var missing in missingItems)
                {
                    logBuilder.AppendLine($" - {missing}");
                }
                logBuilder.AppendLine();
            }
        }

        // Save and refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Write log file
        try
        {
            // Ensure the Editor folder exists for the log file
            string editorFolder = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(editorFolder))
            {
                Directory.CreateDirectory(editorFolder);
            }

            File.WriteAllText(logFilePath, logBuilder.ToString());
            AssetDatabase.ImportAsset(logFilePath);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to write log file:\n{ex.Message}", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Import Complete", "Event data imported successfully!\nCheck the log for missing items.", "OK");
    }

    // Helper method to sanitize file names
    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Replace(' ', '_');
    }
}