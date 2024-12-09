using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Yet another class whose sole purpose is to make the unity editor serialize something, this time something super basic like a pair/tuple that should 
/// by all means already be supported. In any case, if these SerializableThing shenanigans get out of hand I'll look into another way to persuade
/// the editor.
///    ____________,  _     _   ___   __   __   ________   __  __
///   |    ________| | |   | | |   \ |  | |  | |___  ___| |  \/  |
///   /   /__/       | |   | | |    \|  | |  |    |  |     \    /
///  /   /           |  \_/  | |  |\    | |  |    |  |      |  |
/// /___/             \_____/  |__| \___| |__|    |__|      |__|
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
[Serializable]
public class SerializablePair<T1, T2>
{
    public T1 value1;
    public T2 value2;

    public SerializablePair(T1 value1 = default(T1), T2 value2 = default(T2))
    {
        this.value1 = value1;
        this.value2 = value2;
    }
}
