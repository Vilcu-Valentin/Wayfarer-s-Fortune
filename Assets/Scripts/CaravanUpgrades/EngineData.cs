using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Engine", menuName = "Upgrades/Engine")]
public class EngineData : ScriptableObject
{
    [Tooltip("Max power when torque curve is at 1. This is measured in horse power")]
    public float power;
    [Tooltip("This maps a percentage of power based on the rpm")]
    public AnimationCurve torqueCurve;
    public GameObject graphics;
}
