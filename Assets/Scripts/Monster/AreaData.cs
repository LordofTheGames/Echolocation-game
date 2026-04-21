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

    public List<NodeData> ExitNodes;

    private Movement movementScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movementScript = GameObject.Find("Monster").GetComponent<Movement>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Monster")
        {
            movementScript.SetArea(name);
        }
    }
}
