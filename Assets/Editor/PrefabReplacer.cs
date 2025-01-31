using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PrefabReplacer : EditorWindow
{
    // Variables for source and replacement prefab paths
    private string sourcePrefabsPath = "Assets/Prefabs/CityPrefabs"; // Default source folder
    private string replacementPrefabsPath = "Assets/Prefabs/Houses"; // Default replacement folder

    // Dictionary to map prefab names to their paths for quick lookup
    private Dictionary<string, string> replacementPrefabDict = new Dictionary<string, string>();

    // Regex pattern to detect and remove trailing " (number)"
    private static readonly Regex trailingNumberRegex = new Regex(@"^(.*?)(\s*\(\d+\))?$");

    [MenuItem("Tools/Prefab Replacer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabReplacer>("Prefab Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Replacer Tool", EditorStyles.boldLabel);

        // Source Prefabs Folder Selection
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Source Prefabs:", GUILayout.Width(100));
        sourcePrefabsPath = EditorGUILayout.TextField(sourcePrefabsPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Source Prefabs Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    sourcePrefabsPath = "Assets" + selectedPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // Replacement Prefabs Folder Selection
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Replacement Prefabs:", GUILayout.Width(100));
        replacementPrefabsPath = EditorGUILayout.TextField(replacementPrefabsPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Replacement Prefabs Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    replacementPrefabsPath = "Assets" + selectedPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Replace Button
        if (GUILayout.Button("Replace Prefabs", GUILayout.Height(40)))
        {
            ReplacePrefabsInSource();
        }

        GUILayout.Space(10);
    }

    private void ReplacePrefabsInSource()
    {
        // Validate source and replacement folders
        if (!AssetDatabase.IsValidFolder(sourcePrefabsPath))
        {
            EditorUtility.DisplayDialog("Invalid Source Folder", "The specified source prefab folder does not exist.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(replacementPrefabsPath))
        {
            EditorUtility.DisplayDialog("Invalid Replacement Folder", "The specified replacement prefab folder does not exist.", "OK");
            return;
        }

        // Build a dictionary for quick lookup of replacement prefabs by name
        BuildReplacementPrefabDictionary();

        if (replacementPrefabDict.Count == 0)
        {
            EditorUtility.DisplayDialog("No Replacement Prefabs Found", "No prefabs were found in the replacement folder.", "OK");
            return;
        }

        // Find all prefabs in the source folder and subfolders
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { sourcePrefabsPath });
        List<string> sourcePrefabPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            sourcePrefabPaths.Add(path);
        }

        if (sourcePrefabPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No Source Prefabs Found", "No prefabs were found in the source folder.", "OK");
            return;
        }

        int totalPrefabs = sourcePrefabPaths.Count;
        int totalReplacements = 0;
        int totalFailures = 0;

        // Begin progress bar
        EditorUtility.DisplayProgressBar("Replacing Prefabs", "Processing...", 0f);

        for (int i = 0; i < sourcePrefabPaths.Count; i++)
        {
            string prefabPath = sourcePrefabPaths[i];

            // Update progress bar
            float progress = (float)i / totalPrefabs;
            EditorUtility.DisplayProgressBar("Replacing Prefabs", $"Processing {Path.GetFileName(prefabPath)}...", progress);

            try
            {
                // Load the source prefab
                GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (sourcePrefab == null)
                {
                    Debug.LogWarning($"Failed to load prefab at path: {prefabPath}");
                    totalFailures++;
                    continue;
                }

                // Instantiate the prefab in memory (not in the scene)
                GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
                if (prefabInstance == null)
                {
                    Debug.LogWarning($"Failed to instantiate prefab at path: {prefabPath}");
                    totalFailures++;
                    continue;
                }

                bool prefabModified = false;

                // Collect all first-level children into a separate list to avoid modifying the collection during iteration
                List<Transform> children = new List<Transform>();
                foreach (Transform child in prefabInstance.transform)
                {
                    children.Add(child);
                }

                // Iterate through the copied list of children
                foreach (Transform child in children)
                {
                    string originalChildName = child.name;
                    string normalizedChildName = NormalizeName(originalChildName);

                    if (replacementPrefabDict.ContainsKey(normalizedChildName))
                    {
                        string replacementPrefabPath = replacementPrefabDict[normalizedChildName];
                        GameObject replacementPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(replacementPrefabPath);

                        if (replacementPrefab != null)
                        {
                            // Preserve the original child's transform
                            Vector3 originalPosition = child.localPosition;
                            Quaternion originalRotation = child.localRotation;
                            Vector3 originalScale = child.localScale;

                            // Replace the child with the replacement prefab
                            GameObject replacementInstance = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
                            if (replacementInstance != null)
                            {
                                replacementInstance.name = replacementPrefab.name;
                                replacementInstance.transform.SetParent(prefabInstance.transform);
                                replacementInstance.transform.localPosition = originalPosition;
                                replacementInstance.transform.localRotation = originalRotation;
                                replacementInstance.transform.localScale = originalScale;

                                // Destroy the original child
                                DestroyImmediate(child.gameObject);

                                prefabModified = true;
                                totalReplacements++;
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to instantiate replacement prefab: {replacementPrefabPath}");
                                totalFailures++;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Replacement prefab not found at path: {replacementPrefabPath}");
                            totalFailures++;
                        }
                    }
                }

                if (prefabModified)
                {
                    // Apply the changes to the prefab asset
#if UNITY_2018_3_OR_NEWER
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
#else
                    PrefabUtility.ReplacePrefab(prefabInstance, sourcePrefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
                }

                // Destroy the in-memory instance
                DestroyImmediate(prefabInstance);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing prefab at path: {prefabPath}\n{ex.Message}");
                totalFailures++;
            }
        }

        // End progress bar
        EditorUtility.ClearProgressBar();

        // Refresh the Asset Database
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Display completion dialog
        EditorUtility.DisplayDialog(
            "Prefab Replacement Complete",
            $"Processed {totalPrefabs} prefabs.\nReplaced {totalReplacements} child GameObjects.\nFailed: {totalFailures}",
            "OK"
        );
    }

    /// <summary>
    /// Builds a dictionary mapping prefab names to their asset paths for quick lookup.
    /// </summary>
    private void BuildReplacementPrefabDictionary()
    {
        replacementPrefabDict.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { replacementPrefabsPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            if (!replacementPrefabDict.ContainsKey(prefabName))
            {
                replacementPrefabDict.Add(prefabName, path);
            }
            else
            {
                Debug.LogWarning($"Duplicate prefab name detected: {prefabName}. Only the first occurrence will be used.");
            }
        }
    }

    /// <summary>
    /// Normalizes the GameObject name by removing any trailing " (number)" pattern.
    /// For example, "building_archeryrange_blue (1)" becomes "building_archeryrange_blue".
    /// </summary>
    /// <param name="name">Original GameObject name.</param>
    /// <returns>Normalized name without trailing numbers.</returns>
    private string NormalizeName(string name)
    {
        Match match = trailingNumberRegex.Match(name);
        if (match.Success && match.Groups.Count > 1)
        {
            string normalized = match.Groups[1].Value;
            if (normalized != name)
            {
                // Optional: Log normalization
                Debug.Log($"Normalized name from '{name}' to '{normalized}'.");
            }
            return normalized;
        }
        return name;
    }
}
