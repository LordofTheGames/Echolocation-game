using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set both sound params", story: "Set [SoundType] Memory Duration to [memValue] and Ray Count Threshold to [rayValue]", category: "Action", id: "cd213ec72ce5929655e34f80530e20fd")]
public partial class SetBothSoundParamsAction : Action
{
    [SerializeReference] public BlackboardVariable<SoundType> SoundType;
    [SerializeReference] public BlackboardVariable<int> MemValue;
    [SerializeReference] public BlackboardVariable<int> RayValue;
    [SerializeReference] public BlackboardVariable<List<Vector2Int>> SoundParams;

    protected override Status OnStart()
    {
        // copy so we can remove bits and not affect original
        uint remainingBits = (uint)SoundType.Value;
        while (remainingBits != 0)
        {
            // Find the index of the lowest set bit (e.g., 00100 -> 2)
            int index = math.tzcnt(remainingBits);
            SoundParams.Value[index] = new Vector2Int(MemValue, RayValue);
            // Remove that bit so we can find the next one
            // (1 << index) creates the mask for that bit, XOR (^) toggles it off
            remainingBits ^= 1u << index;
        }
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

