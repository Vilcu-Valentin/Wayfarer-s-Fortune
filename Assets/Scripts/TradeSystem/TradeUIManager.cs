using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject tradeItem_Prefab;

    [Header("SettlementInfo")]
    public GameObject infoCanvas;
    public TMP_Text settlementName;
    public TMP_Text settlementSize;
    public TMP_Text settlementOccupation;
    public GameObject tradeItemList;

    [Header("Cameras")]
    public CinemachineFreeLook cityViewCamera;
    public TradeCameraFocus focus;

    [Header("Canvas")]
    public GameObject cityNameCanvas;

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
        ChangeToMainCamera();
    }

    public void OpenInfoCanvas(Settlement settlement)
    {
        infoCanvas.SetActive(true);
        RemoveChildGameObjects(tradeItemList);

        settlementName.text = settlement.data.name;
        settlementSize.text = settlement.data.size;
        settlementOccupation.text = settlement.data.occupation;

        ChangeToCityCamera(settlement.transform);

        List<OutputPriceData> data = settlement.getPrices();
        foreach(OutputPriceData price in data)
        {
            GameObject item = Instantiate(tradeItem_Prefab, tradeItemList.transform);
            if(item.TryGetComponent(out Item_UIManager uiManager))
            {
                uiManager.UpdateUI(price);
            }
        }
    }

    private void RemoveChildGameObjects(GameObject parent)
    {
        Debug.Log(transform.childCount);
        int i = 0;

        //Array to hold all child objd
        GameObject[] allChildren = new GameObject[parent.transform.childCount];

        //Find all child obj and store to that array
        foreach (Transform child in parent.transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        //Now destroy them
        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child.gameObject);
        }

        Debug.Log(parent.transform.childCount);

    }

    public void ChangeToCityCamera(Transform target)
    {
        cityNameCanvas.SetActive(false);

        cityViewCamera.LookAt = target;
        cityViewCamera.Follow = target;

        cityViewCamera.Priority = 100;
        focus.FocusOnObject(target);
    }

    public void ChangeToMainCamera()
    {
        cityNameCanvas.SetActive(true);
        cityViewCamera.Priority = 1;
        focus.AutoFocus();
    }
}
