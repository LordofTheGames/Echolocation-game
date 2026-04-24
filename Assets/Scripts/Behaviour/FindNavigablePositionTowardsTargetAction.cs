using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable Position towards Target", story: "Set [FinalPoint] to navigable position from [Agent] towards [Target] at distance [Distance]", category: "Action", id: "ed57fcf91f3a8953bca8517457ed2c92")]
public partial class FindNavigablePositionTowardsTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Target;
    [SerializeReference] public BlackboardVariable<Vector3> FinalPoint;
    [SerializeReference] public BlackboardVariable<float> Distance = new BlackboardVariable<float>(10.0f);

    private float checkCount;
    private Vector3 towardsDir;
    private Vector3 targetPosition;
    private float currAngle;
    private float currDistance;
    private NavMeshAgent agent;

    protected override Status OnStart()
    {
        agent = Agent.Value.GetComponent<NavMeshAgent>();
        checkCount = 0;
        currAngle = 10;
        currDistance = Distance;
        towardsDir = Target.Value - Agent.Value.transform.position;
        towardsDir.y = 0;
        towardsDir = towardsDir.normalized;
        targetPosition = towardsDir * Distance;
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
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    FinalPoint.Value = hit.position;
                    return Status.Success;
                }
            }
        }
        if (checkCount == 5)
        {
            currAngle += 10;
            checkCount = 0;
            FinalPoint.Value = Agent.Value.transform.position;
            if (currAngle >= 45)
            {
               currAngle = 10;
               currDistance -= 10;
                if (currDistance <= 0)
                {
                    FinalPoint.Value = Agent.Value.transform.position;
                    return Status.Failure;
                }
                else
                {
                    targetPosition = towardsDir * Distance;
                    return Status.Running;
                }
            }
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

