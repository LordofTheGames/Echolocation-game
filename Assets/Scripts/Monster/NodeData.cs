using UnityEditor.EditorTools;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    [Header("Only needed if node is exit node")]
    public bool isExitNode = false;
    public Area nextArea;
    [Header("Only needed if node is exit node and is linking from tunnel to area")]
    public ExitNodeLink tunnelEndLink;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
