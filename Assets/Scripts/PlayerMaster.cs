using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaster
{
    public static readonly int[] lvlExpThresholds = { 100, 300, 500, 1000, 1400, 2000, 3000, 4500, 6000 };

    public float money { get; private set; }
    public int currentExp { get; private set; } = 0;
    public int currentLvl { get; private set; } = 1;

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

    public void addExp(int exp) { currentExp += exp; updateCurrentLvl(); }

    /// <summary>
    /// Updates the currentLvl member field. Returns true if a change occured and false otherwise.
    /// </summary>
    /// <returns></returns>
    private bool updateCurrentLvl() 
    {
        int exp = currentExp;
        int lvl = 1;
        int i = 0;
        while(i<lvlExpThresholds.Length && exp>0)
        {
            currentLvl++;
            exp -= lvlExpThresholds[i];
        }

        if(lvl != currentLvl)
        {
            currentLvl = lvl;
            return true;
        }
        currentLvl = lvl;
        return false;
    }
}
