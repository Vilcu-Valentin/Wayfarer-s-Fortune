using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class represents either a Wagon or a Locomotive
/// This class will also handle the upgrades for a CaravanBody (mainly graphics, and hold the data for the changes)
/// It will also hold other data like the weight etc.
/// </summary>
public class CaravanBody: MonoBehaviour
{
    // A list of scriptableObjects entries that represent the upgrades 
    [Tooltip("This is used in the caravan manager for instantiation. X: the length of the wagon; Y: space between this and the previous wagon")]
    public Vector2 spacing;
}
