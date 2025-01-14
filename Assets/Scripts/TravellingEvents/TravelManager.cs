using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelManager : MonoBehaviour
{
    public MapMaster mapManager;

    [Header("Prefabs")]
    public GameObject travelEventPrefab;
    public GameObject blackScreen;

    [Header("References")]
    public GameObject journeyLog_Canvas;
    public GameObject eventsParent;

    private Image blackScreenImage;

    private void Awake()
    {
        // Get the Image component of the blackScreen
        blackScreenImage = blackScreen.GetComponent<Image>();
        if (blackScreenImage == null)
        {
            Debug.LogError("BlackScreen does not have an Image component!");
        }
    }

    public void UseEffects(List<List<EffectResult>> results, List<TravelEvents> events)
    {
        journeyLog_Canvas.SetActive(true);
        OpenBlackScreen(results, events);
    }

    public void OpenBlackScreen(List<List<EffectResult>> results, List<TravelEvents> events)
    {
        blackScreen.SetActive(true);
        PopulateEvents(results, events);
        // Fade in the black screen and move the player marker afterward
        StartCoroutine(FadeBlackScreen(0f, 1f, 1f, () =>
        {
            mapManager.MovePlayerMarker();
        }));
    }

    public void CloseBlackScreen()
    {
        StopAllCoroutines();
        mapManager.MovePlayerMarker();
        StartCoroutine(FadeBlackScreen(1f, 0f, 1f, () => blackScreen.SetActive(false)));
    }

    private void PopulateEvents(List<List<EffectResult>> results, List<TravelEvents> events)
    {
        // Clear existing event UI elements
        foreach (Transform child in eventsParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new event UI elements
        for (int i = 0; i < results.Count; i++)
        {
            TravelEvent_UI eventUI = Instantiate(travelEventPrefab, eventsParent.transform).GetComponent<TravelEvent_UI>();
            eventUI.Initialize(events[i], results[i]);
        }
    }

    private IEnumerator FadeBlackScreen(float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        if (blackScreenImage == null)
            yield break;

        float elapsed = 0f;
        Color color = blackScreenImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            blackScreenImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Ensure the final alpha value is set
        blackScreenImage.color = new Color(color.r, color.g, color.b, endAlpha);

        // Invoke the onComplete callback if provided
        onComplete?.Invoke();
    }
}
