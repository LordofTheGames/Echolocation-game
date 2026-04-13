using UnityEngine;
using Unity.Collections;

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

        // Starting capacity of 1024, grows if needed
        colliderColorMap = new NativeHashMap<int, int>(1024, Allocator.Persistent);
    }

    // Actual funcitonality

    [Header("Settings")]
    public GameObject echoSystemPrefab; // Drag EcholocationSystem.prefab here in inspector window

    public static void Ping(GameObject sourceObject, Vector3 position) // Default overload that use spherical rays
    {
        Ping(sourceObject, position, Vector3.forward, 360f, 1.0f, 20000, 100f, 100f);
    }

    // sourceObject: the object spawning the ray that should be ignored on first projection to prevent self-collision, can be hit on reflections. If you really don't want it for whatever reson, pass in "null".
    // position: the position to spawn the rays at
    // direction: the direction to project the rays in/around
    // angle: (takes a value 0-360 degrees) the angle around direction to spawn rays within, e.g. 0 = along the line, 60 = a cone around the line, 360 = a complete sphere
    // uniformity: (takes a value 0-1) how clumped around the directino line rays should be, 0 = clumped completely, 1 = completely uniform. 1 used for spheres for uniform projection in all directions, for cones etc. clumped = 0 gives a more realistic sound effect (sound dies out futher from main direction)
    // numRays: number of rays to spawn
    public static void Ping(GameObject sourceObject, Vector3 position, Vector3 direction, float angle, float uniformity, int numRays, float visualVolume, float monsterVolume, bool isFootsteps = false, float priority = 1) // Can add things later like: shape of projection + rotation, loudness, pitch, num rays (if not dependent on other things), etc.
    {
        // Safety check for existence of GlobalEchoSystem and EcholocationSystem.prefab (via GlobalEchoManager GameObject) 
        if (Instance != null && Instance.echoSystemPrefab != null)
        {
            Instance.SpawnPulse(sourceObject, position, direction, angle, uniformity, numRays, visualVolume, monsterVolume, isFootsteps, priority);
        }
        else
        {
            Debug.Log("GlobalEchoSystem is missing! Make sure you have a GlobalEchoManager GameObject in scene.");
        }
    }

    // Takes in an angle between 0 and 360 degrees for shape of projection, and direction for direction to project in  
    void SpawnPulse(GameObject sourceObject, Vector3 position, Vector3 direction, float angle, float uniformity, int numRays, float visualVolume, float monsterVolume, bool isFootsteps, float priority = 1)
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
            manager.SetupScan(sourceObject, direction, angle, uniformity, numRays, visualVolume, monsterVolume, isFootsteps, colliderColorMap, priority);
        }
    }

    // USED FOR MAIN MENU
    public static void PingCustomDuration(GameObject sourceObject, Vector3 position, float pulseDuration)
    {
        if (Instance != null && Instance.echoSystemPrefab != null)
            Instance.SpawnPulseCustomDuration(pulseDuration, sourceObject, position, Vector3.forward, 360, 1, 20000, 100, 100, false, 1);
        else
            Debug.Log("GlobalEchoSystem is missing! Make sure you have a GlobalEchoManager GameObject in scene.");
    }

    // Takes in an angle between 0 and 360 degrees for shape of projection, and direction for direction to project in  
    // Allows you to specify custom pulse duration
    // USED FOR MAIN MENU
    void SpawnPulseCustomDuration(float pulseDuration, GameObject sourceObject, Vector3 position, Vector3 direction, float angle, float uniformity, int numRays, float visualVolume, float monsterVolume, bool isFootsteps, float priority = 1)
    {
        GameObject pulse = Instantiate(echoSystemPrefab, position, Quaternion.identity); // Create (Instantiate) an instance of the EcholocationSystem.prefab at position, with rotation ... (identity means no rotation)

        EcholocationManager manager = pulse.GetComponent<EcholocationManager>();
        manager.pulseDuration = pulseDuration;

        // Can add things later like: shape of projection + rotation, loudness, pitch, num rays (if not dependent on other things), etc.
        // For example: manager.maxDistance = loudness * 0.5;

        // Unity's main thread, upon Instantiate(), instantly creates the GameObject and calls its Awake() method,
        // it then hands control back to the SpawnPulse() method,
        // and once it finsihes execution the main thread is freed again and the GameObject's Start() method is called,
        // so you can safely change variables of the manager here before the rays are actually fired.
        if (manager != null)
            manager.SetupScan(sourceObject, direction, angle, uniformity, numRays, visualVolume, monsterVolume, isFootsteps, colliderColorMap, priority);
    }



    // --- Layer to HashMap conversion ---

    private NativeHashMap<int, int> colliderColorMap;

    [Header("Layers To Detect")]
    public LayerMask monsterLayer;
    public LayerMask batLayer;
    public LayerMask interactableLayer;
    public LayerMask outlinedObjectLayer;
    public LayerMask metalLayer;
    public LayerMask dirtLayer;
    public LayerMask woodLayer;
    public LayerMask labLayer;
    public LayerMask railLayer;
    public LayerMask hideRockLayer;
    public LayerMask waterLayer;
    public LayerMask keyLayer;
    public LayerMask pillBoxLayer;

    void Start()
    {
        RegisterAllColliders();
    }

    void RegisterAllColliders()
    {
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Collider col in allColliders)
        {
            int category = GetCategoryFromLayer(col.gameObject.layer);
            colliderColorMap.TryAdd(col.GetInstanceID(), category);
        }
    }

    int GetCategoryFromLayer(int layer)
    {
        int layerMask = 1 << layer;

        // DO NOT CHANGE VALUES: only add more
        if ((monsterLayer.value & layerMask) > 0) return 1;         // Monster
        if ((interactableLayer.value & layerMask) > 0) return 2;    // Interactable
        if ((outlinedObjectLayer.value & layerMask) > 0) return 2;  // Interactable
        if ((metalLayer.value & layerMask) > 0) return 3;           // Metal
        if ((dirtLayer.value & layerMask) > 0) return 4;            // Dirt
        if ((woodLayer.value & layerMask) > 0) return 5;            // Wood
        if ((labLayer.value & layerMask) > 0) return 6;             // Lab
        if ((railLayer.value & layerMask) > 0) return 7;            // Rail
        if ((hideRockLayer.value & layerMask) > 0) return 8;        // HideRock
        if ((waterLayer.value & layerMask) > 0) return 9;           // Water
        if ((batLayer.value & layerMask) > 0) return 10;            // Bat
        if ((keyLayer.value & layerMask) > 0) return 11;            // Key
        if ((pillBoxLayer.value & layerMask) > 0) return 12;        // Pill Box
        return 0;                                                   // Default
    }

    // Runtime function to register a newly spawned collider 
    public void RegisterCollider(Collider col)
    {
        int category = GetCategoryFromLayer(col.gameObject.layer);
        int id = col.GetInstanceID();

        if (colliderColorMap.ContainsKey(id))
        {
            colliderColorMap[id] = category;
        }
        else
        {
            colliderColorMap.TryAdd(id, category);
        }
    }

    // Call in objects OnDestroy method to unregister - Unity can re-use instance ids after an object dies and a new one spawns
    public void UnregisterCollider(Collider col)
    {
        colliderColorMap.Remove(col.GetInstanceID());
    }

    void OnDestroy()
    {
        if (colliderColorMap.IsCreated) colliderColorMap.Dispose();
    }

}
