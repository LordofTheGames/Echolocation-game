using UnityEngine;

public class MicToEcholocation : MonoBehaviour
{
    public MicInput mic;
    public Transform cameraTransform;

    private float interval = 0.5f;
    private int maxRays = 20000;
    private int minRays = 100;
    private float timer;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;
        if (mic.loudness < 0.05f) return;

        int rays = Mathf.RoundToInt(mic.loudness * maxRays);
        rays = Mathf.Clamp(rays, minRays, maxRays);
        GlobalEchoSystem.Ping(cameraTransform.position, cameraTransform.forward, 120f, 1f, rays);
    }
}
