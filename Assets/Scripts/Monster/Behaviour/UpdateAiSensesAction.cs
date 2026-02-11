using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
#nullable enable annotations

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "update ai senses", story: "Update AI Senses", description: "Updates all the variables below (Except for Agent and Target)", category: "Action", id: "4b96d6fa266b06b15ec3a7ead8af5e72")]
        
public partial class UpdateAiSensesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    // --- Seeing variables ---
    [SerializeReference] public BlackboardVariable<bool> targetSeen;
    [SerializeReference] public BlackboardVariable<Vector3> lastLocation;
    [SerializeReference] public BlackboardVariable<Vector3> lastDirection;
    FieldOfViewChecker[] FOVScripts;
    Vector3 secondToLastLocation;
    // --- Hearing variables ---
    [SerializeReference] public BlackboardVariable<bool> heardSound;
    [SerializeReference] public BlackboardVariable<Vector3> soundLocation;
    HearingChecker hearingScript;
    Dictionary<Transform, List<float>> suspicionTracks = new Dictionary<Transform, List<float>>();
    // TODO: replace this with per-sound-type values
    // TODO: also, these are to be used when we have ray collisions with monster working!
    // int raysToTrigger = 50;        
    // float memoryDuration = 1.0f;   
    [SerializeReference] public BlackboardVariable<int> maxDistance;
    [SerializeReference] public BlackboardVariable<int> volumeThreshold;

    bool initialised = false;

    protected override Status OnStart()
    {
        if (!initialised)
        {
            FOVScripts = Agent.Value.GetComponentsInChildren<FieldOfViewChecker>();
            hearingScript = Agent.Value.GetComponentInChildren<HearingChecker>();
            lastLocation.Value = Agent.Value.transform.position;
            lastDirection.Value = Agent.Value.transform.forward;
            initialised = true;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        updateFOV();
        updateHearing();
        return Status.Success;
    }

    private void updateFOV()
    {
        for (int i = 0; i < FOVScripts.Length; i++)
        {
            if (FOVScripts[i].FieldOfViewCheck(Target))
            {
                targetSeen.Value = true;
                secondToLastLocation = lastLocation.Value;
                lastLocation.Value = Target.Value.transform.position;
                lastDirection.Value = lastLocation.Value - secondToLastLocation;
                return;
            }
        }
        targetSeen.Value = false;
    }

    private void updateHearing()
    {
        Transform? source = hearingScript.HearingCheck();
        if (source == null) return;
        
        // A. Do I already know about this object?
        if (!suspicionTracks.ContainsKey(source))
            suspicionTracks.Add(source, new List<float>());

        // B. Add a new "memory" of being hit right now
        suspicionTracks[source].Add(Time.time);

        // C. Check Suspicion Level for THIS SPECIFIC object
        int suspicionLevel = suspicionTracks[source].Count;

        if (suspicionLevel >= raysToTrigger)
        {
            heardSound.Value = true;
            soundLocation.Value = source.position;
        }
    }

    protected override void OnEnd()
    {
    }
}

