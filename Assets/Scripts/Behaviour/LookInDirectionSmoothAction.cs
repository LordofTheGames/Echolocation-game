using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look In Direction Smooth", story: "[Agent] smoothly turns to direction of [LookVector]", category: "Action", id: "27fd5f65789848e253f5f65f620569ef")]
public partial class LookInDirectionSmoothAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> LookVector;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(5.0f);

    private Quaternion targetRotation;
    private Transform agentTransform;

    protected override Status OnStart()
    {
        agentTransform = Agent.Value.transform;
        Vector3 direction = LookVector.Value;
        direction.y = 0; // Limit rotation to y-axis

        if (direction != Vector3.zero) 
            targetRotation = Quaternion.LookRotation(direction);
        else 
            targetRotation = agentTransform.rotation; 

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
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

