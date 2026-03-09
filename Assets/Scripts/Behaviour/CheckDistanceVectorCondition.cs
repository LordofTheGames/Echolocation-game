using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check Distance Vector", story: "distance between [Transform] and [Vector] [Operator] [Threshold]", category: "Conditions", id: "0043e625efb29ae5ce60c8f97bddc8f6")]
public partial class CheckDistanceVectorCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Vector3> Vector;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> Threshold;

    public override bool IsTrue()
    {
        float distance = Vector3.Distance(Transform.Value.position, Vector.Value);
        return ConditionUtils.Evaluate(distance, Operator, Threshold.Value);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
