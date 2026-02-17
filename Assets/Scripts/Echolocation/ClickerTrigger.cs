using UnityEngine;
using UnityEngine.InputSystem;

public class ClickerTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip clickerSound;
    [Range(0f,10f)]
    public float soundEffectVolume = 5f;
    public AudioSource audioSource; // If not assigned on is auto created

    [Header("Pulse Origins")]
    public Transform cameraTransform; // Assign main camera here

    [Header("Echo Projection Uniformity (0 = clumped, 1 = uniform)")]
    public float uniformity = 1.0f;

    [Header("Number of Rays")]
    public int numRays = 4000;

    [Header("Ray Max Distance")]
    [Range(0f, 50f)]
    public float maxDistance = 50f;

    [Header("Angle of projection (0 = line, 60 = cone, 360 = sphere)")]
    public float angle = 60f;
    
    [Header("Volume of sound (used for AI reactions)")]
    public float volume = 100f;
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // Heard equally in both ears since it's coming from the player
        }
    }

    public void OnEcholocate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (clickerSound != null)
            {
                audioSource.pitch = Random.Range(0.98f, 1.02f); // Vary pitch very slightly each time
                audioSource.PlayOneShot(clickerSound, soundEffectVolume);
            }
            GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, angle, uniformity, numRays, maxDistance, volume);
        }
    }
}
