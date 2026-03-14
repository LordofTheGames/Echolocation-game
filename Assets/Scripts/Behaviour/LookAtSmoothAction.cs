using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAt smooth", story: "[Agent] smoothly turns to [Target]", category: "Action", id: "3b071c53d53cd2fcd5e64be0de608a95")]
public partial class LookAtSmoothAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(5.0f);

    private Quaternion targetRotation;
    private Transform agentTransform;

    protected override Status OnStart()
    {
        agentTransform = Agent.Value.transform;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // re-calculate rotation every frame as target may have moved
        Vector3 direction = Target.Value.transform.position - agentTransform.position;
        direction.y = 0; // Limit rotation to y-axis
        if (direction != Vector3.zero) 
            targetRotation = Quaternion.LookRotation(direction);
        else 
            targetRotation = agentTransform.rotation; 

        float angle = Quaternion.Angle(agentTransform.rotation, targetRotation);
        // If we are done, exit early
        if (angle < 0.5f)
        {
            agentTransform.rotation = targetRotation;
            return Status.Success;
        }
        // Calculate speed based on how far away we are.
        // If angle is 90, speed is High. If angle is 1, speed is Low (but clamped to a minimum).
        // This mimics Slerp but ensures we never drop below '20f' speed.
        float dynamicSpeed = Mathf.Lerp(20f, Speed.Value * 50f, angle / 90f);
        // Apply rotation
        agentTransform.rotation = Quaternion.RotateTowards(agentTransform.rotation, targetRotation, dynamicSpeed * Time.deltaTime);

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

