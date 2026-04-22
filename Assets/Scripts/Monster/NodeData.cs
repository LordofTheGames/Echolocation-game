using UnityEditor.EditorTools;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    [Header("EXIT NODE INFO:\nExit nodes should be IN the nextArea\nand as such are used to direct the monster\nto cross the area boundary")]
    [Header("Only needed if node is exit node\n(this should be same as the area the node is in)")]
    public GameObject nextArea;
    [Header("Only needed if node is exit node and is linking from tunnel to area")]
    public ExitNodeLink tunnelEndLink = ExitNodeLink.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
