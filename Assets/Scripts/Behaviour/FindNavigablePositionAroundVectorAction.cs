using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable position around Vector", story: "Set [FinalPoint] to navigable position on radius [Radius] from [Vector]", category: "Action", id: "49a3b614ea795818b894a3c25d883128")]
public partial class FindNavigablePositionAroundVectorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Vector;
    [SerializeReference] public BlackboardVariable<Vector3> FinalPoint;
    [SerializeReference] public BlackboardVariable<float> Radius = new BlackboardVariable<float>(10.0f);

    private float checkCount;
    private NavMeshAgent agent;


    protected override Status OnStart()
    {
        checkCount = 0;
        Debug.Log("RESETTING");
        agent = Agent.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 randomPosition = UnityEngine.Random.onUnitSphere;
        randomPosition.y = 0;
        randomPosition = randomPosition.normalized * Radius; // normalize so point is always at radius, not inside !
        randomPosition += Vector;

        NavMeshHit hit;
        // if random position is not navigable, this is max distance SamplePosition will look to find navigable position
        float maxSearchDist = 4f;
        checkCount++;
        Debug.Log(checkCount);
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
            if (Agent.Value != null) FinalPoint.Value = Agent.Value.transform.position;
            return Status.Failure;
        }
        else 
        {
            if (Agent.Value != null) FinalPoint.Value = Agent.Value.transform.position;
            return Status.Running;
        }

    }

    protected override void OnEnd()
    {
    }
}

