using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Journey_CaravanManager : CaravanManager
{
    [SerializeField] private Vector3 wagonOffset;

    public override bool AddWagon(GameObject wagonPrefab, List<StorageModule> storageModules)
    {
        try
        {
            if (wagonPrefab == null) return false;

            Vector3 position = CalculateNextWagonPosition();
            GameObject wagonObject = Instantiate(wagonPrefab, position, transform.rotation);
            Wagon newWagon = wagonObject.GetComponent<Wagon>();

            if(WagonCount == 0) 
                wagonObject.GetComponent<ConfigurableJoint>().connectedBody = this.GetComponent<Rigidbody>();
            else
                wagonObject.GetComponent<ConfigurableJoint>().connectedBody = Wagons[WagonCount - 1].GetComponent<Rigidbody>();

            if (newWagon == null)
            {
                Destroy(wagonObject);
                return false;
            }

            if (storageModules != null)
            {
                foreach (StorageModule storageModule in storageModules)
                {
                    newWagon.AddStorageModule(storageModule);
                }
            }

            Wagons.Add(newWagon);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding wagon: {e.Message}");
            return false;
        }
    }

    public override void ResetCaravan()
    {
        foreach (var wagon in Wagons)
        {
            Destroy(wagon.gameObject);
        }
        Wagons.Clear();
    }

    private Vector3 CalculateNextWagonPosition()
    {
        if (Wagons.Count == 0)
        {
            // Calculate offset from the main object’s position along its local axis
            return transform.TransformPoint(wagonOffset);
        }

        Wagon lastWagon = Wagons[Wagons.Count - 1];

        // Calculate offset from the last wagon’s local position along its local axis
        return lastWagon.transform.TransformPoint(wagonOffset);
    }

}
