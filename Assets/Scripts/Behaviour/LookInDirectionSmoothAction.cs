using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look In Direction Smooth", story: "[Agent] smoothly turns to direction of [Vector]", category: "Action", id: "27fd5f65789848e253f5f65f620569ef")]
public partial class LookInDirectionSmoothAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Vector;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(5.0f);
    [SerializeReference] public BlackboardVariable<bool> LimitRotationToYAxis = new BlackboardVariable<bool>(true);

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        Quaternion rotation = Quaternion.LookRotation(Vector);
        // Limit rotation to y-axis
        if (LimitRotationToYAxis) {
            rotation.x = 0;
            rotation.z = 0;
        }
        Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, rotation, Time.deltaTime * Speed.Value);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

