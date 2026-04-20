using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public struct Area
{
    public string name;
    public bool isTunnel;
}

public struct ExitNode
{
    public Vector3 position;
    public Vector3 areaEnterPosition; // TODO: what about this?!?!?!?!
    public Area nextArea;
}


public class Movement : MonoBehaviour
{
    private Dictionary<Area, List<Vector3>> nodes;
    private Dictionary<Area, List<ExitNode>> exitNodes;

    private Area currentArea;
    private Vector3 currentNode;
    private int currentNodeIdx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject nodesObj = GameObject.Find("Monster Nav Nodes");
        nodes = new Dictionary<Area, List<Vector3>>();
        exitNodes = new Dictionary<Area, List<ExitNode>>();

        foreach(Transform areaObj in nodesObj.transform)
        {
            AreaData ad = areaObj.GetComponent<AreaData>();
            Area area = new Area {name = areaObj.name, isTunnel = ad.isTunnel};

            nodes[area] = new List<Vector3>();
            exitNodes[area] = new List<ExitNode>();

            foreach (Transform nodeObj in areaObj)
            {
                NodeData n = nodeObj.GetComponent<NodeData>();
                if (n.isExitNode)
                {
                   exitNodes[area].Add(new ExitNode{position = nodeObj.position, nextArea = n.nextArea}); 
                }
                nodes[area].Add(nodeObj.position);
            }
        }
        
    }

    public Vector3 NextNode() // TODO: low(er) chance of going to a node already been to in this room, until been to all nodes (or lower chance of turning around for corridors)
    {
        if (currentArea.isTunnel == false)
        {
            int nextNodeIdx = currentNodeIdx;
            while(nextNodeIdx == currentNodeIdx) nextNodeIdx = UnityEngine.Random.Range(0, nodes[currentArea].Count);
            currentNode = nodes[currentArea][nextNodeIdx];
            return currentNode;
        }
        else
        {
            return nodes[currentArea][0];
        }

    }

    public Vector3 GetClosestNode(Vector3 position)
    {
        // TODO: have a system for changing the current area automatically when the monster moves between them when chasing player?
        // that way, we don't need this function as we can just make it walk in the current area, (and in the direction the player went if it is a tunnel)
        return nodes[currentArea][0];
    }

    public Vector3 NextArea() // TODO: low(er) chance of going into an area just been in
    {

        return nodes[currentArea][0];
    }

    // TODO: go close to player - should be some chance of going to room with player in, else go to adjacent rooms??? or nodes close to player?? or nodes close to "room that player's in"'s exits?
}
