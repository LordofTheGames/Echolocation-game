using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set sound param", story: "Set [SoundType] [SoundParam] to [Value]", category: "Action", id: "7a343249dc932edd2eb25809db5e19e8")]
public partial class SetSoundParamAction : Action
{
    [SerializeReference] public BlackboardVariable<SoundType> SoundType;
    [SerializeReference] public BlackboardVariable<SoundParamType> SoundParam;
    [SerializeReference] public BlackboardVariable<int> Value;
    [SerializeReference] public BlackboardVariable<List<Vector2Int>> SoundParams;

    protected override Status OnStart()
    {
        // copy so we can remove bits and not affect original
        uint remainingBits = (uint)SoundType.Value;
        while (remainingBits != 0)
        {
            // Find the index of the lowest set bit (e.g., 00100 -> 2)
            int index = math.tzcnt(remainingBits);
            if (SoundParam == SoundParamType.MemoryDuration)
                SoundParams.Value[index] = new Vector2Int(Value, SoundParams.Value[index].y);
            else if (SoundParam == SoundParamType.RayCountThreshold)
                SoundParams.Value[index] = new Vector2Int(SoundParams.Value[index].x, Value);
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

