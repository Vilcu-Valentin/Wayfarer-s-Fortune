using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MapMaster : MonoBehaviour
{
    public MapData data;
    public TravelManager travelManager;

    // The roads that the player can see on the map 
    [SerializeField] private List<GameObject> roadGraphics;
    [Header("Player Location")]
    // Ideally, only the getter should be public. It will need to be initialized when loading a save.
    public SettlementData playerLocation;
    [Tooltip("A list of all the real settlements")]
    public List<Settlement> all_settlements;
    [SerializeField] private GameObject playerMarker;
    [SerializeField] private Vector3 playerMarkerOffset;
    [Tooltip("This is used for position data, nothing else")]
    [SerializeField] private List<GameObject> settlements;

    // Color to use for highlighting roads
    [SerializeField] private Color highlightColor = Color.yellow;

    // Stores the original colors of the roads
    private List<Color> originalRoadColors;

    // Adjacency list now stores pairs of SettlementData and the distance to the neighbor
    private Dictionary<SettlementData, List<(SettlementData neighbor, int distance)>> graphAdjList = null;

    private List<SettlementData> currentPath;

    private int timeDifference = 0;

    void Awake()
    {
        // Validate that roadGraphics matches data.roads count
        if (roadGraphics.Count != data.roads.Count)
        {
            Debug.LogError("The number of roadGraphics does not match the number of roads in MapData.");
            return;
        }

        // Initialize the graph and link to existing settlements in the scene
        graphAdjList = new Dictionary<SettlementData, List<(SettlementData, int)>>();

        // Fetch all Settlement components already placed in the scene
        Settlement[] settlementsInScene = FindObjectsOfType<Settlement>();
        foreach (Settlement settlement in settlementsInScene)
        {
            if (!graphAdjList.ContainsKey(settlement.data))
            {
                graphAdjList.Add(settlement.data, new List<(SettlementData, int)>());
            }
        }

        // Set up the adjacency list for roads with distances
        foreach (Roads road in data.roads)
        {
            if (graphAdjList.ContainsKey(road.road1) && graphAdjList.ContainsKey(road.road2))
            {
                graphAdjList[road.road1].Add((road.road2, road.distance));
                graphAdjList[road.road2].Add((road.road1, road.distance));
            }
            else
            {
                Debug.LogWarning($"One or both settlements for the road between {road.road1.name} and {road.road2.name} are missing.");
            }
        }

        // Initialize and store the original colors of the roads
        originalRoadColors = new List<Color>();
        for (int i = 0; i < roadGraphics.Count; i++)
        {
            Renderer renderer = roadGraphics[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                // Ensure each road has its own material instance to avoid shared material issues
                renderer.material = new Material(renderer.material);
                originalRoadColors.Add(renderer.material.color);
            }
            else
            {
                Debug.LogWarning($"Road graphic at index {i} does not have a Renderer component.");
                originalRoadColors.Add(Color.white); // Default color if no renderer is found
            }
        }

        MovePlayerMarker();
    }

    /// <summary>
    /// Returns the minimum total distance in hours between source and destination using road distances.
    /// Returns -1 if a path doesn't exist.
    /// </summary>
    /// <param name="source">Starting settlement.</param>
    /// <param name="destination">Destination settlement.</param>
    /// <returns>Total distance in hours or -1 if no path exists.</returns>
    public int GetDistanceBetween(SettlementData source, SettlementData destination)
    {
        if (source == null || destination == null)
        {
            Debug.LogError("Source or destination is null.");
            return -1;
        }

        if (!graphAdjList.ContainsKey(source) || !graphAdjList.ContainsKey(destination))
        {
            Debug.LogWarning("Source or destination settlement not found in the graph.");
            return -1;
        }

        if (source.Equals(destination))
            return 0;

        // Dijkstra's algorithm implementation using custom PriorityQueue
        Dictionary<SettlementData, int> distances = new Dictionary<SettlementData, int>();
        HashSet<SettlementData> visited = new HashSet<SettlementData>();
        PriorityQueue<SettlementData, int> priorityQueue = new PriorityQueue<SettlementData, int>();

        // Initialize distances
        foreach (var settlement in graphAdjList.Keys)
        {
            distances[settlement] = int.MaxValue;
        }
        distances[source] = 0;
        priorityQueue.Enqueue(source, 0);

        while (priorityQueue.Count > 0)
        {
            SettlementData current = priorityQueue.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            if (current.Equals(destination))
            {
                Debug.Log($"Shortest distance from '{source.name}' to '{destination.name}' is {distances[current]} hours.");
                return distances[current];
            }

            foreach (var (neighbor, distance) in graphAdjList[current])
            {
                if (visited.Contains(neighbor))
                    continue;

                int newDistance = distances[current] + distance;
                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    priorityQueue.Enqueue(neighbor, newDistance);
                    Debug.Log($"Updated distance to '{neighbor.name}' as {newDistance} hours.");
                }
            }
        }

        Debug.Log($"No path found from '{source.name}' to '{destination.name}'.");
        return -1; // No path found.
    }

    /// <summary>
    /// Retrieves the shortest path between source and destination as a list of settlements.
    /// Returns null if no path exists.
    /// </summary>
    /// <param name="source">Starting settlement.</param>
    /// <param name="destination">Destination settlement.</param>
    /// <returns>List of settlements representing the path or null.</returns>
    public List<SettlementData> GetPathBetween(SettlementData source, SettlementData destination)
    {
        if (source == null || destination == null)
        {
            Debug.LogError("Source or destination is null.");
            return null;
        }

        if (!graphAdjList.ContainsKey(source) || !graphAdjList.ContainsKey(destination))
        {
            Debug.LogWarning("Source or destination settlement not found in the graph.");
            return null;
        }

        if (source.Equals(destination))
        {
            return new List<SettlementData> { source };
        }

        // Dijkstra's algorithm with predecessor tracking
        Dictionary<SettlementData, int> distances = new Dictionary<SettlementData, int>();
        Dictionary<SettlementData, SettlementData> predecessors = new Dictionary<SettlementData, SettlementData>();
        HashSet<SettlementData> visited = new HashSet<SettlementData>();
        PriorityQueue<SettlementData, int> priorityQueue = new PriorityQueue<SettlementData, int>();

        foreach (var settlement in graphAdjList.Keys)
        {
            distances[settlement] = int.MaxValue;
        }
        distances[source] = 0;
        priorityQueue.Enqueue(source, 0);

        while (priorityQueue.Count > 0)
        {
            SettlementData current = priorityQueue.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            if (current.Equals(destination))
            {
                // Reconstruct path
                List<SettlementData> path = new List<SettlementData>();
                SettlementData step = destination;

                while (step != null)
                {
                    path.Add(step);
                    if (predecessors.ContainsKey(step))
                        step = predecessors[step];
                    else
                        step = null;
                }

                path.Reverse();
                return path;
            }

            foreach (var (neighbor, distance) in graphAdjList[current])
            {
                if (visited.Contains(neighbor))
                    continue;

                int newDistance = distances[current] + distance;
                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    predecessors[neighbor] = current;
                    priorityQueue.Enqueue(neighbor, newDistance);
                }
            }
        }

        // No path found
        return null;
    }

    public Settlement getPlayerLocation() 
    {
        var location = all_settlements.Find(i => i.data == playerLocation);
        return location;
    }

    /// <summary>
    /// Shorthand for GetDistanceBetween(playerLocation, settlement)
    /// </summary>
    /// <param name="settlement">Destination settlement.</param>
    /// <returns>Total distance in hours or -1 if no path exists.</returns>
    public int GetPlayerDistanceTo(SettlementData settlement)
    {
        return GetDistanceBetween(playerLocation, settlement);
    }

    /// <summary>
    /// Moves the player to a settlement.
    /// </summary>
    /// <param name="destination">Destination settlement.</param>
    /// <returns>True for success and false for failure.</returns>
    public bool MovePlayer(SettlementData destination)
    {
        int distance = GetPlayerDistanceTo(destination);

        playerLocation = destination;
        Debug.Log($"Player moved to '{destination.name}'.");
        RollTravelEvents();

        Debug.Log("Time has been advanced by: " + distance + " but we also have a difference off: " + timeDifference + " final: " + (distance + timeDifference));

        distance = Mathf.RoundToInt(distance / Inventory.Instance.caravanStatistics.speedModifier);
        distance += timeDifference;
        distance = Mathf.Max(distance, 1);
        TimeMaster.Instance().advanceTime(distance);

        ResetRoadColors();
        return true;
    }

    /// <summary>
    /// Moves the player marker to a specified city
    /// </summary>
    /// <param name="location">The city in which the player is located</param>
    public void MovePlayerMarker()
    {
        foreach(var settlement in settlements)
        {
            if (settlement.GetComponent<Settlement>().data == playerLocation)
            {
                playerMarker.transform.position = settlement.transform.position + playerMarkerOffset;
            }
        }
    }

    /// <summary>
    /// Highlights the path the player will take from its location to the destination.
    /// If the destination is the same as the player location, it will not highlight any road.
    /// </summary>
    /// <param name="destination">The destination settlement.</param>
    /// <returns>True if roads were highlighted successfully; false otherwise.</returns>
    public bool HighlighRoads(SettlementData destination)
    {
        // Reset all roads to their original colors
        ResetRoadColors();

        // Get the path from playerLocation to destination
        currentPath = GetPathBetween(playerLocation, destination);

        if (currentPath == null)
        {
            Debug.LogWarning("No path found. No roads to highlight.");
            return false;
        }

        if (currentPath.Count < 2)
        {
            Debug.Log("Destination is the same as the player location. No roads to highlight.");
            return false;
        }

        // Iterate through the path and highlight the corresponding roads
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            SettlementData current = currentPath[i];
            SettlementData next = currentPath[i + 1];

            // Find the road that connects current and next settlements
            int roadIndex = FindRoadIndex(current, next);
            if (roadIndex != -1)
            {
                HighlightRoad(roadIndex);
            }
            else
            {
                Debug.LogWarning($"Road between '{current.name}' and '{next.name}' not found in data.roads.");
            }
        }

        return true;
    }

    /// <summary>
    /// Resets all road graphics to their original colors.
    /// </summary>
    private void ResetRoadColors()
    {
        for (int i = 0; i < roadGraphics.Count; i++)
        {
            Renderer renderer = roadGraphics[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = originalRoadColors[i];
            }
        }
    }

    /// <summary>
    /// Finds the index of the road connecting two settlements.
    /// </summary>
    /// <param name="settlement1">First settlement.</param>
    /// <param name="settlement2">Second settlement.</param>
    /// <returns>Index of the road in data.roads list or -1 if not found.</returns>
    private int FindRoadIndex(SettlementData settlement1, SettlementData settlement2)
    {
        for (int i = 0; i < data.roads.Count; i++)
        {
            Roads road = data.roads[i];
            if ((road.road1 == settlement1 && road.road2 == settlement2) ||
                (road.road1 == settlement2 && road.road2 == settlement1))
            {
                return i;
            }
        }
        return -1; // Road not found
    }

    /// <summary>
    /// Highlights a specific road by its index in roadGraphics.
    /// </summary>
    /// <param name="roadIndex">Index of the road to highlight.</param>
    private void HighlightRoad(int roadIndex)
    {
        if (roadIndex < 0 || roadIndex >= roadGraphics.Count)
        {
            Debug.LogWarning($"Road index {roadIndex} is out of bounds.");
            return;
        }

        Renderer renderer = roadGraphics[roadIndex].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = highlightColor;
        }
    }

    public void RollTravelEvents()
    {
        var allResults = new List<List<EffectResult>>();
        var allEvents = new List<TravelEvents>();

        timeDifference = 0;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            SettlementData current = currentPath[i];
            SettlementData next = currentPath[i + 1];

            // Find the road that connects the current and next settlements
            int roadIndex = FindRoadIndex(current, next);

            if (roadIndex == -1)
            {
                Debug.LogWarning($"Road between '{current.name}' and '{next.name}' not found in data.roads.");
                continue;
            }

            if (!IsValidRoadIndex(roadIndex))
            {
                Debug.LogWarning($"Road index {roadIndex} is out of bounds.");
                return;
            }

            var road = data.roads[roadIndex];
            var selectedEvent = GetRandomTravelEvent(road);

            if (selectedEvent != null)
            {
                var result = selectedEvent.TriggerEvent();

                foreach (var effect in result)
                {
                    if (effect.EffectType == typeof(DurationEffect))
                    {
                        int durationResult = (int)effect.Value;
                        timeDifference += durationResult;
                    }
                }

                allResults.Add(result);
                allEvents.Add(selectedEvent);
            }
        }

        if (allResults.Count > 0)
        {
            travelManager.UseEffects(allResults, allEvents);
        }
    }

    private bool IsValidRoadIndex(int index)
    {
        return index >= 0 && index < roadGraphics.Count;
    }

    private TravelEvents GetRandomTravelEvent(Roads road)
    {
        if (road.travelEvents == null || road.travelEvents.Count == 0)
        {
            Debug.LogWarning("No travel events found for the road.");
            return null;
        }

        int ticketCount = 1;
        var tickets = new List<Vector2>();

        foreach (var travelEvent in road.travelEvents)
        {
            tickets.Add(new Vector2(ticketCount, ticketCount + travelEvent.ticket));
            ticketCount += travelEvent.ticket + 1;
        }

        int drawnTicket = Random.Range(1, ticketCount);

        for (int i = 0; i < tickets.Count; i++)
        {
            if (drawnTicket >= tickets[i].x && drawnTicket <= tickets[i].y)
            {
                return road.travelEvents[i];
            }
        }

        Debug.LogWarning("No travel event matched the drawn ticket.");
        return null;
    }

}
