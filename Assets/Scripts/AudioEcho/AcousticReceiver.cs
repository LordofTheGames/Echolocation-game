using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(CsoundUnity))]
public class AcousticReceiver : MonoBehaviour
{
    public static AcousticReceiver Instance;

    [Header("Listener Settings")]
    public float transitionSpeed = 3f; // How fast the sphere grows/shrinks between rooms
    public float defaultRadius = 15f;  // Fallback size

    private SphereCollider listenerCollider;
    private CsoundUnity csound;

    private float targetRadius;


    void Awake()
    {
        Instance = this;
        listenerCollider = GetComponent<SphereCollider>();
        listenerCollider.isTrigger = true;
        csound = GetComponent<CsoundUnity>();

        targetRadius = defaultRadius;
        listenerCollider.radius = defaultRadius;
    }

    void Update()
    {
        // Smooth transition between different volume rooms, independent of framerate
        if (Mathf.Abs(listenerCollider.radius - targetRadius) > 0.01f)
        {
            listenerCollider.radius = Mathf.Lerp(listenerCollider.radius, targetRadius, 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime));
        }
    }

    // Called by invisible trigger boxes in game
    public void SetTargetRadius(float newRadius)
    {
        targetRadius = newRadius;
    }

    public void ReceiveAcousticHit(AcousticHit hit)
    {
        // Syntax: i [instrument] [start_time] [duration] [p4] [p5] [p6] ...
        // We set duration to 5.0 seconds so the instrument automatically deletes itself after the reverb tail finishes
        
        string scoreEvent = $"i 1 0 5.0 {hit.timeDelay} {hit.finalEnergy.b63} {hit.finalEnergy.b125} {hit.finalEnergy.b250} {hit.finalEnergy.b500} {hit.finalEnergy.b1k} {hit.finalEnergy.b2k} {hit.finalEnergy.b4k} {hit.finalEnergy.b8k} {hit.finalEnergy.b16k}";
        
        csound.SendScoreEvent(scoreEvent);
    }
}
