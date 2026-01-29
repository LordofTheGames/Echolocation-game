using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Line of sight", story: "[Agent] has line of sight to [Target]", category: "Flow/Conditional", id: "26e9d86cf37c6e007ddde95f464149e5")]
public partial class LineOfSightSequence : Composite
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<bool> ShowDebugVisuals;
    [SerializeReference] public Node True;
    [SerializeReference] public Node False;

    Transform agentTransform;
    Transform targetTransform;

    protected override Status OnStart()
    {
        agentTransform = Agent.Value.transform;
        targetTransform = Target.Value.transform;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        RaycastHit hit;
        Vector3 direction = targetTransform.position - agentTransform.position;
        Physics.Raycast(agentTransform.position + Vector3.up * Range,
            direction, out hit, Range, Target.Value.layer);

        if (hit.collider != null && hit.collider.gameObject == Target.Value)
        {
            if (ShowDebugVisuals)
            {
                Debug.DrawLine(agentTransform.position + Vector3.up * Range,
                    targetTransform.position, Color.green);
            }
            // return True;
        }
        // return False;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    private void OnDrawGizmos()
    {
        if (ShowDebugVisuals)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Agent.Value.transform.position + Vector3.up * Range, 0.3f);
        }
    }
}

