using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    private PlayerFootsteps script;
    private SurfaceType lastSurface = SurfaceType.Default;
    void Start()
    {
        script = GameObject.FindWithTag("Player").GetComponent<PlayerFootsteps>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastSurface = script.currentSurface;
            script.currentSurface = SurfaceType.Water;
        } 
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) script.currentSurface = lastSurface;
    }
}
