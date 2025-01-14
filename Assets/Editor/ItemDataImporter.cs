using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Serializable classes to match JSON structure
[Serializable]
public class ItemList
{
    public List<SerializedItem> Goods;
    public List<SerializedItem> Fluids;
    public List<SerializedItem> Livestock;
}

[Serializable]
public class SerializedItem
{
    public string name;
    public int size;
    public int weight;
}

// Editor Window for importing items
public class ItemDataImporter : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/ItemData.json";
    private string outputFolder = "Assets/Resources/Items";

    [MenuItem("Tools/Import SerializedItem Data")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataImporter>("Import SerializedItem Data");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import SerializedItem Data from JSON", EditorStyles.boldLabel);

        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Import Items"))
        {
            ImportItems();
        }
    }

    private void ImportItems()
    {
        if (!File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"JSON file not found at path: {jsonFilePath}", "OK");
            return;
        }

        string jsonContent = File.ReadAllText(jsonFilePath);
        ItemList itemList = JsonUtility.FromJson<ItemList>(jsonContent);

        // Create output folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parent = "Assets/Resources";
            string newFolder = "Items";
            string guid = AssetDatabase.CreateFolder(parent, newFolder);
            outputFolder = AssetDatabase.GUIDToAssetPath(guid);
        }

        // Process each category
        ProcessCategory(itemList.Goods, "Goods");
        ProcessCategory(itemList.Fluids, "Fluids");
        ProcessCategory(itemList.Livestock, "Livestock");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Import Complete", "SerializedItem data imported successfully!", "OK");
    }

    private void ProcessCategory(List<SerializedItem> items, string category)
    {
        foreach (var item in items)
        {
            // Create new ItemData ScriptableObject
            ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
            itemData.item_name = item.name;
            itemData.weight = item.weight;

            if (item.size > 0)
            {
                itemData.size = item.size;
            }
            else
            {
                itemData.size = 0;
            }


            if (category == "Good") itemData.store_type = StorageType.Good;
            else if (category == "Fluids") itemData.store_type = StorageType.Fluid;
            else if (category == "Livestock") itemData.store_type = StorageType.Livestock;

            // Generate a unique asset path
            string assetName = SanitizeFileName(item.name);
            string assetPath = Path.Combine(outputFolder, $"{assetName}.asset");

            // Ensure unique asset name
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            // Create the asset
            AssetDatabase.CreateAsset(itemData, assetPath);
        }
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
