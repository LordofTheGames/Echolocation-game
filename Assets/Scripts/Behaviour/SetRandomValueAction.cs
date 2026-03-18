using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set random value", story: "Set [Number] to random value between [LowerBound] and [UpperBound]", category: "Action", id: "2ab1133d051d9b8dac0e30bfc9f7175a")]
public partial class SetRandomValueAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Number;
    [SerializeReference] public BlackboardVariable<float> LowerBound;
    [SerializeReference] public BlackboardVariable<float> UpperBound;

    protected override Status OnStart()
    {
        Number.Value = UnityEngine.Random.Range(LowerBound, UpperBound);
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

