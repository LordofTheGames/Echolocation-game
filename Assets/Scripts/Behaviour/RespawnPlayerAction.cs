using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Respawn player", story: "Respawn [Target] at [ObjectPosition]", category: "Action", id: "58bf0acf9c875af958df4f6842e441cc")]
public partial class RespawnPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> ObjectPosition;

    protected override Status OnStart()
    {
        PlayerRespawn script = Target.Value.GetComponent<PlayerRespawn>();
        if (script != null)
        {
            script.RespawnPosition = ObjectPosition.Value.transform.position;
            script.Respawn();
        }
        else Debug.LogError("TRYING TO RESPAWN ON OBJECT WITH NO RESPAWN SCRIPT");
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

