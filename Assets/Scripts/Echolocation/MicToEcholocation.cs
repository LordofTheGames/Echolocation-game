using UnityEngine;
using UnityEngine.InputSystem;

public class MicToEcholocation : MonoBehaviour
{
    public MicInput mic;
    public Transform cameraTransform;

    private float numRaysScale = 0.8f;
    private int maxRays = 30000;
    private int minRays = 1000;
    private float maxVisualVolume = 300f;
    private float maxVolForMonster = 300f;
    private float timer;
    private float proportion;
    private float visualVolume;
    private float volForMonster;

    [Header("Settings")]
    public bool micToEchoEnabled = true;

    [Header("Detection Interval")]
    public float interval = 0.2f;

    [Header("Angle (0-360)")]
    public float angle = 200f;

    [Header("Uniformity (0-1)")]
    public float uniformity = 0.3f;

    // Update is called once per frame
    void Update()
    {
        if (!mic) return;
        timer += Time.deltaTime;
        if(!micToEchoEnabled) return;
        if (timer < interval) return;
        timer = 0f;
        if (mic.volume < 0.05f) return;

        int rays = Mathf.RoundToInt(mic.volume * maxRays * numRaysScale);
        rays = Mathf.Clamp(rays, minRays, maxRays);

        proportion = ((float) rays / maxRays);
        visualVolume = proportion * maxVisualVolume * 1.5f;    // Calculate visual volume of rays to travel based on proportion of max - gives a max distance of 50, loss per meter = 2
        volForMonster = proportion * maxVolForMonster * 1.5f;  // Calculate max volume for monster based on proportion of rays of max rays

        GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, angle, uniformity, rays, visualVolume, volForMonster);

    }
}
