// Assets/Editor/ConformRoadToTerrainEditor.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ConformRoadToTerrainEditor : EditorWindow
{
    [MenuItem("Tools/Conform Roads To Terrain")]
    public static void ShowWindow()
    {
        GetWindow<ConformRoadToTerrainEditor>("Conform Roads To Terrain");
    }

    // Serialized Fields
    private Terrain terrain;
    private float heightOffset = 0.1f;
    private float maxSlopeDegrees = 10f;
    private float maxEdgeLength = 5f;
    private int maxSubdivisionIterations = 2;
    private string savePath = "Assets/GeneratedRoadMeshes";

    private void OnGUI()
    {
        GUILayout.Label("Conform Roads To Terrain", EditorStyles.boldLabel);

        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        heightOffset = EditorGUILayout.FloatField("Height Offset", heightOffset);
        maxSlopeDegrees = EditorGUILayout.Slider("Max Slope Degrees", maxSlopeDegrees, 1f, 45f);
        maxEdgeLength = EditorGUILayout.FloatField("Max Edge Length", maxEdgeLength);
        maxSubdivisionIterations = EditorGUILayout.IntSlider("Max Subdivision Iterations", maxSubdivisionIterations, 1, 5);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Conform and Save Roads"))
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Terrain object.", "OK");
                return;
            }

            // Ensure save directory exists
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                string parent = Path.GetDirectoryName(savePath);
                string folder = Path.GetFileName(savePath);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EditorUtility.DisplayDialog("Error", $"Parent folder '{parent}' does not exist.", "OK");
                    return;
                }
                AssetDatabase.CreateFolder(parent, folder);
            }

            // Find the selected parent GameObject
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select the parent GameObject containing road sections.", "OK");
                return;
            }

            ConformRoads(Selection.activeGameObject);
        }
    }

    /// <summary>
    /// Adjusts all child road meshes to conform to the terrain's height and saves them as assets.
    /// </summary>
    /// <param name="parent">The parent GameObject containing road sections as children.</param>
    private void ConformRoads(GameObject parent)
    {
        Transform parentTransform = parent.transform;
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            GameObject child = meshFilter.gameObject;
            Mesh originalMesh = meshFilter.sharedMesh;

            if (originalMesh == null)
            {
                Debug.LogWarning($"Child '{child.name}' does not have a valid MeshFilter. Skipping.");
                continue;
            }

            // Duplicate the mesh to avoid modifying the original
            Mesh mesh = Instantiate(originalMesh);
            mesh.name = $"{child.name}_Conformed";

            // Subdivide the mesh based on slope criteria
            Mesh subdividedMesh = SubdivideMesh(mesh, maxSlopeDegrees, maxEdgeLength, maxSubdivisionIterations);

            // Conform the subdivided mesh to the terrain
            ConformMesh(subdividedMesh, child.transform, terrain, heightOffset);

            // Save the subdivided and conformed mesh as an asset
            string assetPath = $"{savePath}/{subdividedMesh.name}.asset";
            AssetDatabase.CreateAsset(subdividedMesh, assetPath);
            AssetDatabase.SaveAssets();

            // Assign the new mesh to the MeshFilter
            meshFilter.mesh = subdividedMesh;

            // Update or add MeshCollider if present
            MeshCollider meshCollider = child.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = subdividedMesh;
            }

            Debug.Log($"Road mesh '{child.name}' conformed and saved to '{assetPath}'.");
        }

        EditorUtility.DisplayDialog("Conform Roads", "All road meshes have been conformed and saved successfully.", "OK");
    }

    /// <summary>
    /// Subdivides a mesh based on maximum slope and edge length criteria.
    /// </summary>
    /// <param name="mesh">The original mesh to subdivide.</param>
    /// <param name="maxSlopeDegrees">Maximum allowed slope in degrees.</param>
    /// <param name="maxEdgeLength">Maximum allowed edge length before subdivision.</param>
    /// <param name="maxIterations">Maximum number of subdivision iterations.</param>
    /// <returns>A new subdivided mesh.</returns>
    private Mesh SubdivideMesh(Mesh mesh, float maxSlopeDegrees, float maxEdgeLength, int maxIterations)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<int> newTriangles = new List<int>();

        // Dictionary to store midpoints to avoid duplicate vertices
        Dictionary<(int, int), int> midPointCache = new Dictionary<(int, int), int>();

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool subdivideFurther = false;
            newTriangles.Clear();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int index0 = triangles[i];
                int index1 = triangles[i + 1];
                int index2 = triangles[i + 2];

                Vector3 v0 = newVertices[index0];
                Vector3 v1 = newVertices[index1];
                Vector3 v2 = newVertices[index2];

                // Calculate slopes of the triangle
                float slope0 = Vector3.Angle(v1 - v0, Vector3.up);
                float slope1 = Vector3.Angle(v2 - v1, Vector3.up);
                float slope2 = Vector3.Angle(v0 - v2, Vector3.up);

                // Calculate edge lengths
                float edgeLength0 = Vector3.Distance(v0, v1);
                float edgeLength1 = Vector3.Distance(v1, v2);
                float edgeLength2 = Vector3.Distance(v2, v0);

                bool needsSubdivision = false;

                // Check if any slope exceeds the maximum allowed
                if (slope0 > maxSlopeDegrees || slope1 > maxSlopeDegrees || slope2 > maxSlopeDegrees)
                {
                    needsSubdivision = true;
                }

                // Check if any edge length exceeds the maximum allowed
                if (edgeLength0 > maxEdgeLength || edgeLength1 > maxEdgeLength || edgeLength2 > maxEdgeLength)
                {
                    needsSubdivision = true;
                }

                if (needsSubdivision)
                {
                    subdivideFurther = true;

                    // Subdivide the triangle by adding midpoints
                    int a = GetMidPoint(index0, index1, newVertices, midPointCache);
                    int b = GetMidPoint(index1, index2, newVertices, midPointCache);
                    int c = GetMidPoint(index2, index0, newVertices, midPointCache);

                    // Create four new triangles
                    newTriangles.Add(index0);
                    newTriangles.Add(a);
                    newTriangles.Add(c);

                    newTriangles.Add(index1);
                    newTriangles.Add(b);
                    newTriangles.Add(a);

                    newTriangles.Add(index2);
                    newTriangles.Add(c);
                    newTriangles.Add(b);

                    newTriangles.Add(a);
                    newTriangles.Add(b);
                    newTriangles.Add(c);
                }
                else
                {
                    // No subdivision needed, retain the original triangle
                    newTriangles.Add(index0);
                    newTriangles.Add(index1);
                    newTriangles.Add(index2);
                }
            }

            triangles = newTriangles.ToArray();

            if (!subdivideFurther)
            {
                break; // No further subdivisions needed
            }
        }

        // Create the new subdivided mesh
        Mesh subdividedMesh = new Mesh();
        subdividedMesh.name = mesh.name; // Keep the same name
        subdividedMesh.vertices = newVertices.ToArray();
        subdividedMesh.triangles = triangles;
        subdividedMesh.RecalculateNormals();
        subdividedMesh.RecalculateBounds();

        return subdividedMesh;
    }

    /// <summary>
    /// Gets the midpoint between two vertices, adding it to the vertex list if not already present.
    /// </summary>
    /// <param name="indexA">Index of the first vertex.</param>
    /// <param name="indexB">Index of the second vertex.</param>
    /// <param name="vertices">List of vertices.</param>
    /// <param name="midPointCache">Cache to store and retrieve midpoints.</param>
    /// <returns>The index of the midpoint vertex.</returns>
    private int GetMidPoint(int indexA, int indexB, List<Vector3> vertices, Dictionary<(int, int), int> midPointCache)
    {
        // Order the indices to ensure consistency
        (int, int) key = indexA < indexB ? (indexA, indexB) : (indexB, indexA);

        if (midPointCache.TryGetValue(key, out int midpointIndex))
        {
            return midpointIndex;
        }

        // Calculate the midpoint
        Vector3 midpoint = (vertices[indexA] + vertices[indexB]) / 2f;
        vertices.Add(midpoint);
        midpointIndex = vertices.Count - 1;

        // Store in cache
        midPointCache[key] = midpointIndex;

        return midpointIndex;
    }

    /// <summary>
    /// Adjusts the Y-values of mesh vertices to conform to the terrain's height.
    /// </summary>
    /// <param name="mesh">The mesh to conform.</param>
    /// <param name="meshTransform">The transform of the mesh GameObject.</param>
    /// <param name="terrain">The Terrain object.</param>
    /// <param name="heightOffset">Offset to raise the road above the terrain.</param>
    private void ConformMesh(Mesh mesh, Transform meshTransform, Terrain terrain, float heightOffset)
    {
        Vector3[] vertices = mesh.vertices;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Convert local vertex position to world position
            Vector3 worldPos = meshTransform.TransformPoint(vertices[i]);

            // Calculate normalized coordinates within the terrain
            float xNormalized = (worldPos.x - terrainPosition.x) / terrainSize.x;
            float zNormalized = (worldPos.z - terrainPosition.z) / terrainSize.z;

            // Clamp normalized coordinates to [0,1] to avoid out-of-bounds
            xNormalized = Mathf.Clamp01(xNormalized);
            zNormalized = Mathf.Clamp01(zNormalized);

            // Get height from terrain at the specified normalized coordinates
            float terrainHeight = terrainData.GetInterpolatedHeight(xNormalized, zNormalized) + terrainPosition.y;

            // Convert terrain height to mesh's local Y
            float localY = (terrainHeight + heightOffset - meshTransform.position.y) / meshTransform.lossyScale.y;

            // Update the Y position of the vertex
            vertices[i].y = localY;
        }

        // Assign the updated vertices back to the mesh
        mesh.vertices = vertices;
    }
}
