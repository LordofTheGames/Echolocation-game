using System;
using System.Collections;
using UnityEngine;
#nullable enable

public class HearingChecker : MonoBehaviour, INoiseSensitive
{
    private Transform? newSource = null;
    private bool newSound = false;

    // This function is called automatically by the Scanner when rays hit the monster
    public void OnHeardScan(Transform source)
    {
        if (source == null) return;
        newSound = true;
        newSource = source;
    }

    // called every frame by monster behaviour tree to check for new sounds
    public Transform? HearingCheck()
    {
        if (!newSound) return null;
        newSound = false;
        return newSource;
    }
}