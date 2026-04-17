using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Ping global echo system", story: "[Agent] pings Global Echo System and plays sound", category: "Action", id: "5cc5cdc17b8bdd296e8aef1058ea1b65")]
public partial class PingGlobalEchoSystemAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

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

    [SerializeReference] public BlackboardVariable<Vector3> positionOffset = new BlackboardVariable<Vector3>(new(0, 1, 0));

    [SerializeReference] public BlackboardVariable<AudioClip> sound1;
    [SerializeReference] public BlackboardVariable<AudioClip> sound2;
    [SerializeReference] public BlackboardVariable<float> soundVolume = new BlackboardVariable<float>(1);
    [SerializeReference] public BlackboardVariable<AudioSource> audioSource;

    protected override Status OnStart()
    {
        int soundNum = UnityEngine.Random.Range(1, 3);
        if (soundNum == 1) audioSource.Value.PlayOneShot(sound1, soundVolume);
        if (soundNum == 2) audioSource.Value.PlayOneShot(sound2, soundVolume);

        GlobalEchoSystem.Ping(Agent.Value, Agent.Value.transform.position + positionOffset, Agent.Value.transform.forward, angle, uniformity, numRays, visualVolume, monsterVolume, isMonsterEcholocation: true);
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

