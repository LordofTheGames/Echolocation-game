using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set NavMeshAgent Speed", story: "Set [Agent] NavMesh speed to [Value]", category: "Action", id: "77581067e2ec8c871dcd34aecffe2724")]
public partial class SetNavMeshAgentSpeedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        Agent.Value.GetComponent<NavMeshAgent>().speed = Value.Value;
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

