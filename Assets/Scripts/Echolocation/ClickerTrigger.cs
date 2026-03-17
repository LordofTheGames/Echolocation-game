using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Angle of projection (0 = line, 60 = cone, 360 = sphere)")]
    public float angle = 60f;
    
    [Header("Volume of sound (used for fading effect)")]
    public float visualVolume = 100f;
    [Header("Volume of sound (used for AI reactions)")]
    public float monsterVolume = 100f;

    [Header("Cooldown Settings")]
    [Tooltip("Time in seconds before the clicker can be used again")]
    public float cooldownTime = 1.5f;
    [Tooltip("Assign a UI image here to act as the reload bar")]
    public Image cooldownBar;
    [Tooltip("Colour of the shrinking reload bar")]
    public Color cooldownColor = Color.yellow;
    [Tooltip("Transparncy of cooldown bar: 0 (transparent) - 1 (opaque))")]
    [Range(0f, 1f)]
    public float transparency = 0.8f;

    private float nextAvailableTime = 0f; // Tracks when the player is allowed to use clicker again
    private RectTransform cooldownBarRect;
    private Vector2 newSizeDelta = new(300, 5);
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // Heard equally in both ears since it's coming from the player
        }

        if (cooldownBar != null)
        {
            cooldownBarRect = cooldownBar.GetComponent<RectTransform>();
            cooldownColor.a *= transparency;
            cooldownBar.color = cooldownColor;
            cooldownBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (cooldownBar != null)
        {
            if (Time.time < nextAvailableTime)
            {
                // Calculate remaining time and therefore new size of bar
                float timeRemaining = nextAvailableTime - Time.time;
                newSizeDelta.x = timeRemaining / cooldownTime * 300;
                cooldownBarRect.sizeDelta = newSizeDelta;
            }
            else if (cooldownBar.gameObject.activeSelf)
            {
                // Cooldown is finished - hide the bar and reset size
                cooldownBar.gameObject.SetActive(false);
                newSizeDelta.x = 300;
                cooldownBarRect.sizeDelta = newSizeDelta;
            }
        }
    }

    public void OnEcholocate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.time >= nextAvailableTime)
            {
                if (clickerSound != null)
                {
                    audioSource.pitch = Random.Range(0.98f, 1.02f); // Vary pitch very slightly each time
                    audioSource.PlayOneShot(clickerSound, soundEffectVolume);
                }
                GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, angle, uniformity, numRays, visualVolume, monsterVolume);

                // Start next cooldown timer
                nextAvailableTime = Time.time + cooldownTime;

                // Show and reset the bar
                if (cooldownBar != null && cooldownTime > 0)
                {
                    cooldownBar.color = cooldownColor;
                    cooldownBar.gameObject.SetActive(true);
                    cooldownBar.fillAmount = 1f;
                }
            }
        }
    }
}
