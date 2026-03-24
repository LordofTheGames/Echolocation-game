using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable Position with Decreasing Radius", story: "[Agent] finds navigable position at [FinalPoint] with max. radius [MaxRadius]", category: "Action", id: "5f5c4900a142469118da4718a16b92f9")]
public partial class FindNavigablePositionWithDecreasingRadiusAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> FinalPoint;
    [SerializeReference] public BlackboardVariable<float> MaxRadius = new BlackboardVariable<float>(10.0f);

    private float checkCount;
    private float currRadius;
    private NavMeshAgent agent;


    protected override Status OnStart()
    {
        agent = Agent.Value.GetComponent<NavMeshAgent>();
        checkCount = 0;
        currRadius = MaxRadius;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 randomPosition = UnityEngine.Random.onUnitSphere * MaxRadius;
        randomPosition.y = 0;
        randomPosition += Agent.Value.transform.position;

        NavMeshHit hit;
        // if random position is not navigable, max distance SamplePosition will look to find navigable position
        float maxSearchDist = 4f;
        checkCount++;
        if(NavMesh.SamplePosition(randomPosition, out hit, maxSearchDist, NavMesh.AllAreas))
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
            currRadius -= 2;
            checkCount = 0;
            FinalPoint.Value = Agent.Value.transform.position;
            if (currRadius <= 1) return Status.Failure;
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

