using UnityEngine;

public class DragLayerManager : MonoBehaviour
{
    public static DragLayerManager Instance { get; private set; }

    [SerializeField]
    private Transform dragLayerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public Transform GetDragLayer()
    {
        return dragLayerTransform;
    }
}
