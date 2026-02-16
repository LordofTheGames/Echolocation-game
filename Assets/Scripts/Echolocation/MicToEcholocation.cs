using UnityEngine;

public class MicToEcholocation : MonoBehaviour
{
    public MicInput mic;
    public Transform cameraTransform;

    private int maxRays = 20000;
    private int minRays = 100;
    private float maxDistance = 50f;
    private float maxVolForMonster = 200f;
    private float timer;
    private float proportion;
    private float distance;
    private float volForMonster;

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
        if (timer < interval) return;
        timer = 0f;
        if (mic.loudness < 0.05f) return;

        int rays = Mathf.RoundToInt(mic.loudness * maxRays);
        rays = Mathf.Clamp(rays, minRays, maxRays);

        proportion = ((float) rays / maxRays);
        Debug.Log("Proportion: " + proportion);
        distance = proportion * maxDistance;            // Calculate max distance of rays to travel based on proportion of rays of max rays
        volForMonster = proportion * maxVolForMonster;  // Calculate max volume for monster based on proportion of rays of max rays

        Debug.Log("Rays: " + rays + "\nDistance: " + distance + "\nVolume for monster: " + volForMonster);

        GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, 200f, 0.3f, rays, distance, volForMonster);
    }
}
