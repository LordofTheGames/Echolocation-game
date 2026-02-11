using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Agent hears sound", story: "[Agent] hears [SoundType] with [Count] rays", category: "Conditions", id: "22fe3af3df8dace5b0299c998b71da4f")]
public partial class AgentHearsSoundCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<LookDirection> SoundType;
    [SerializeReference] public BlackboardVariable<int> Count;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
