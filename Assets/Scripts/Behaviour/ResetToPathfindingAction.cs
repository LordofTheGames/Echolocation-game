using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Reset to pathfinding", story: "[Agent] sets [node] to closest node (reset to pathfinding)", category: "Action", id: "da9738166ecdb738b5105b3e861e93e6")]
public partial class ResetToPathfindingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Node;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Node.Value = Agent.Value.GetComponent<Movement>().ResetToPathfinding();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

