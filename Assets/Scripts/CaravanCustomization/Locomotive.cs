using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
public class Locomotive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
=======
public class Locomotive : CaravanBody
{
    [SerializeField] private UpgradesManager upgradesManager;
    // Start is called before the first frame update
    void Start()
    {
        upgradesManager.UpdateVehicleUpgrades();
        spacing = upgradesManager.GetCurrentFrame().spacing;
    }

>>>>>>> develop
}
