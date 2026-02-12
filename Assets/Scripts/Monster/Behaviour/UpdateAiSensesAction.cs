using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeReference] public BlackboardVariable<Vector3> currentSoundLocation;
    [SerializeReference] public BlackboardVariable<float> currentSoundVolume;
    [SerializeReference] public BlackboardVariable<bool> newSoundToInvestigate;
    [SerializeReference] public BlackboardVariable<Vector3> newSoundLocation;
    [SerializeReference] public BlackboardVariable<float> newSoundVolume;
    [SerializeReference] public BlackboardVariable<float> maxSoundDistance;
    HearingChecker hearingScript;
    // TODO: replace this with per-sound-type values
    // TODO: also, these are to be used when we have ray collisions with monster working!
    // int raysToTrigger = 50;        
    // float memoryDuration = 1.0f;   
    // Dictionary<Transform, List<float>> suspicionTracks = new Dictionary<Transform, List<float>>();

    bool initialised = false;

    protected override Status OnStart()
    {
        if (!initialised)
        {
            FOVScripts = Agent.Value.GetComponentsInChildren<FieldOfViewChecker>();
            lastLocation.Value = Agent.Value.transform.position;
            lastDirection.Value = Agent.Value.transform.forward;
            hearingScript = Agent.Value.GetComponentInChildren<HearingChecker>();
            currentSoundLocation.Value = Agent.Value.transform.position;
            currentSoundVolume.Value = 0;
            initialised = true;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        updateFOV();
        // updateHearing();
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
        SoundData? source = hearingScript.HearingCheck();
        if (source.HasValue)
        {
            float distance = Vector3.Distance(source.Value.transform.position, Agent.Value.transform.position);
            if (distance <= maxSoundDistance)
            {
                newSoundToInvestigate.Value = true;
                newSoundLocation.Value = source.Value.transform.position;
                newSoundVolume.Value = source.Value.volume / distance;
            }
        } 
    }

    protected override void OnEnd()
    {
    }
}

