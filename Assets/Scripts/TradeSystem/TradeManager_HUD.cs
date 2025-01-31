using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering.HighDefinition;

public class TradeManager_HUD : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coin_counter;
    public TextMeshProUGUI hour;
    public TextMeshProUGUI day;
    public TextMeshProUGUI season;
    public Image hour_clock;

    private float previous_coint_counter;

    [Header("Light")]
    public Light sunLight; // Assign your directional light (Sun)
    public float maxLux = 130000f; // Noon intensity
    public float minLux = 13000f; // Midnight intensity
    public float noonTemperature = 6500f; // Daylight temperature
    public float dawnDuskTemperature = 4500f; // Warm sunset/sunrise
    public float moonTemperature = 8000f; // Moonlight temperature

    public void Update()
    {
        if(previous_coint_counter != PlayerMaster.Instance().money)
        {
            previous_coint_counter = PlayerMaster.Instance().money;
            UpdateCoinCounter();
        }

        UpdateDateTime();
        UpdateSunLighting(TimeMaster.Instance().hour);
    }

    public void UpdateSunLighting(int hour)
    {
        // Ensure hour is within range
        hour = Mathf.Clamp(hour, 0, 24);

        // Calculate light intensity (lux)
        float intensity;
        if (hour < 6) // Midnight to Dawn (rapid increase)
        {
            intensity = Mathf.Lerp(minLux, maxLux, Mathf.SmoothStep(0, 1, (hour - 4f) / 2f)); // Rapid transition from 4 to 6
        }
        else if (hour < 12) // Dawn to Noon (steady increase)
        {
            intensity = maxLux;
        }
        else if (hour < 16) // Noon to Afternoon (steady high)
        {
            intensity = maxLux;
        }
        else if (hour < 18) // Afternoon to Sunset (gradual decrease)
        {
            intensity = Mathf.Lerp(maxLux, minLux, (hour - 16f) / 2f);
        }
        else // Sunset to Midnight (rapid decrease)
        {
            intensity = minLux;
        }
        
        sunLight.intensity = intensity;

        // Adjust color temperature
        float temperature;
        if (hour < 3 || hour >= 20) // Night (8000K)
        {
            temperature = moonTemperature;
        }
        else if (hour >= 3 && hour < 6) // Dawn (instant 4500K)
        {
            temperature = dawnDuskTemperature;
        }
        else if (hour >= 6 && hour < 17) // Daytime (6500K)
        {
            temperature = noonTemperature;
        }
        else if (hour >= 17 && hour < 20) // Sunset (4500K transition)
        {
            temperature = Mathf.Lerp(noonTemperature, dawnDuskTemperature, (hour - 16f) / 2f);
        }
        else // Default safety
        {
            temperature = noonTemperature;
        }

        sunLight.colorTemperature = temperature;
    }


    public void UpdateCoinCounter()
    {
        coin_counter.text = FormatNumber(PlayerMaster.Instance().money);
    }

    public void UpdateDateTime()
    {
        hour.text = TimeMaster.Instance().hour.ToString();
        day.text = TimeMaster.Instance().day.ToString();
        season.text = TimeMaster.Instance().getCurrentSeason().ToString();
        hour_clock.transform.rotation = Quaternion.Euler(0, 0, GetRotationZ(TimeMaster.Instance().hour));
    }

    public static float GetRotationZ(int hour)
    {
        // Convert the hour to rotation, where 6 maps to 0 degrees and 18 maps to 180 degrees
        float rotation = (hour - 6) * 15f;

        return -rotation;
    }

    private static string FormatNumber(float number)
    {
        if (number == -1)
            return "???";

        if (number >= 1000000) // For numbers in the millions
            return (number / 1000000).ToString("0.#") + "M";
        else if (number >= 100000)
            return (number / 1000).ToString("0") + "k";
        else if (number >= 10000)
            return (number / 1000).ToString("0.#") + "k";
        else if (number >= 1000)
            return number.ToString("0");
        else if (number >= 100)
            return number.ToString("0.##");
        else
            return number.ToString("0.##"); // For numbers less than 1000
    }
}