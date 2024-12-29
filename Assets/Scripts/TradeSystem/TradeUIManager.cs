using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    [Header("SettlementInfo")]
    public GameObject infoCanvas;
    public TMP_Text settlementName;
    public TMP_Text settlementSize;
    public TMP_Text settlementOccupation;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform.TryGetComponent(out Settlement settlement))
                    OpenInfoCanvas(settlement);
            }
        }
    }

    public void CloseInfoCanvas()
    {
        infoCanvas.SetActive(false);
    }

    public void OpenInfoCanvas(Settlement settlement)
    {
        infoCanvas.SetActive(true);

        settlementName.text = settlement.data.name;
        settlementSize.text = settlement.data.size;
        settlementOccupation.text = settlement.data.occupation;
    }
}
