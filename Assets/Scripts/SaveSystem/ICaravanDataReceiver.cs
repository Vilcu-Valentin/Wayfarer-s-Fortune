// This interface defines the minimal set of operations needed to load a caravan
using System.Collections.Generic;
using UnityEngine;

public interface ICaravanDataReceiver
{
    void ResetCaravan();
    void LoadWagon(GameObject wagonPrefab, List<StorageModule> modules);
}
