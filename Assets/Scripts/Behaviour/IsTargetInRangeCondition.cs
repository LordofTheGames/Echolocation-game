using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is target in range", story: "[Self] is in range of [Target]", category: "Conditions", id: "fcfd702c9be3e2f89c3aa95ec33f20eb")]
public partial class IsTargetInRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> ShowDebugVisuals;

    GameObject DetectedTarget;
    public override bool IsTrue()
    {
        // Perform sphere check
        Collider[] colliders = Physics.OverlapSphere(Self.Value.transform.position, Radius, Target.Value.layer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == Target.Value)
            {
                return true;
            }
        }
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (ShowDebugVisuals)
        {
            Gizmos.color = DetectedTarget ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(Self.Value.transform.position, Radius);
        }

    }
}
