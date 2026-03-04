using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable Position away from Target", story: "[Agent] finds navigable position away from [Target] at [FinalPoint] at distance [Distance]", category: "Action", id: "def0acda80fd891678833e56aa3b4bb8")]
public partial class FindNavigablePositionAwayFromTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Vector3> FinalPoint;
    [SerializeReference] public BlackboardVariable<float> Distance = new BlackboardVariable<float>(10.0f);

    private float checkCount;
    private Vector3 awayDir;
    private Vector3 targetPosition;
    private float currAngle;

    protected override Status OnStart()
    {
        checkCount = 0;
        currAngle = 10;
        awayDir = Agent.Value.transform.position - Target.Value.transform.position;
        awayDir.y = 0;
        awayDir = awayDir.normalized;
        targetPosition = awayDir * Distance;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 newPos = (Quaternion.AngleAxis(UnityEngine.Random.Range(-currAngle, currAngle), Vector3.up) * targetPosition) + Agent.Value.transform.position;

        NavMeshHit hit;
        // if random position is not navigable, max distance SamplePosition will look to find navigable position
        float maxSearchDist = 4f;

        checkCount++;
        if(NavMesh.SamplePosition(newPos, out hit, maxSearchDist, NavMesh.AllAreas))
        {
            FinalPoint.Value = hit.position;
            return Status.Success;
        }
        else if (checkCount == 5)
        {
            currAngle += 10;
            checkCount = 0;
            FinalPoint.Value = Agent.Value.transform.position;
            if (currAngle >= 90) return Status.Failure;
            else return Status.Running;
        }
        else 
        {
            FinalPoint.Value = Agent.Value.transform.position;
            return Status.Running;
        }

    }

    protected override void OnEnd()
    {
    }
}

