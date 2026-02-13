using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Pick location near target", story: "Set [TargetLocation] to nearest point to [Agent] on sphere radius [Radius] from [Target]", category: "Action", id: "f02ab739078fc30279adb3750ab006e1")]
public partial class PickLocationNearTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        Vector3 direction = Agent.Value.transform.position - Target.Value.transform.position;
        TargetLocation.Value = Target.Value.transform.position + Radius.Value * Vector3.Normalize(direction);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

