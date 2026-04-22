using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get next area node", story: "[Agent] sets [node] to next area node", category: "Action", id: "62a89cb931dc040f4e6b42dc038c8b69")]
public partial class GetNextAreaNodeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Node;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Node.Value = Agent.Value.GetComponent<Movement>().NextArea();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

