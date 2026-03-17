using UnityEngine;
using Unity.Behavior;

public class TutorialTrigger : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            agent.BlackboardReference.SetVariableValue("playerInTutorial", false);
        }
    }
}
