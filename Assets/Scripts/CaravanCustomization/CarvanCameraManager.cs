using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class CaravanCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineDollyCart dollyCart;
    [SerializeField] private CinemachineSmoothPath dollyTrack;
    [SerializeField] private float lateralSpeed;

    void Update()
    {
        float speed = Input.GetAxis("Horizontal");
        dollyCart.m_Speed = -speed * lateralSpeed;
    }

    public void UpdateDollyLength(float zPos)
    {
        Debug.Log($"Updating dolly length to: {zPos}");

        dollyTrack.m_Waypoints[0].position.z = zPos;
    }
}
