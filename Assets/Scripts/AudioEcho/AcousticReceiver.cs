using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(CsoundUnity))]
public class AcousticReceiver : MonoBehaviour
{
    public static AcousticReceiver Instance;

    [Header("Listener Settings")]
    public float transitionSpeed = 3f; // How fast the sphere grows/shrinks between rooms
    public float defaultRadius = 1f;  // Fallback size
    [Range(0f,10f)]
    public float targetRadius = 1f;   // So size can be changed in play

    private SphereCollider listenerCollider;
    private CsoundUnity csound;


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
        // Follow the pplayer's head
        if (Camera.main != null)
        {
            transform.position = Camera.main.transform.position;
        }

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

    private int hitNumber = 1;
    public void ReceiveAcousticHit(AcousticHit hit)
    {
        Debug.Log("Hit Number: " + hitNumber);
        hitNumber++;
        // Syntax: i [instrument] [start_time] [duration] [p4] [p5] [p6] ...
        // We set duration to 5.0 seconds so the instrument automatically deletes itself after the reverb tail finishes
        
        string scoreEvent = $"i 1 0 5.0 {hit.timeDelay} {hit.finalEnergy.b63} {hit.finalEnergy.b125} {hit.finalEnergy.b250} {hit.finalEnergy.b500} {hit.finalEnergy.b1k} {hit.finalEnergy.b2k} {hit.finalEnergy.b4k} {hit.finalEnergy.b8k} {hit.finalEnergy.b16k}";
        
        csound.SendScoreEvent(scoreEvent);
    }

    // This draws a visual representation of the sphere in the editor
    void OnDrawGizmos()
    {
        if (listenerCollider != null)
        {
            // Create a transparent blue color (Red, Green, Blue, Alpha)
            // Alpha is 0.3f, meaning 30% opaque
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f); 
            
            // Draw the sphere exactly where the collider is, using its exact radius
            Gizmos.DrawSphere(transform.position, listenerCollider.radius);
        }
    }
}
