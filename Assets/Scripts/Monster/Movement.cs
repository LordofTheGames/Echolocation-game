using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.UIElements;

public class Area
{
    public string name;
    public bool isTunnel;
    
    public List<Vector3> nodes = new List<Vector3>();
    public List<ExitNode> exitNodes = new List<ExitNode>();
    public TunnelPath tunnelPath; 
}

public struct ExitNode
{
    public Vector3 position;
    public int index;
    public Area nextArea;
}

public enum Direction {Increasing, Decreasing};

public class TunnelPath
{
    public int triggeredNode;

    public bool hasFork;

    public int start;
    public int end;

    public int forkStart1;
    public int forkEnd1;
    public int forkStart2;
    public int forkEnd2;
    public int forkStart3;
    public int forkEnd3;

    public TunnelPath(int start, int end)
    {
        triggeredNode = start;
        hasFork = false;
        this.start = start;
        this.end = end;
        forkStart1 = 0;
        forkStart2 = 0;
        forkStart3 = 0;
        forkEnd1 = 0;
        forkEnd2 = 0;
        forkEnd3 = 0;
    }
    public TunnelPath(int forkStart1, int forkEnd1, int forkStart2, int forkEnd2, int forkStart3, int forkEnd3)
    {
        triggeredNode = forkEnd1;
        hasFork = true;
        start = 0;
        end = 0;
        this.forkStart1 = forkStart1;
        this.forkStart2 = forkStart2;
        this.forkStart3 = forkStart3;
        this.forkEnd1 = forkEnd1;
        this.forkEnd2 = forkEnd2;
        this.forkEnd3 = forkEnd3;
    }
}


public class Movement : MonoBehaviour
{
    private List<Area> areas;

    private Area currentArea;
    private Area lastArea;
    private Vector3 currentNode;
    private int currentNodeIdx;
    private List<int> unvisitedNodes;
    private List<int> visitedNodes;

    private Direction currTunnelMoveDir = Direction.Increasing;
    private int directionChangeCount = 0;
    private float directionChangeProb = 0.3f;
    private bool firstTunnelMove = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject nodesObj = GameObject.Find("Monster Nav Nodes");
        areas = new List<Area>();
        visitedNodes = new List<int>();
        unvisitedNodes = new List<int>();

        foreach(Transform areaObj in nodesObj.transform)
        {
            AreaData ad = areaObj.GetComponent<AreaData>();
            Area area = new Area {name = areaObj.name, isTunnel = ad.isTunnel};
            areas.Add(area);

            area.nodes = new List<Vector3>();
            area.exitNodes = new List<ExitNode>();

            foreach (Transform nodeObj in areaObj)
            {
                NodeData n = nodeObj.GetComponent<NodeData>();
                area.nodes.Add(nodeObj.position);
            }
            foreach (NodeData exitnd in ad.ExitNodes)
            {
                area.exitNodes.Add(new ExitNode{position = exitnd.transform.position, index = exitnd.index, nextArea = exitnd.nextArea});
            }

            if (area.isTunnel)
            {
                if (ad.hasFork)
                    area.tunnelPath = new TunnelPath(ad.forkStart1, ad.forkEnd1, ad.forkStart2, ad.forkEnd2, ad.forkStart3, ad.forkEnd3);
                else
                    area.tunnelPath = new TunnelPath(0, area.nodes.Count - 1);
            }
        }

        lastArea = areas[0]; //Tutorial tunnel
        currentArea = areas[1]; //Bridge area
        for (int i = 0; i < currentArea.nodes.Count; i++)
            unvisitedNodes.Add(i);

        currentNodeIdx = -1; // make the first use of NextNode pick any node in bridge area!
    }

    public Vector3 NextNode() // TODO: low(er) chance of going to a node already been to in this room, until been to all nodes (or lower chance of turning around for corridors)
    {
        if (currentArea.isTunnel == false)
        {
            int nextNodeIdx = currentNodeIdx;
            while(nextNodeIdx == currentNodeIdx) 
            {
                if (unvisitedNodes.Count == 0) nextNodeIdx = PickRandomFromList(visitedNodes);
                else if (visitedNodes.Count == 0) nextNodeIdx = PickRandomFromList(unvisitedNodes);
                else
                {
                    float roll = UnityEngine.Random.value;
                    if (roll <= 0.7f) 
                        nextNodeIdx = PickRandomFromList(unvisitedNodes);
                    else 
                        nextNodeIdx = PickRandomFromList(visitedNodes);
                }
            }

            currentNodeIdx = nextNodeIdx;
            unvisitedNodes.Remove(currentNodeIdx);
            visitedNodes.Add(currentNodeIdx);
        }
        else
        {
            TunnelPath tp = currentArea.tunnelPath;

            // deal with at start/end
            // FIXME:
            bool atStartEnd = false;
            if (!firstTunnelMove && tp.hasFork)
            {
                if (currentNodeIdx == tp.forkEnd1)
                {
                    atStartEnd = true;
                }
                else if(currentNodeIdx == tp.forkEnd2)
                {
                    atStartEnd = true;
                }
                else if(currentNodeIdx == tp.forkEnd3)
                {
                    atStartEnd = true;
                }
            }
            else if (!firstTunnelMove)
            {
                if (currentNodeIdx == tp.start)
                {
                    atStartEnd = true;
                }
                else if(currentNodeIdx == tp.end)
                {
                    atStartEnd = true;
                }
            }

            // not at start/end
            if (!atStartEnd)
            {
                if (tp.hasFork && currentNodeIdx == tp.forkStart1)
                    calcForkNextNode(tp.forkStart1, tp.forkEnd1, tp.forkStart2, tp.forkEnd2, tp.forkStart3, tp.forkEnd3);
                else if (tp.hasFork && currentNodeIdx == tp.forkStart2)
                    calcForkNextNode(tp.forkStart2, tp.forkEnd2, tp.forkStart1, tp.forkEnd1, tp.forkStart3, tp.forkEnd3);
                else if (tp.hasFork && currentNodeIdx == tp.forkStart3)
                    calcForkNextNode(tp.forkStart3, tp.forkEnd3, tp.forkStart1, tp.forkEnd1, tp.forkStart2, tp.forkEnd2);
                else
                {
                    float roll = UnityEngine.Random.value;
                    if (roll <= directionChangeProb && directionChangeCount < 3) 
                    {
                        currTunnelMoveDir = (currTunnelMoveDir == Direction.Increasing) ? Direction.Decreasing : Direction.Increasing;
                        directionChangeCount++;
                        directionChangeProb -= 0.1f;
                    }
                    currentNodeIdx += (currTunnelMoveDir == Direction.Increasing) ? 1 : -1;
                }
            }
        }

        currentNode = currentArea.nodes[currentNodeIdx];
        return currentNode;
    }
    private void calcForkNextNode(int forkStart, int forkEnd, int forkStart1, int forkEnd1, int forkStart2, int forkEnd2)
    {
        TunnelPath tp = currentArea.tunnelPath;
        Direction towardsForkDir = (forkStart < forkEnd) ? Direction.Increasing : Direction.Decreasing;
        // if we are moving away from fork
        if (towardsForkDir != currTunnelMoveDir)
        {
            float roll = UnityEngine.Random.value;
            if (roll <= directionChangeProb && directionChangeCount < 3)
            {
                currTunnelMoveDir = towardsForkDir;
                directionChangeCount++;
                directionChangeProb -= 0.1f;
            }
            else
            {
                currentNodeIdx += (currTunnelMoveDir == Direction.Increasing) ? 1 : -1;
            }
        }
        if (towardsForkDir == currTunnelMoveDir)
        {
            float roll = UnityEngine.Random.value;
            if (roll <= 0.5f)
            {
                currTunnelMoveDir = (forkEnd1 < forkStart1) ? Direction.Decreasing : Direction.Increasing;
                currentNodeIdx = forkStart1;
            }
            else 
            {
                currTunnelMoveDir = (forkEnd2 < forkStart2) ? Direction.Decreasing : Direction.Increasing;
                currentNodeIdx = forkStart2;
            }
        }
    }

    // public Vector3 GetClosestNode(Vector3 position)
    // {
    //     // TODO: have a system for changing the current area automatically when the monster moves between them when chasing player?
    //     // that way, we don't need this function as we can just make it walk in the current area, (and in the direction the player went if it is a tunnel)
    //     return nodes[currentArea][0];
    // }

    // TODO: will only be called when exiting room (not tunnel)???????????????????????
    public Vector3 NextArea()
    {
        if (currentArea.isTunnel == false)
        {
            ExitNode lastAreaExit = new ExitNode();
            List<ExitNode> allOtherExits = new List<ExitNode>();
            foreach (ExitNode exitNode in currentArea.exitNodes)
            {
                if (exitNode.nextArea.name == lastArea.name) lastAreaExit = exitNode;
                else allOtherExits.Add(exitNode);
            }

            float roll = UnityEngine.Random.value;
            ExitNode chosenExit;
            if (roll <= 0.3f) 
                chosenExit = lastAreaExit;
            else 
                chosenExit = PickRandomFromList(allOtherExits);

            currentNode = chosenExit.position;
            currentNodeIdx = chosenExit.nextArea.nodes.IndexOf(currentNode);
            // if (chosenExit.nextArea.isTunnel)  WE CAN ASSUME THIS!!!
            // work out tunnel starting direction
            TunnelPath tp = chosenExit.nextArea.tunnelPath;
            if (tp.hasFork)
            {
                if (currentNodeIdx == tp.forkEnd1)
                    currTunnelMoveDir = (tp.forkEnd1 < tp.forkStart1) ? Direction.Increasing : Direction.Decreasing;
                else if (currentNodeIdx == tp.forkEnd2)
                    currTunnelMoveDir = (tp.forkEnd2 < tp.forkStart2) ? Direction.Increasing : Direction.Decreasing;
                else if (currentNodeIdx == tp.forkEnd3)
                    currTunnelMoveDir = (tp.forkEnd3 < tp.forkStart3) ? Direction.Increasing : Direction.Decreasing;
            }
            else
            {
                if (currentNodeIdx == tp.start)
                    currTunnelMoveDir = (tp.start < tp.end) ? Direction.Increasing : Direction.Decreasing;
                else if (currentNodeIdx == tp.end)
                    currTunnelMoveDir = (tp.start < tp.end) ? Direction.Decreasing : Direction.Increasing;
            }
        }
        else
        {
            // FIXME:            
        }
        return currentNode;
    }

    // TODO: go close to player - should be some chance of going to room with player in, else go to adjacent rooms??? or nodes close to player?? or nodes close to "room that player's in"'s exits?
    // Solution: call normal close to player function, then continue from new position doing room/tunnel logic (should all work due to auto room transitions)

    public void SetArea(string name)
    {
        Area newArea = areas.Find(area => area.name == name);
        if (newArea.name != currentArea.name)
        {
            lastArea = currentArea;
            currentArea = newArea;

            if (currentArea.isTunnel)
            {
                directionChangeCount = 0;
                directionChangeProb = 0.3f;
                firstTunnelMove = false;
            }
            else
            {
                visitedNodes.Clear();
                unvisitedNodes.Clear();
                for (int i = 0; i < currentArea.nodes.Count; i++)
                    unvisitedNodes.Add(i);
            }
        }
    }
    public void SetTunnelNode(int idx)
    {
        // currentNode = nodes[currentArea].Find(node => node == position);
        // currentNodeIdx = nodes[currentArea].IndexOf(currentNode);
        currentArea.tunnelPath.triggeredNode = idx;
    }

    private T PickRandomFromList<T>(List<T> targetList)
    {
        int randomIndex = UnityEngine.Random.Range(0, targetList.Count);
        return targetList[randomIndex];
    }
}
