using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set transform position", story: "Set [Target] position to [Object]", category: "Action", id: "279c43769c7be6f39ab345e9c442f5f7")]
public partial class SetTransformPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Object;
    private CharacterController cc;

    protected override Status OnStart()
    {
        NavMeshAgent agent = Target.Value.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(Object.Value.transform.position);
            return Status.Running;
        }

        cc = Target.Value.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        Target.Value.transform.position = Object.Value.transform.position;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (cc != null) cc.enabled = true;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

