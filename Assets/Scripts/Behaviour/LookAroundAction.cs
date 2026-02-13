using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look around", story: "[Agent] smoothly looks around with angle [Angle] turning [InitialDirection] first", category: "Action", id: "5826bfab915f70528e58fb058190ff82")]
public partial class LookAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Angle;
    [SerializeReference] public BlackboardVariable<LookDirection> InitialDirection;
    [SerializeReference] public BlackboardVariable<float> Speed  = new BlackboardVariable<float>(1.0f);
    [SerializeReference] public BlackboardVariable<int> NumberOfRotations  = new BlackboardVariable<int>(2);

    private enum LookPhase { ToLeft, ToRight, ToCenter }
    private LookPhase currentPhase;
    private int rotationsCompleted;

    private Quaternion centerRotation; // The rotation when we started
    private Quaternion targetRotation;   // Where we are going in the current swing
    private Transform agentTransform;

    protected override Status OnStart()
    {
        rotationsCompleted = 0;
        agentTransform = Agent.Value.transform;
        centerRotation = agentTransform.rotation;
        if (InitialDirection == LookDirection.RANDOM)
        {
            // pick random direction (between 0 and 1)
            InitialDirection.Value = (LookDirection) new System.Random().Next(0, 2);
        }

        if (InitialDirection == LookDirection.LEFT) 
        {
            currentPhase = LookPhase.ToLeft;
            targetRotation = centerRotation * Quaternion.Euler(0, -Angle.Value, 0);
        }
        else if (InitialDirection == LookDirection.RIGHT)
        {
            currentPhase = LookPhase.ToRight;
            targetRotation = centerRotation * Quaternion.Euler(0, Angle.Value, 0);
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float angle = Quaternion.Angle(agentTransform.rotation, targetRotation);
        // If we are done, start next phase or exit
        if (angle < 0.5f) 
        {
            // snap to rotation
            agentTransform.rotation = targetRotation;
            rotationsCompleted++;
            if (rotationsCompleted == NumberOfRotations && currentPhase != LookPhase.ToCenter)
            {
                // Return to Center
                currentPhase = LookPhase.ToCenter;
                targetRotation = centerRotation;
            }
            else if (currentPhase == LookPhase.ToLeft)
            {
                // Go to Right
                currentPhase = LookPhase.ToRight;
                targetRotation = centerRotation * Quaternion.Euler(0, Angle.Value, 0);
            }
            else if (currentPhase == LookPhase.ToRight)
            {
                // Go to Left
                currentPhase = LookPhase.ToLeft;
                targetRotation = centerRotation * Quaternion.Euler(0, -Angle.Value, 0);
            }
            else if (currentPhase == LookPhase.ToCenter)
            {
                return Status.Success;
            }
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

