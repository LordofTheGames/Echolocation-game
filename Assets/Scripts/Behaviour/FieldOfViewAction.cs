using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "field of view", story: "Check if [Target] is in field of view of [Agent] and set [FollowTarget]", category: "Action", id: "2de421468d7cef870074735fc8379e2a")]
public partial class FieldOfViewAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> FollowTarget;
    [SerializeReference] public BlackboardVariable<FieldOfView> FovScript;
    Vector3 lastPos;

    protected override Status OnStart()
    {
        lastPos = Target.Value.transform.position;
        FollowTarget.Value = Target.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        bool canSee = FovScript.Value.FieldOfViewCheck(Target.Value);
        if (canSee)
        {
            FollowTarget.Value = Target.Value;
            lastPos = Target.Value.transform.position;
            return Status.Success;
        }
        FollowTarget.Value.transform.position = lastPos;
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

