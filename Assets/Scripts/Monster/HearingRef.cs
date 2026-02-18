using UnityEngine;
using UnityEngine.AI; // Needed for NavMeshAgent
using System.Collections.Generic;

public class MonsterAI : MonoBehaviour, INoiseSensitive
{
    [Header("Senses")]
    [Tooltip("How many rays must hit me within 1 second to trigger an investigation?")]
    public int raysToTrigger = 50;        
    
    [Tooltip("How long (in seconds) I remember a hit before forgetting it.")]
    public float memoryDuration = 1.0f;   

    [Header("Debug")]
    [Tooltip("Read-only view of who the monster is currently suspicious of")]
    public string currentSuspicionDebug; 

    // The Brain: Maps a Source Object -> List of times it hit me
    private Dictionary<Transform, List<float>> suspicionTracks = new Dictionary<Transform, List<float>>();
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // cleanupList is needed because we can't remove items from a Dictionary while looping through it
        List<Transform> sourcesToRemove = new List<Transform>();

        // 1. Decaying Memory Logic
        foreach (var track in suspicionTracks)
        {
            Transform source = track.Key;
            List<float> hits = track.Value;

            // Remove hits that are older than 'memoryDuration'
            while (hits.Count > 0 && Time.time > hits[0] + memoryDuration)
            {
                hits.RemoveAt(0);
            }

            // If a source has 0 active hits, mark it for removal
            if (hits.Count == 0)
            {
                sourcesToRemove.Add(source);
            }
        }

        // 2. Remove empty tracks to keep memory clean
        foreach (var source in sourcesToRemove)
        {
            suspicionTracks.Remove(source);
        }
        
        // Debug info for Inspector
        currentSuspicionDebug = $"Tracking {suspicionTracks.Count} sources";
    }

    // This function is called automatically by the Scanner when rays hit the monster
    public void OnHeardScan(Transform source, float volume, bool isFootsteps)
    {
        if (source == null) return;

        // A. Do I already know about this object?
        if (!suspicionTracks.ContainsKey(source))
        {
            suspicionTracks.Add(source, new List<float>());
        }

        // B. Add a new "memory" of being hit right now
        suspicionTracks[source].Add(Time.time);

        // C. Check Suspicion Level for THIS SPECIFIC object
        int suspicionLevel = suspicionTracks[source].Count;

        if (suspicionLevel >= raysToTrigger)
        {
            Investigate(source);
        }
    }

    void Investigate(Transform target)
    {
        // Don't restart the path calculation if we are already going to this target (Optional optimization)
        // if (agent.destination == target.position) return;

        Debug.Log($"<color=red>ALERT!</color> Investigating noise from {target.name} (Hits: {suspicionTracks[target].Count})");

        // Tell Unity's Navigation system to walk to the source of the sound
        agent.SetDestination(target.position);
        
        // Optional: Clear suspicion so we don't re-trigger the command 100 times a second
        // This makes the monster "decide" to move, then resets its threshold
        suspicionTracks[target].Clear();
    }
}