using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Event_UIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image eventSprite;

    // Define events for hover enter and exit
    public event Action<Event_UIManager> OnHoverEnter;
    public event Action<Event_UIManager> OnHoverExit;

    private EventData data;

    public void UpdateUI(EventData _event)
    {
        eventSprite.sprite = _event.icon;
        data = _event;
    }

    public EventData getEvent()
    {
        return data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Mouse entered: {gameObject.name}");
        // Trigger the hover enter event
        OnHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Mouse exited: {gameObject.name}");
        // Trigger the hover exit event
        OnHoverExit?.Invoke(this);
    }
}
