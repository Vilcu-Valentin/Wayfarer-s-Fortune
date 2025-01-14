using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab; // The prefab to pool
    public int initialSize = 20; // Initial pool size

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Pool is empty, instantiate a new object
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }

    public void ClearPool()
    {
        while (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            Destroy(obj);
        }
    }
}
