using UnityEngine;
using Unity.Behavior;

public class WaterfallTrigger : MonoBehaviour
{
    private BehaviorGraphAgent agent;
    [SerializeField] private Transform waterfallExitLocation;
    [SerializeField] private GameObject respawnPoint;
    [SerializeField] private GameObject monsterRespawnPoint;

    private bool firstEnter = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GameObject.Find("Monster").GetComponent<BehaviorGraphAgent>();
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

            if (firstEnter)
            {
                GameObject.Find("UIManager").GetComponent<SimpleOverlayMap>().AddStaticBlueDot(transform.GetChild(1).position);
                firstEnter = false;
            }
        }
    }
}
