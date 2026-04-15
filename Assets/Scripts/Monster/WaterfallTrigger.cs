using UnityEngine;
using Unity.Behavior;

public class WaterfallTrigger : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private Transform waterfallExitLocation;
    [SerializeField] private GameObject respawnPoint;
    [SerializeField] private GameObject monsterRespawnPoint;

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
        if (other.CompareTag("Monster"))
        {
            bool inWaterfall;
            agent.BlackboardReference.GetVariableValue("inWaterfall", out inWaterfall);
            agent.BlackboardReference.SetVariableValue("inWaterfall", !inWaterfall);
            agent.BlackboardReference.SetVariableValue("waterfallExitLocation", waterfallExitLocation.position);
        }
        else if (other.CompareTag("Player"))
        {
            agent.BlackboardReference.SetVariableValue("playerInRespawn", false);
            agent.BlackboardReference.SetVariableValue("respawnPoint", respawnPoint);
            agent.BlackboardReference.SetVariableValue("monsterRespawnPoint", monsterRespawnPoint);
        }
    }
}
