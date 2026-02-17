using UnityEngine;

public class ObjectWaterSplash : MonoBehaviour
{
    [Header("Visual & Audio Effects")]
    public ParticleSystem ripplePrefab;
    public AudioClip splashSound;
    public float volume = 0.8f;

    [Header("Settings")]
    public string playerTag = "Player"; 

    private Collider waterCollider;

    private void Start()
    {
        waterCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) return;

        if (other.attachedRigidbody != null)
        {
            // Calculate where the splash should spawn.
            Vector3 splashPos = other.transform.position;
            
            if (waterCollider != null)
            {
                splashPos.y = waterCollider.bounds.max.y + 0.01f; // offset just above water level
            }

            if (splashSound != null)
            {
                AudioSource.PlayClipAtPoint(splashSound, splashPos, volume);
            }

            if (ripplePrefab != null)
            {
                ParticleSystem newRipple = Instantiate(ripplePrefab, splashPos, ripplePrefab.transform.rotation);
                Destroy(newRipple.gameObject, newRipple.main.duration + newRipple.main.startLifetime.constantMax);
            }
        }
    }
}