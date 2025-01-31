using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Zone", menuName = "TradeSystem/Zone")]
public class ZoneData : ScriptableObject
{
    public new string name;
    public List<EventSettlementContext> eventContexts;
    public List<SettlementData> settlements;
}
