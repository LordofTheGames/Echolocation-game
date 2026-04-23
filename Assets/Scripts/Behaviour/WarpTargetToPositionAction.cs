using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WARP target to position", story: "Warp [Agent] to [position]", category: "Action", id: "f62f9eca63b13b9e7bf7481ac5ad1177")]
public partial class WarpTargetToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Position;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        NavMeshAgent nav = Agent.Value.GetComponent<NavMeshAgent>();
        nav.ResetPath();
        nav.Warp(Position.Value.position);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

