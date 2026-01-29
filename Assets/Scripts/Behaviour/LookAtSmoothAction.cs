using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAt smooth", story: "[Self] smoothly turns to [Target]", category: "Action", id: "3b071c53d53cd2fcd5e64be0de608a95")]
public partial class LookAtSmoothAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;
    Transform selfTransform;
    Transform targetTransform;
    protected override Status OnStart()
    {
        selfTransform = Self.Value.transform;
        targetTransform = Target.Value.transform;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Quaternion rotation = Quaternion.LookRotation(targetTransform.position - selfTransform.position);
        // Limit rotation to y-axis
        rotation.x = 0;
        rotation.z = 0;
        selfTransform.rotation = Quaternion.Slerp (selfTransform.rotation, rotation, Time.deltaTime * Speed);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

