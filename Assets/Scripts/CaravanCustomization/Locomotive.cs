using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotive : CaravanBody
{
    [SerializeField] private UpgradesManager upgradesManager;
    // Start is called before the first frame update
    void Start()
    {
        upgradesManager.UpdateVehicleUpgrades();
        spacing = upgradesManager.GetCurrentFrame().spacing;
    }

}
