using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wheels" , menuName = "Upgrades/Wheels")]
public class WheelData : ScriptableObject
{
    [Tooltip("0 means no grip at all, 1 means maximum grip")]
    public float grip;

    public GameObject graphics;
}
