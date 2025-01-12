using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class TradeCameraFocus : MonoBehaviour
{
    [Header("DOF Settings")]
    [Tooltip("X value is the start offset from the focus distance, Y value is the end offset from the focus distance")]
    public Vector2 nearRange;
    [Tooltip("X value is the start offset from the focus distance, Y value is the end offset from the focus distance")]
    public Vector2 farRange;

    [Header("Volume")]
    public VolumeProfile profile;
    private DepthOfField dof;
    private Transform mainCamera;

    private bool autoFocus = true;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main.transform;
        profile.TryGet<DepthOfField>(out dof);
    }

    // Update is called once per frame
    void Update()
    {
        if(autoFocus)
            HandleFocus();
    }

    void HandleFocus()
    {
        if (dof == null) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            float dist = Vector3.Distance(mainCamera.position, hit.point);
            dof.nearFocusStart.value = dist - nearRange.x * dist / 40;
            dof.nearFocusEnd.value = dist - nearRange.y * dist / 40;
            dof.farFocusStart.value = dist + farRange.x * dist / 40;
            dof.farFocusEnd.value = dist + farRange.y * dist / 40;
        }
    }

    public void FocusOnObject(Transform focus)
    {
        StartCoroutine(lateObjectFocus(focus));
    }

    IEnumerator lateObjectFocus(Transform focus)
    {
        yield return new WaitForSeconds(1);
        autoFocus = false;
        float dist = Vector3.Distance(mainCamera.transform.position, focus.position);
        dof.nearFocusStart.value = dist - nearRange.x;
        dof.nearFocusEnd.value = dist - nearRange.y;
        dof.farFocusStart.value = dist + farRange.x;
        dof.farFocusEnd.value = dist + farRange.y;
    }

    public void AutoFocus()
    {
        autoFocus = true;
        StopAllCoroutines();
    }
}
