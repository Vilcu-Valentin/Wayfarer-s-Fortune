using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
public class InventoryVisualizer
{
    private readonly HDAdditionalCameraData hdCamera;
    private readonly GameObject normalVolume;
    private readonly GameObject outsideLight;
    private readonly LayerMask moduleMask;
    private readonly LayerMask buildMask;

    public InventoryVisualizer(Camera mainCamera, GameObject normalVolume, GameObject outsideLight, LayerMask moduleMask, LayerMask buildMask)
    {
        this.hdCamera = mainCamera.GetComponent<HDAdditionalCameraData>();
        this.normalVolume = normalVolume;
        this.outsideLight = outsideLight;
        this.moduleMask = moduleMask;
        this.buildMask = buildMask;
    }

    public void SetInventoryView(bool isInventoryMode)
    {
        if (hdCamera == null) return;

        if (isInventoryMode)
        {
            hdCamera.backgroundColorHDR = new Color(17f / 255f, 5f / 255f, 39f / 255f) * Mathf.Pow(2, 1);
            hdCamera.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            Camera.main.cullingMask = moduleMask | buildMask;
            normalVolume.SetActive(false);
            outsideLight.SetActive(false);
        }
        else
        {
            hdCamera.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
            Camera.main.cullingMask = -1;
            normalVolume.SetActive(true);
            outsideLight.SetActive(true);   
        }
    }
}
