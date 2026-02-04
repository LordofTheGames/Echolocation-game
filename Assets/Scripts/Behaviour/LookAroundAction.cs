using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look around", story: "[Agent] smoothly looks around with angle [Angle]", category: "Action", id: "5826bfab915f70528e58fb058190ff82")]
public partial class LookAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Angle;
    [SerializeReference] public BlackboardVariable<float> Speed  = new BlackboardVariable<float>(5.0f);

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Transform selfTransform = Agent.Value.transform;
        // rotate around the Y-axis only
        Quaternion rotation1 = Quaternion.Euler(0f, Angle, 0f);
        Quaternion rotation2 = Quaternion.Euler(0f, -2 *Angle, 0f);
        selfTransform.rotation = Quaternion.Slerp (selfTransform.rotation, rotation1, Time.deltaTime * Speed);
        selfTransform.rotation = Quaternion.Slerp (selfTransform.rotation, rotation2, Time.deltaTime * Speed);
        selfTransform.rotation = Quaternion.Slerp (selfTransform.rotation, rotation1, Time.deltaTime * Speed);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

