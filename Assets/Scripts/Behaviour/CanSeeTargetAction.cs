using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "can see target", story: "Check if [Agent] can see [Target] and update [TargetLastSeen] and [Boolean]", category: "Action", id: "77d048b0278674653ff686a26dc43d5b")]
public partial class CanSeeTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> TargetLastSeen;
    [SerializeReference] public BlackboardVariable<bool> Boolean;

    FieldOfViewChecker[] FOVScripts;

    private Vector3 secondLastSeenPosition;
    private bool lostTarget;
    private Transform lastSeenTransform;

    protected override Status OnStart()
    //TODO: CALLED EVERY FRAME!!!!!
    {
        FOVScripts = Agent.Value.GetComponents<FieldOfViewChecker>();
        lastSeenTransform = TargetLastSeen.Value.transform;
        lastSeenTransform.position = Agent.Value.transform.position;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        for (int i = 0; i < FOVScripts.Length; i++)
        {
            Boolean.Value = FOVScripts[i].FieldOfViewCheck(Target.Value);
            if (Boolean.Value) 
            {
                secondLastSeenPosition = lastSeenTransform.position;
                TargetLastSeen.Value.transform.position = Target.Value.transform.position;
                lostTarget = false;
            }
            else if (!lostTarget)
            {
                Quaternion rotation = Quaternion.LookRotation(lastSeenTransform.position - secondLastSeenPosition);
                // Limit rotation to y-axis
                rotation.x = 0;
                rotation.z = 0;
                lastSeenTransform.rotation = rotation;
                lostTarget = true;
            }
        }
        
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

