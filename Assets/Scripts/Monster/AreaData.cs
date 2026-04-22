using System;
using System.Collections.Generic;
using UnityEngine;

public class AreaData : MonoBehaviour
{
    public bool isTunnel = false;
    public bool hasFork = false;
    public int forkStart1;
    public int forkEnd1;
    public int forkStart2;
    public int forkEnd2;
    public int forkStart3;
    public int forkEnd3;

    [Header("Exit nodes should be IN the nextArea\n(and as such cross the boundary)")]
    public List<NodeData> ExitNodes;
}
