using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of sight to Target", story: "[Self] has line of sight to [Target]", category: "Conditions", id: "db6b3525ba3fe545dc7f1cdd4dd62188")]
public partial class LineOfSightToTargetCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<bool> ShowDebugVisuals;

    Transform selfTransform;
    Transform targetTransform;

    public override bool IsTrue()
    {
        RaycastHit hit;
        Vector3 direction = targetTransform.position - selfTransform.position;
        Physics.Raycast(selfTransform.position + Vector3.up * Range,
            direction, out hit, Range, Target.Value.layer);

        if (hit.collider != null && hit.collider.gameObject == Target.Value)
        {
            if (ShowDebugVisuals)
            {
                Debug.DrawLine(selfTransform.position + Vector3.up * Range,
                    targetTransform.position, Color.green);
            }
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        selfTransform = Self.Value.transform;
        targetTransform = Target.Value.transform;
    }

    public override void OnEnd()
    {
    }
    
    private void OnDrawGizmos()
    {
        if (ShowDebugVisuals)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Self.Value.transform.position + Vector3.up * Range, 0.3f);
        }
    }
}
