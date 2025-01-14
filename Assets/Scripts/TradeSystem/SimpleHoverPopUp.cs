using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleHoverPopUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target;
    public bool activeByDefault = false;

    private void Start()
    {
        if(activeByDefault)
            target.SetActive(true);
        else 
            target.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.SetActive(false);
    }

    public void DisableTarget()
    {
        target.SetActive(false);
    }
}
