using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeManager : MonoBehaviour
{
    public TradeUIManager uiManager;
    public MapMaster mapManger;
    public LayerMask mask;

    private Settlement currentSelectedSettlement;
    private SettlementData currentDestination;

    private OutlineManager cachedOutliner;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && uiManager.cityMode == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform.TryGetComponent(out Settlement settlement))
                {
                    uiManager.OpenInfoCanvas(settlement, mapManger.GetPlayerDistanceTo(settlement.data));
                    currentSelectedSettlement = settlement;
                }
            }
        }

        if (cachedOutliner != null)
            cachedOutliner.EnableOutlines(false);
        if (uiManager.cityMode == false)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            {
                if (hit.transform.TryGetComponent(out OutlineManager outliner))
                { 
                    outliner.EnableOutlines(true);
                    cachedOutliner = outliner;
                }
            }
        }
    }

    public void ChangePlayerDestination()
    {
        if (currentSelectedSettlement != null)
            currentDestination = currentSelectedSettlement.data;
        else
            currentDestination = mapManger.playerLocation;

        mapManger.HighlighRoads(currentDestination);

        mapManger.MovePlayer(currentDestination);

        Debug.LogWarning("Player changed location --- remember, we don't advance the time yet");
    }

    public Settlement getCurrentSettlement()
    {
        return mapManger.getPlayerLocation();
    }
}
