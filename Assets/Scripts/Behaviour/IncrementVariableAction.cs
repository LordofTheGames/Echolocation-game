using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Increment variable", story: "Increment [Variable] by [Value]", category: "Action", id: "9db1bac06b085115c24309d456367c6f")]
public partial class IncrementVariableAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Variable;
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        Variable.Value += Value.Value;
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

