using UnityEngine;
using Unity.Behavior;

public class WaterfallTrigger : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private Transform waterfallExitLocation;

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
        Debug.Log(other.tag);
        if (other.CompareTag("Monster"))
        {
            Debug.Log("HAHAH");
            bool inWaterfall;
            agent.BlackboardReference.GetVariableValue("inWaterfall", out inWaterfall);
            agent.BlackboardReference.SetVariableValue("inWaterfall", !inWaterfall);
            agent.BlackboardReference.SetVariableValue("waterfallExitLocation", waterfallExitLocation.position);
        }
    }
}
