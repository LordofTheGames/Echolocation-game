using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look In Direction Smooth", story: "[Agent] smoothly turns to same direction as [Target]", category: "Action", id: "27fd5f65789848e253f5f65f620569ef")]
public partial class LookInDirectionSmoothAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, Target.Value.transform.rotation, Time.deltaTime * Speed);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

