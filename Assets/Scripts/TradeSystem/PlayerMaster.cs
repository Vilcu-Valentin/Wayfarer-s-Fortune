using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaster
{
    public static readonly int[] lvlExpThresholds = { 100, 300, 500, 1000, 1400, 2000, 3000, 4500, 6000 };

    public float money { get; private set; }
    public int currentExp { get; private set; } = 0;
    public int currentLvl { get; private set; } = 1;

    [Tooltip("The amount of money you need to win the level")]
    public int moneyGoal { get; private set; }
    [Tooltip("Time in hours, when the current time reaches the maxTime you either win or lose")]
    public int maxTime { get; private set; }

    private static PlayerMaster instance;
    private static readonly object padlock = new object();

    private PlayerMaster(float money = 1000, int exp = 0)
    {
        this.money = money;
        this.currentExp = exp;
        updateCurrentLvl();
    }

    public static PlayerMaster Instance()
    {
        lock(padlock)
        {
            if(instance == null)
                instance = new PlayerMaster();
        }
        return instance;
    }

    public bool addMoney(float amount)
    {
        money += amount;
        return true;
    }

    public bool removeMoney(float amount)
    {
        if (money - amount < 0)
            return false;
        money -= amount;
        return true;
    }

    public void addExp(int exp) { currentExp += exp; updateCurrentLvl(); }
    public void incLvl() { currentLvl++; updateCurrentLvl(); }

    /// <summary>
    /// Updates the currentLvl member field. Returns true if a change occured and false otherwise.
    /// </summary>
    /// <returns></returns>
    private bool updateCurrentLvl()
    {
        int exp = currentExp;
        int lvl = 1;
        int i = 0;
        while (i < lvlExpThresholds.Length && exp > 0)
        {
            currentLvl++;
            exp -= lvlExpThresholds[i];
        }

        if (lvl != currentLvl)
        {
            currentLvl = lvl;
            return true;
        }
        currentLvl = lvl;
        return false;
    }

}
