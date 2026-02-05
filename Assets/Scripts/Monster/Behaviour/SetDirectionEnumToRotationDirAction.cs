using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Direction Enum to rotation dir", story: "Set [Direction] to direction between [Agent] and [LookVector]", category: "Action", id: "3a8bcdd192717ca5c97dc69b016bffc2")]
public partial class SetDirectionEnumToRotationDirAction : Action
{
    [SerializeReference] public BlackboardVariable<LookDirection> Direction;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> LookVector;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 forward = Agent.Value.transform.forward;
        Vector3 direction = LookVector.Value;
        forward.y = 0;
        direction.y = 0;
        float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
        if (angle > 0)
            Direction.Value = LookDirection.RIGHT;
        else
            Direction.Value = LookDirection.LEFT;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

