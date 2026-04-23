using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "What area is monster in", story: "[Agent] is in room", category: "Conditions", id: "68a1bc9c63b417e7093811f850312edf")]
public partial class WhatAreaIsMonsterInCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        return Agent.Value.GetComponent<Movement>().inRoom(); 
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
