using System.Collections;
using System.Collections.Generic;
using Unity.XR.GoogleVr;
using UnityEngine;
using System.Threading;

public enum Season : int
{
    Spring = 0,
    Summer = 1,
    Autumn = 2,
    Winter = 3
}

public sealed class TimeMaster
{
    public static readonly int daysPerSeason = 14; // if this...
    public static readonly int daysPerYear = 4 * daysPerSeason; // or this will ever be changed remember to check the editor range bounds for time-related thingies

    public int day { get; private set; }
    public int hour { get; private set; }

    public delegate void TimeChangedEvent(int newDay, int newTime);
    public TimeChangedEvent timeChangedEvent;

    private InputMaster inputMaster = null;

    private static TimeMaster instance = null;
    private static readonly object padlock = new object();
    

    private TimeMaster(int day=1, int hour=8)
    {
        this.day = day;
        this.hour = hour;
        inputMaster = new InputMaster();
        inputMaster.TimeManipulation.Enable();
        inputMaster.TimeManipulation.AdvanceHour.performed += _ => advanceTime(1);
        inputMaster.TimeManipulation.AdvanceDay.performed += _ => advanceTime(24);
        inputMaster.TimeManipulation.AdvanceSeason.performed += _ => advanceTime(24 * daysPerSeason);
    }

    public static TimeMaster Instance()
    {
        lock(padlock)
        {
            if(instance == null)
                instance = new TimeMaster();
        }
        return instance;
    }

    public void advanceTime(int hours)
    {
        day += (hour+hours) / 24;
        hour = (hour+hours) % 24;
        timeChangedEvent(day, hour);
    }

    public Season getCurrentSeason() { 
        return getSeason(day);
    }

    public static Season getSeason(int day) {
        return (Season)((int)((day - 1) / daysPerSeason) % 4);
    }
}
