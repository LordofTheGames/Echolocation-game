using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "update ai senses", story: "Update AI Senses", description: "Updates all the variables below (Except for Agent and Target)", category: "Action", id: "4b96d6fa266b06b15ec3a7ead8af5e72")]
        
public partial class UpdateAiSensesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> targetSeen;
    [SerializeReference] public BlackboardVariable<Vector3> lastLocation;
    [SerializeReference] public BlackboardVariable<Vector3> lastDirection;

    FieldOfViewChecker[] FOVScripts;
    Vector3 secondToLastLocation;
    bool initialised = false;

    protected override Status OnStart()
    {
        if (!initialised)
        {
            FOVScripts = Agent.Value.GetComponents<FieldOfViewChecker>();
            initialised = true;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        updateFOV();
        return Status.Success;
    }

    private void updateFOV()
    {
        for (int i = 0; i < FOVScripts.Length; i++)
        {
            if (FOVScripts[i].FieldOfViewCheck(Target))
            {
                targetSeen.Value = true;
                secondToLastLocation = lastLocation.Value;
                lastLocation.Value = Target.Value.transform.position;
                lastDirection.Value = lastLocation.Value - secondToLastLocation;
                return;
            }
        }
        targetSeen.Value = false;
    }

    protected override void OnEnd()
    {
    }
}

