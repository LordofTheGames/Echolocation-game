using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get next node", story: "[Agent] sets [node] to next node", category: "Action", id: "c592f790ab8b8a5dcc64b5eef3029abc")]
public partial class GetNextNodeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Node;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Node.Value = Agent.Value.GetComponent<Movement>().NextNode();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

