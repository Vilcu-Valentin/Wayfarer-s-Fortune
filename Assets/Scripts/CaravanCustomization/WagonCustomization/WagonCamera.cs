using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook lookCamera;
    // Used to set a high priority on this camera
    public void FocusCamera()
    {
        lookCamera.Priority = 100;
    }

    // Used to set a low priority on this camera
    public void DeFocusCamera()
    {
        lookCamera.Priority = 1;
    }
}
