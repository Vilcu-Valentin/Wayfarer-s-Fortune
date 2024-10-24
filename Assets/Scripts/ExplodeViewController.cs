using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ExplodedViewController
{
    private bool isExploded;
    private readonly float additionalLayerOffset;
    private Dictionary<int, float> layerHeights;
    private Dictionary<GameObject, Vector3> originalPositions;
    private Dictionary<GameObject, Vector3> targetPositions;
    private bool isTransitioning;

    private readonly float transitionDuration = 0.5f;
    private float currentTransitionTime;

    public bool IsExploded => isExploded;
    public bool IsTransitioning => isTransitioning;

    public ExplodedViewController(float additionalLayerOffset = 0f)
    {
        this.additionalLayerOffset = additionalLayerOffset;
        this.layerHeights = new Dictionary<int, float>();
        this.originalPositions = new Dictionary<GameObject, Vector3>();
        this.targetPositions = new Dictionary<GameObject, Vector3>();
        this.isTransitioning = false;
    }

    public void ToggleExplodedView(List<Item> items, float cellSize)
    {
        if (isTransitioning) return;

        isExploded = !isExploded;
        PrepareTransition(items, cellSize);
        currentTransitionTime = 0f;
        isTransitioning = true;
    }

    private void PrepareTransition(List<Item> items, float cellSize)
    {
        CalculateLayerHeights(items, cellSize);
        targetPositions.Clear();

        foreach (var item in items.OrderBy(i => i.Position.y))
        {
            if (item?.ObjectReference == null) continue;

            // Store current position if it's not already an original position
            if (!originalPositions.ContainsKey(item.ObjectReference))
            {
                originalPositions[item.ObjectReference] = item.ObjectReference.transform.position;
            }

            // Calculate target position
            Vector3 targetPosition = originalPositions[item.ObjectReference];
            if (isExploded && item.Position.y > 0)
            {
                float previousLayerHeight = 0;
                for (int y = 0; y < item.Position.y; y++)
                {
                    previousLayerHeight += (layerHeights[y] + additionalLayerOffset) * cellSize;
                }
                targetPosition.y += previousLayerHeight;
            }

            targetPositions[item.ObjectReference] = targetPosition;
        }
    }

    public void UpdateTransition()
    {
        if (!isTransitioning) return;

        currentTransitionTime += Time.deltaTime;
        float t = currentTransitionTime / transitionDuration;

        if (t >= 1f)
        {
            CompleteTransition();
            return;
        }

        float smoothT = Mathf.SmoothStep(0, 1, t);

        foreach (var kvp in targetPositions)
        {
            if (kvp.Key == null) continue;

            Vector3 startPos = kvp.Key.transform.position;
            Vector3 endPos = isExploded ? kvp.Value : originalPositions[kvp.Key];

            kvp.Key.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
        }
    }

    private void CompleteTransition()
    {
        foreach (var kvp in targetPositions)
        {
            if (kvp.Key == null) continue;
            kvp.Key.transform.position = isExploded ? kvp.Value : originalPositions[kvp.Key];
        }

        isTransitioning = false;
    }

    private void CalculateLayerHeights(List<Item> items, float cellSize)
    {
        layerHeights.Clear();

        // Initialize all possible layers with height 0
        for (int i = 0; i < items.Max(item => item.Position.y + item.Size.y); i++)
        {
            layerHeights[i] = 0;
        }

        // Calculate maximum height for each layer
        foreach (var item in items)
        {
            int baseLayer = item.Position.y;
            float itemHeight = item.Size.y;

            layerHeights[baseLayer] = Mathf.Max(layerHeights[baseLayer], itemHeight);
        }
    }

    public Vector3 TranslateExplodedToGridPosition(GameObject obj, float cellSize)
    {
        if (!isExploded || obj == null || !originalPositions.ContainsKey(obj))
            return obj.transform.position; // Return original if not exploded

        // Get the original grid position before explosion
        Vector3 originalPosition = originalPositions[obj];

        // Calculate how much the object has been offset in the exploded view
        float explodedOffset = obj.transform.position.y - originalPosition.y;

        // Reverse the explosion offset by subtracting it from the current position
        Vector3 adjustedPosition = obj.transform.position;
        adjustedPosition.y -= explodedOffset;

        return adjustedPosition;
    }


    public void Reset()
    {
        if (isExploded || isTransitioning)
        {
            isExploded = false;
            foreach (var kvp in originalPositions)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.transform.position = kvp.Value;
                }
            }
        }
        isTransitioning = false;
        currentTransitionTime = 0f;
    }
}