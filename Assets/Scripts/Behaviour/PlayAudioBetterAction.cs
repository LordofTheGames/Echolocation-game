using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Rendering;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Play audio better", story: "Play [Clip] using [audioSource]", category: "Action", id: "a7bc09adecebd9c87d46bc8155056766")]
public partial class PlayAudioBetterAction : Action
{
    [SerializeReference] public BlackboardVariable<AudioClip> Clip;
    [SerializeReference] public BlackboardVariable<AudioSource> audioSource;
    [SerializeReference] public BlackboardVariable<float> Volume = new BlackboardVariable<float>(1.0f);

    protected override Status OnStart()
    {
        audioSource.Value.PlayOneShot(Clip, Volume);
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

