using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Repeat N times", story: "Repeat [Count] times", category: "Flow", id: "3e039b5a506d3d10239de024d36eacd6")]
public partial class RepeatNTimesModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<int> Count;

    private int count;

    protected override Status OnStart()
    {
        count = 0;

        var status = StartNode(Child);
        if (status == Status.Failure || status == Status.Success)
            return Status.Running;
        return Status.Waiting;
    }

    protected override Status OnUpdate()
    {
        count++;
        Status status = Child.CurrentStatus;
        if (status == Status.Failure || status == Status.Success)
        {
            if (count == Count)
                return status;

            var newStatus = StartNode(Child);
            if (newStatus == Status.Failure || newStatus == Status.Success)
                return Status.Running;
        }
        return Status.Waiting;
    }

    protected override void OnEnd()
    {
    }
}

