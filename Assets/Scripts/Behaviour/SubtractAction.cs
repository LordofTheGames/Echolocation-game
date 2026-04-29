using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "subtract", story: "Subtract [Num] from [Val]", category: "Action", id: "c211e3b22494a269b83f5354589afd32")]
public partial class SubtractAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Num;
    [SerializeReference] public BlackboardVariable<float> Val;

    protected override Status OnStart()
    {
        Val.Value -= Num.Value;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

