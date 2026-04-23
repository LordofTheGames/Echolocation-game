using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "sound volume comparison", story: "[newVolume] is [Operator] [currentVolume] proportional to distance between [Agent] / [currentAgentStartPos] and [newLocation] / [currentLocation]  [newIsFootsteps] [currIsFootsteps]", category: "Conditions", id: "13c71d98c06eaff42c3d9a4f992e0920")]
public partial class SoundVolumeComparisonCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> NewVolume;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> CurrentVolume;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> CurrentAgentStartPos;
    [SerializeReference] public BlackboardVariable<Vector3> NewLocation;
    [SerializeReference] public BlackboardVariable<Vector3> CurrentLocation;
    [SerializeReference] public BlackboardVariable<bool> newIsFootsteps;
    [SerializeReference] public BlackboardVariable<bool> currIsFootsteps;

    public override bool IsTrue()
    {
        Vector3 agentPos = Agent.Value.transform.position;
        if (newIsFootsteps && currIsFootsteps)
            return ConditionUtils.Evaluate(NewVolume.Value, Operator, CurrentVolume.Value); // if both current and new are footsteps, go to loudest one regardless of distance
            // TODO: save distance value when sound heard so that doesnt keep updating
        return ConditionUtils.Evaluate(NewVolume.Value / Vector3.Distance(agentPos, NewLocation.Value), Operator, CurrentVolume.Value / Vector3.Distance(CurrentAgentStartPos, CurrentLocation.Value));
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
