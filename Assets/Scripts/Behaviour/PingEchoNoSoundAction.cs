using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Ping echo NO SOUND", story: "[Agent] pings Global Echo System (no sound)", category: "Action", id: "f2c8d2b8493bbb7ba0887eb792d99e24")]
public partial class PingEchoNoSoundAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [Tooltip("Echo Projection Uniformity (0 = clumped, 1 = uniform)")]
    [SerializeReference] public BlackboardVariable<float> uniformity = new BlackboardVariable<float>(0);

    [Tooltip("Number of Rays")]
    [SerializeReference] public BlackboardVariable<int> numRays = new BlackboardVariable<int>(20000);

    [Tooltip("Angle of projection (0 = line, 60 = cone, 360 = sphere)")]
    [SerializeReference] public BlackboardVariable<float> angle = new BlackboardVariable<float>(100f);
    
    [Tooltip("Volume of sound (used for fading effect)")]
    [SerializeReference] public BlackboardVariable<float> visualVolume = new BlackboardVariable<float>(100f);
    [Tooltip("Volume of sound (used for AI reactions)")]
    [SerializeReference] public BlackboardVariable<float> monsterVolume = new BlackboardVariable<float>(0);

    [SerializeReference] public BlackboardVariable<Vector3> MonsterHeightOffset;
    [SerializeReference] public BlackboardVariable<MonsterSearchMode> searchMode;

    protected override Status OnStart()
    {
        GlobalEchoSystem.Ping(Agent.Value, Agent.Value.transform.position + MonsterHeightOffset, Agent.Value.transform.forward, angle, uniformity, numRays, visualVolume, monsterVolume, isMonsterEcholocation: true, monsterSearchMode: searchMode);
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

