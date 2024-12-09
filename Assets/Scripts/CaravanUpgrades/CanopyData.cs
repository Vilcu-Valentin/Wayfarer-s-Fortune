using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Canopy", menuName = "Upgrades/Canopy")]
public class CanopyData : ScriptableObject
{
    [Tooltip("0 means no height increase. You can have both negative and positive values")]
    public int heightDiff;
    [Tooltip("0 means no height increase. You can have both negative and positive values")]
    public int healthDiff;

    public GameObject graphics;
}

