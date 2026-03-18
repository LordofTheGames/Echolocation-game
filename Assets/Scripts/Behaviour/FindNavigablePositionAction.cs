using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable Position on Radius", story: "[Agent] finds navigable position at [FinalPoint] on radius [Radius]", category: "Action", id: "e54da2c2e5a9af96897aea6c83ff1418")]
public partial class FindNavigablePositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> FinalPoint;
    [SerializeReference] public BlackboardVariable<float> Radius = new BlackboardVariable<float>(10.0f);

    private float checkCount;

    protected override Status OnStart()
    {
        checkCount = 0;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 randomPosition = UnityEngine.Random.onUnitSphere * Radius;
        randomPosition.y = 0;
        randomPosition += Agent.Value.transform.position;

        NavMeshHit hit;
        // if random position is not navigable, max distance SamplePosition will look to find navigable position
        float maxSearchDist = 4f;
        checkCount++;
        if(NavMesh.SamplePosition(randomPosition, out hit, maxSearchDist, NavMesh.AllAreas))
        {
            FinalPoint.Value = hit.position;
            return Status.Success;
        }
        else if (checkCount == 5)
        {
            FinalPoint.Value = Agent.Value.transform.position;
            return Status.Failure;
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

