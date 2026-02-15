using UnityEngine;

public class MicToEcholocation : MonoBehaviour
{
    public MicInput mic;
    public Transform cameraTransform;

    private int maxRays = 20000;
    private int minRays = 100;
    private float timer;

    [Header("Detection Interval")]
    public float interval = 0.2f;

    [Header("Angle (0-360)")]
    public float angle = 200f;

    [Header("Uniformity (0-1)")]
    public float uniformity = 0.3f;
    public float volumeScale = 350f;
     public float maxVolume = 300f;

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

        float volume = mic.loudness * volumeScale;
        volume = Mathf.Clamp(volume, 0f, maxVolume);
        GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, 200f, 0.3f, rays, volume);
    }
}
