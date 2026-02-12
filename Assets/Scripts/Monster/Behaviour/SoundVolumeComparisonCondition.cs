using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "sound volume comparison", story: "[newVolume] is [operator] [currentVolume] proportional to distance between [Agent] and [newLocation] / [currentLocation]", category: "Conditions", id: "13c71d98c06eaff42c3d9a4f992e0920")]
public partial class SoundVolumeComparisonCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> NewVolume;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> CurrentVolume;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> NewLocation;
    [SerializeReference] public BlackboardVariable<Vector3> CurrentLocation;

    public override bool IsTrue()
    {
        Vector3 agentPos = Agent.Value.transform.position;
        return ConditionUtils.Evaluate(NewVolume / Vector3.Distance(agentPos, NewLocation), Operator, CurrentVolume / Vector3.Distance(agentPos, CurrentLocation));
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
