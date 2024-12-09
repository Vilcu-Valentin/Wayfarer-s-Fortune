using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CaravanManager : MonoBehaviour
{
    private List<Wagon> wagons = new List<Wagon>();

    public int WagonCount => wagons.Count; // Read-only access to the number of wagons
    public List<Wagon> Wagons => wagons; // Expose Wagons
    public Locomotive locomotive;

    public abstract void ResetCaravan();
    public abstract bool AddWagon(GameObject wagonPrefab, List<StorageModule> storageModules);
}
