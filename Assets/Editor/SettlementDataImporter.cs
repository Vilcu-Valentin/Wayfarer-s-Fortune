using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json; // Import Newtonsoft.Json
using Unity.Plastic.Newtonsoft.Json;

// Serializable classes to match JSON structure for import
[Serializable]
public class SettlementImportData
{
    public string Name;
    public string Occupation;
    public Dictionary<string, float> Goods;
}

public class SettlementDataImporter : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/SettlementData.json";
    private string outputFolder = "Assets/Resources/Settlements";
    private string logFilePath = "Assets/Editor/SettlementImportLog.txt";

    [MenuItem("Tools/Import Settlement Data")]
    public static void ShowWindow()
    {
        GetWindow<SettlementDataImporter>("Import Settlement Data");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Settlement Data from JSON", EditorStyles.boldLabel);

        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        logFilePath = EditorGUILayout.TextField("Log File Path", logFilePath);

        if (GUILayout.Button("Import Settlements"))
        {
            ImportSettlements();
        }
    }

    private void ImportSettlements()
    {
        if (!File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"JSON file not found at path: {jsonFilePath}", "OK");
            return;
        }

        string jsonContent = File.ReadAllText(jsonFilePath);

        List<SettlementImportData> settlements;
        try
        {
            settlements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SettlementImportData>>(jsonContent);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to parse JSON:\n{ex.Message}", "OK");
            return;
        }

        // Create output folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parent = "Assets/Resources";
            string newFolder = "Settlements";
            string guid = AssetDatabase.CreateFolder(parent, newFolder);
            outputFolder = AssetDatabase.GUIDToAssetPath(guid);
        }

        // Prepare log
        StringBuilder logBuilder = new StringBuilder();
        logBuilder.AppendLine("Settlement Import Log");
        logBuilder.AppendLine("=====================");
        logBuilder.AppendLine($"Import Date: {DateTime.Now}");
        logBuilder.AppendLine();

        foreach (var settlement in settlements)
        {
            List<string> missingItems = new List<string>();

            // Create new SettlementData ScriptableObject
            SettlementData settlementData = ScriptableObject.CreateInstance<SettlementData>();
            settlementData.name = settlement.Name; // 'name' field
            settlementData.occupation = settlement.Occupation;
            settlementData.goodsPool = new List<ItemMarketProfile>();

            foreach (var goodsEntry in settlement.Goods)
            {
                string itemName = goodsEntry.Key;
                float basePrice = goodsEntry.Value;

                // Attempt to find the ItemData asset by name
                // Replace spaces with underscores and remove invalid characters
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
                    // Create ItemMarketProfile
                    ItemMarketProfile profile = new ItemMarketProfile
                    {
                        itemData = itemData,
                        basePricePerUnit = basePrice,
                        volatility = 0.05f
                    };
                    settlementData.goodsPool.Add(profile);
                }
                else
                {
                    missingItems.Add(itemName);
                }
            }

            // Generate a unique asset path
            string assetName = SanitizeFileName(settlement.Name);
            string assetPath = Path.Combine(outputFolder, $"{assetName}.asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            // Create the asset
            AssetDatabase.CreateAsset(settlementData, assetPath);

            // Log missing items if any
            if (missingItems.Count > 0)
            {
                logBuilder.AppendLine($"Settlement: {settlement.Name}");
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

        EditorUtility.DisplayDialog("Import Complete", "Settlement data imported successfully!\nCheck the log for missing items.", "OK");
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
