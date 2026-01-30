using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "field of view", story: "Check if [Target] is in field of view of [Agent] and set [TargetLastSeen]", category: "Action", id: "2de421468d7cef870074735fc8379e2a")]
public partial class FieldOfViewAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> TargetLastSeen;
    [SerializeReference] public BlackboardVariable<FieldOfView> FovScript;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        bool canSee = FovScript.Value.FieldOfViewCheck(Target.Value);
        if (canSee)
        {
            TargetLastSeen.Value = Target.Value.transform.position;
            return Status.Success;
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

