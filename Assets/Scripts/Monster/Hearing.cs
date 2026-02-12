using System;
using System.Collections;
using UnityEngine;
#nullable enable

public struct SoundData 
{
    public Transform transform;
    public float volume;
}

// TODO: get Kieran to implement this!
// public class HearingChecker : MonoBehaviour, INoiseSensitive
public class HearingChecker : MonoBehaviour
{
    private SoundData newSource;
    private bool newSound = false;

    // This function is called automatically by the Scanner when rays hit the monster
    public void OnHeardScan(Transform source, float volume)
    {
        if (source == null) return;
        newSound = true;
        newSource.transform = source;
        newSource.volume = volume;
    }

    // called every frame by monster behaviour tree to check for new sounds
    public SoundData? HearingCheck()
    {
        if (!newSound) return null;
        newSound = false;
        return newSource;
    }
}