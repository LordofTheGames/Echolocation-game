using UnityEngine;
using Unity.Behavior;

public class MazeTrigger : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        bool playerInMaze;
        agent.BlackboardReference.GetVariableValue("playerInMaze", out playerInMaze);
        agent.BlackboardReference.SetVariableValue("playerInMaze", !playerInMaze);
    }
}
