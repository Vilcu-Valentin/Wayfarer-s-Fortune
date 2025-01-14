using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool to export MapData ScriptableObject to JSON, including only the names of road1 and road2.
/// </summary>
public class MapDataExporter : EditorWindow
{
    private MapData mapDataToExport;
    private string exportPath = "Assets/ExportedJSON/MapData.json";

    // Define the data structures for export
    [System.Serializable]
    private class RoadExport
    {
        public string road1Name;
        public string road2Name;
        public int distance;
    }

    [System.Serializable]
    private class MapDataExport
    {
        public List<RoadExport> roads;
    }

    [MenuItem("Tools/Export MapData to JSON")]
    public static void ShowWindow()
    {
        GetWindow<MapDataExporter>("Export MapData to JSON");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export MapData to JSON", EditorStyles.boldLabel);

        // Object field to select the MapData ScriptableObject
        mapDataToExport = (MapData)EditorGUILayout.ObjectField(
            "MapData",
            mapDataToExport,
            typeof(MapData),
            false
        );

        // Input field for the export path
        exportPath = EditorGUILayout.TextField("Export Path", exportPath);

        GUILayout.Space(10);

        if (GUILayout.Button("Export as JSON"))
        {
            ExportToJson();
        }
    }

    private void ExportToJson()
    {
        if (mapDataToExport == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a MapData ScriptableObject to export.", "OK");
            return;
        }

        // Prepare the export directory
        string directoryPath = Path.GetDirectoryName(exportPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Create the export data
        MapDataExport exportData = new MapDataExport();
        exportData.roads = new List<RoadExport>();

        foreach (var road in mapDataToExport.roads)
        {
            // Ensure that road1 and road2 are not null
            string road1Name = road.road1 != null ? road.road1.name : "Undefined";
            string road2Name = road.road2 != null ? road.road2.name : "Undefined";

            RoadExport roadExport = new RoadExport
            {
                road1Name = road1Name,
                road2Name = road2Name,
                distance = road.distance
            };

            exportData.roads.Add(roadExport);
        }

        // Serialize the export data to JSON
        string json = JsonUtility.ToJson(exportData, prettyPrint: true);

        // Write the JSON to the file
        try
        {
            File.WriteAllText(exportPath, json);
            EditorUtility.DisplayDialog("Success", $"MapData exported to:\n{exportPath}", "OK");
        }
        catch (IOException e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to write JSON file:\n{e.Message}", "OK");
        }

        // Refresh the AssetDatabase to show the new file
        AssetDatabase.Refresh();
    }
}
