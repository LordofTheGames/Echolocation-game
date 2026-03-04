using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Navigable Position around Target", story: "Set [FinalPoint] to navigable position on radius [Radius] from [Target]", category: "Action", id: "4b3db31b3aaf9aadbd4c073ce974e19d")]
public partial class FindNavigablePositionAroundTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
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
        Vector3 randomPosition = UnityEngine.Random.onUnitSphere;
        randomPosition.y = 0;
        randomPosition = randomPosition.normalized * Radius; // normalize so point is always at radius, not inside !
        randomPosition += Target.Value.transform.position;

        NavMeshHit hit;
        // if random position is not navigable, this is max distance SamplePosition will look to find navigable position
        float maxSearchDist = 4f;
        checkCount++;
        if(NavMesh.SamplePosition(randomPosition, out hit, maxSearchDist, NavMesh.AllAreas))
        {
            FinalPoint.Value = hit.position;
            return Status.Success;
        }
        else if (checkCount == 5)
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

