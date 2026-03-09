using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Vector to Position", story: "Set [vector3] to [gameObject] position", category: "Action", id: "b0da03c33c71104a09316e65fa0dc57b")]
public partial class SetVectorToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> vector3;
    [SerializeReference] public BlackboardVariable<GameObject> gameObject;

    protected override Status OnStart()
    {
        vector3.Value = gameObject.Value.transform.position;
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

