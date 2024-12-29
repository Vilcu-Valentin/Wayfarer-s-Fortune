using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaster : MonoBehaviour
{
    public MapData data;

    // This would ideally only have the getter public, but it will still need to be initialized somehow when loading a save,
    // and it would seem constructors are a no-go.
    public SettlementData playerLocation;

    private Dictionary<SettlementData, Settlement> settlementObjDict = null;
    private Dictionary<SettlementData, List<SettlementData>> graphAdjList = null;

    private InputMaster inputMaster = null;

    void Awake()
    {
        // Initialize the graph and link to existing settlements in the scene
        settlementObjDict = new Dictionary<SettlementData, Settlement>();
        graphAdjList = new Dictionary<SettlementData, List<SettlementData>>();

        // Fetch all Settlement components already placed in the scene
        Settlement[] settlementsInScene = FindObjectsOfType<Settlement>();
        foreach (Settlement settlement in settlementsInScene)
        {
            if (!settlementObjDict.ContainsKey(settlement.data))
            {
                settlementObjDict.Add(settlement.data, settlement);
                graphAdjList.Add(settlement.data, new List<SettlementData>());
            }
        }

        // Set up the adjacency list for roads
        foreach (SerializablePair<SettlementData, SettlementData> road in data.roads)
        {
            if (graphAdjList.ContainsKey(road.value1) && graphAdjList.ContainsKey(road.value2))
            {
                graphAdjList[road.value1].Add(road.value2);
                graphAdjList[road.value2].Add(road.value1);
            }
            else
            {
                Debug.LogWarning($"One or both settlements for the road between {road.value1.name} and {road.value2.name} are missing.");
            }
        }

        // Init input actions for testing
        inputMaster = new InputMaster();
        inputMaster.SettlementChange.Enable();
        inputMaster.SettlementChange.MoveToFirstNeighbour.performed += _ => MoveToFirstNeighbour();
    }

    /// <returns>
    /// Returns the minimum distance measured in roads between source and destination or -1 if a path doesn't exist.
    /// </returns>
    public int GetDistanceBetween(SettlementData source, SettlementData destination)
    {
        if (source.Equals(destination))
            return 0;

        List<SettlementData> currLvl = new List<SettlementData>();
        List<SettlementData> nextLvl = new List<SettlementData>();
        nextLvl.Add(source);
        HashSet<SettlementData> discovered = new HashSet<SettlementData>();
        discovered.Add(source);
        int distance = 0;
        while (nextLvl.Count > 0)
        {
            currLvl = nextLvl;
            nextLvl.Clear();
            distance++;
            foreach (SettlementData settlement in currLvl)
            {
                foreach (SettlementData neighbour in graphAdjList[settlement])
                {
                    if (neighbour.Equals(destination))
                        return distance;
                    else if (!discovered.Contains(neighbour))
                    {
                        discovered.Add(neighbour);
                        nextLvl.Add(neighbour);
                    }
                }
            }
        }
        return -1; // a.k.a. not found
    }

    /// <summary>
    /// Shorthand for GetDistanceBetween(playerLocation, settlement)
    /// </summary>
    /// <returns>
    /// Returns the minimum distance measured in roads between player location and given settlement or -1 if a path doesn't exist.
    /// </returns>
    public int GetPlayerDistanceTo(SettlementData settlement) { return GetDistanceBetween(playerLocation, settlement); }

    /// <summary>
    /// Moves the player to a settlement adjacent to its current location.
    /// </summary>
    /// <param name="destination"></param>
    /// <returns>True for success and false for failure.</returns>
    public bool MovePlayer(SettlementData destination)
    {
        if (!graphAdjList[playerLocation].Contains(destination))
            return false;
        playerLocation = destination;
        return true;
    }

    /// <summary>
    /// Testing only.
    /// </summary>
    public void MoveToFirstNeighbour()
    {
        if (graphAdjList[playerLocation].Count >= 1)
            playerLocation = graphAdjList[playerLocation][0];
        else
            Debug.Log("Neighbour not found");
    }
}
