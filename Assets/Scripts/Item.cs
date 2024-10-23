using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData ItemData;

    public Vector3Int currentPosition;
    public bool rotated;
    public GameObject objectRef;
}
