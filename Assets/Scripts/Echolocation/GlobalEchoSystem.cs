using UnityEngine;

public class GlobalEchoSystem : MonoBehaviour
{

    // Safe singleton structure for public access to create instances of EcholocationSystem prefab to spawn rays at certain locations

    private static GlobalEchoSystem _instance; // Holds the actual system but is private to prevent accidental changes

    public static GlobalEchoSystem Instance // public to allow global usage of the instance
    {
        get  { return _instance; }
    }


    // Prevents more than one GlobalEchoSystem from being created

    private void Awake() 
    {
        if (_instance != null && _instance != this) // If one already exists and it/s not me, I must destroy myself
        {
            Destroy(this.gameObject);
        }
        else // If one doesn't exist yet, I can assign myself to that position
        {
            _instance = this;
        }
    }

    // Actual funcitonality

    [Header("Settings")]
    public GameObject echoSystemPrefab; // Drag EcholocationSystem.prefab here in inspector window

    public static void Ping(GameObject sourceObject, Vector3 position) // Default overload that use spherical rays
    {
        Ping(sourceObject, position, Vector3.forward, 360f, 1.0f, 20000, 50f, 100f);
    }

    // sourceObject: the object spawning the ray that should be ignored on first projection to prevent self-collision, can be hit on reflections. If you really don't want it for whatever reson, pass in "null".
    // position: the position to spawn the rays at
    // direction: the direction to project the rays in/around
    // angle: (takes a value 0-360 degrees) the angle around direction to spawn rays within, e.g. 0 = along the line, 60 = a cone around the line, 360 = a complete sphere
    // uniformity: (takes a value 0-1) how clumped around the directino line rays should be, 0 = clumped completely, 1 = completely uniform. 1 used for spheres for uniform projection in all directions, for cones etc. clumped = 0 gives a more realistic sound effect (sound dies out futher from main direction)
    // numRays: number of rays to spawn
    public static void Ping(GameObject sourceObject, Vector3 position, Vector3 direction, float angle, float uniformity, int numRays, float maxDistance, float volume, bool isFootsteps = false) // Can add things later like: shape of projection + rotation, loudness, pitch, num rays (if not dependent on other things), etc.
    {
        // Safety check for existence of GlobalEchoSystem and EcholocationSystem.prefab (via GlobalEchoManager GameObject) 
        if (Instance != null && Instance.echoSystemPrefab != null)
        {
            Instance.SpawnPulse(sourceObject, position, direction, angle, uniformity, numRays, maxDistance, volume, isFootsteps);
        }
        else
        {
            Debug.Log("GlobalEchoSystem is missing! Make sure you have a GlobalEchoManager GameObject in scene.");
        }
    }

    // Takes in an angle between 0 and 360 degrees for shape of projection, and direction for direction to project in  
    void SpawnPulse(GameObject sourceObject, Vector3 position, Vector3 direction, float angle, float uniformity, int numRays, float maxDistance, float volume, bool isFootsteps)
    {
        GameObject pulse = Instantiate(echoSystemPrefab, position, Quaternion.identity); // Create (Instantiate) an instance of the EcholocationSystem.prefab at position, with rotation ... (identity means no rotation)

        EcholocationManager manager = pulse.GetComponent<EcholocationManager>();

        // Can add things later like: shape of projection + rotation, loudness, pitch, num rays (if not dependent on other things), etc.
        // For example: manager.maxDistance = loudness * 0.5;

        // Unity's main thread, upon Instantiate(), instantly creates the GameObject and calls its Awake() method,
        // it then hands control back to the SpawnPulse() method,
        // and once it finsihes execution the main thread is freed again and the GameObject's Start() method is called,
        // so you can safely change variables of the manager here before the rays are actually fired.

        if (manager != null)
        {
            manager.SetupScan(sourceObject, direction, angle, uniformity, numRays, maxDistance, volume, isFootsteps);
        }
    }
}
