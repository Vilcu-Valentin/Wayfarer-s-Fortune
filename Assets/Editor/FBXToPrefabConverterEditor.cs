using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FBXToPrefabConverterEditor : EditorWindow
{
    // Variables for input and output paths
    private string inputFolderPath = "Assets/Models"; // Default input folder
    private string outputFolderPath = "Assets/Prefabs"; // Default output folder

    [MenuItem("Tools/FBX to Prefab Converter")]
    public static void ShowWindow()
    {
        GetWindow<FBXToPrefabConverterEditor>("FBX to Prefab Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX to Prefab Converter", EditorStyles.boldLabel);

        // Input Folder Selection
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Input Folder:", GUILayout.Width(80));
        inputFolderPath = EditorGUILayout.TextField(inputFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Input Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    inputFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // Output Folder Selection
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Output Folder:", GUILayout.Width(80));
        outputFolderPath = EditorGUILayout.TextField(outputFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    outputFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Convert Button
        if (GUILayout.Button("Convert FBX to Prefabs", GUILayout.Height(40)))
        {
            ConvertAllFBXToPrefabs();
        }
    }

    private void ConvertAllFBXToPrefabs()
    {
        // Validate input folder
        if (!AssetDatabase.IsValidFolder(inputFolderPath))
        {
            EditorUtility.DisplayDialog("Invalid Input Folder", "The specified input folder does not exist.", "OK");
            return;
        }

        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(outputFolderPath))
        {
            // Create the output folder if it doesn't exist
            Directory.CreateDirectory(Path.Combine(Application.dataPath, outputFolderPath.Substring("Assets/".Length)));
            AssetDatabase.Refresh();
        }

        // Find all FBX files in the input folder and subfolders
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { inputFolderPath });
        List<string> fbxPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetExtension(path).ToLower() == ".fbx")
            {
                fbxPaths.Add(path);
            }
        }

        if (fbxPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No FBX Files Found", "No FBX files were found in the specified input folder.", "OK");
            return;
        }

        int successCount = 0;
        int failCount = 0;

        foreach (string fbxPath in fbxPaths)
        {
            try
            {
                // Load the FBX as a GameObject
                GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxAsset == null)
                {
                    Debug.LogWarning($"Failed to load FBX at path: {fbxPath}");
                    failCount++;
                    continue;
                }

                // Instantiate the FBX in the scene
                GameObject instance = Instantiate(fbxAsset);
                instance.name = fbxAsset.name;

                // Optionally, you can perform unpacking or modifications here
                // For example: PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                // Determine the relative path from input folder to the FBX
                string relativePath = Path.GetDirectoryName(fbxPath.Substring(inputFolderPath.Length + 1));
                string prefabFolderPath = Path.Combine(outputFolderPath, relativePath).Replace("\\", "/");

                // Ensure the prefab folder exists
                if (!AssetDatabase.IsValidFolder(prefabFolderPath))
                {
                    CreateFolderRecursively(prefabFolderPath);
                }

                // Define the prefab path
                string prefabPath = Path.Combine(prefabFolderPath, fbxAsset.name + ".prefab").Replace("\\", "/");

                // Create the prefab
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
#else
                PrefabUtility.CreatePrefab(prefabPath, instance);
#endif
                successCount++;

                // Destroy the instantiated GameObject from the scene
                DestroyImmediate(instance);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing FBX at path: {fbxPath}\n{ex.Message}");
                failCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Conversion Complete", $"Successfully converted {successCount} FBX files to prefabs.\nFailed: {failCount}", "OK");
    }

    /// <summary>
    /// Creates folders recursively based on the provided path.
    /// </summary>
    /// <param name="folderPath">The folder path to create.</param>
    private void CreateFolderRecursively(string folderPath)
    {
        string[] folders = folderPath.Split('/');
        string currentPath = folders[0]; // Start with the first folder (should be "Assets")

        for (int i = 1; i < folders.Length; i++)
        {
            string folder = folders[i];
            currentPath = Path.Combine(currentPath, folder).Replace("\\", "/");

            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                string parent = Path.GetDirectoryName(currentPath).Replace("\\", "/");
                string newFolderName = folder;
                AssetDatabase.CreateFolder(parent, newFolderName);
            }
        }
    }

}
