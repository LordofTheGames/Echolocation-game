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
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(1.0f);
    [SerializeReference] public BlackboardVariable<int> NumberOfRotations = new BlackboardVariable<int>(2);

    [SerializeReference] public BlackboardVariable<bool> PingEchoSystem = new BlackboardVariable<bool>(true);
    [SerializeReference] public BlackboardVariable<float> uniformity = new BlackboardVariable<float>(0);
    [SerializeReference] public BlackboardVariable<int> numRays = new BlackboardVariable<int>(20000);
    [SerializeReference] public BlackboardVariable<float> echoAngle = new BlackboardVariable<float>(80);
    [SerializeReference] public BlackboardVariable<float> visualVolume = new BlackboardVariable<float>(100);
    [SerializeReference] public BlackboardVariable<float> monsterVolume = new BlackboardVariable<float>(0);
    [SerializeReference] public BlackboardVariable<Vector3> MonsterHeightOffset;
    [SerializeReference] public BlackboardVariable<AudioClip> sound1;
    [SerializeReference] public BlackboardVariable<AudioClip> sound2;
    [SerializeReference] public BlackboardVariable<float> soundVolume = new BlackboardVariable<float>(1);
    [SerializeReference] public BlackboardVariable<AudioSource> audioSource;
    [SerializeReference] public BlackboardVariable<MonsterSearchMode> searchMode;

    private enum LookPhase { ToLeft, ToRight, ToCenter }
    private LookPhase currentPhase;
    private int rotationsCompleted;

    private Quaternion centerRotation; // The rotation when we started
    private Quaternion targetRotation;   // Where we are going in the current swing
    private Transform agentTransform;

    private Quaternion echoRotation;

    protected override Status OnStart()
    {
        Speed.Value = 2;
        Angle.Value = 30;
        InitialDirection.Value = LookDirection.RIGHT;
        NumberOfRotations.Value = 2;

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

        echoRotation = agentTransform.rotation;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // float angle = Quaternion.Angle(agentTransform.rotation, targetRotation);
        float angle = Quaternion.Angle(echoRotation, targetRotation);
        // If we are done, start next phase or exit
        if (angle < 0.5f) 
        {
            // snap to rotation
            // agentTransform.rotation = targetRotation;
            echoRotation = targetRotation;
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

            // GlobalEchoSystem.Ping(Agent.Value, Agent.Value.transform.position + MonsterHeightOffset, Agent.Value.transform.forward, echoAngle, uniformity, numRays, visualVolume, monsterVolume, isMonsterEcholocation: true, monsterSearchMode: MonsterSearchMode.RED);
            GlobalEchoSystem.Ping(Agent.Value, Agent.Value.transform.position + MonsterHeightOffset, echoRotation * Agent.Value.transform.forward, echoAngle, uniformity, numRays, visualVolume, monsterVolume, isMonsterEcholocation: true, monsterSearchMode: searchMode);
            int soundNum = UnityEngine.Random.Range(1, 3);
            if (soundNum == 1) audioSource.Value.PlayOneShot(sound1, soundVolume);
            if (soundNum == 2) audioSource.Value.PlayOneShot(sound2, soundVolume);
        }
        // Calculate speed based on how far away we are.
        // If angle is 90, speed is High. If angle is 1, speed is Low (but clamped to a minimum).
        // This mimics Slerp but ensures we never drop below '20f' speed.
        float dynamicSpeed = Mathf.Lerp(20f, Speed.Value * 50f, angle / 90f);
        // Apply rotation
        // agentTransform.rotation = Quaternion.RotateTowards(agentTransform.rotation, targetRotation, dynamicSpeed * Time.deltaTime);
        echoRotation = Quaternion.RotateTowards(echoRotation, targetRotation, dynamicSpeed * Time.deltaTime);

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

