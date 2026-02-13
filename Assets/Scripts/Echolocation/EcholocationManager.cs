using UnityEngine;
using Unity.Collections;            // Native collections (NativeArray) for high-performance memory management
using Unity.Jobs;                   // The job system - allows us to run code on multiple CPU cores
using System.Collections.Generic;   // For use of dictionary

public class EcholocationManager : MonoBehaviour
{

    // Properties/variables (can edit in inspector windows in Unity)

    [Header("Pulse Settings")]
    public float pulseDuration = 2.0f;  // Duration of pulse that the visualisation fades over
    public int raysPerScan = 20000;     // Number of rays fired for a single scan
    public float maxDistance = 50f;     // Max distance rays can travel
    public LayerMask scanLayers;

    [Header("Reflection Settings")]
    [Tooltip("Maximum number of times sound rays will bounce before stopping")]
    public int maxBounces = 2;

    [Header("Visual Settings")]
    // Tooltip adds pop-up info when hovering mouse over the variable in the inspector
    [Tooltip("Controls the size of the dots in dot mode.")]
    public float dotScale = 0.2f;
    [Tooltip("Controls the size of the quad 'spotlight' in Grid mode.")]
    public float gridQuadSize = 5.0f;
    [Tooltip("Offset for dots in dot mode, smaller = better, just to prevent flickering.")]
    public float dotOffset = 0.01f;
    [Tooltip("Offset for 'windows' in grid mode, higher = better in concave areas.")]
    public float gridOffset = 0.5f;
    [Tooltip("How far from the quad/'window' the grid will project the grid (how far surfaces can be before the grid isn't painted on them).")]
    public float gridDepth = 10.0f;

    [Header("References")]
    public Mesh quadMesh;               // Holds the 3D shape data (vertices and triangles) - we're using a simple flat quad
    public Material scannerMaterial;    // Holds the Shader and Textures


    // Hidden GPU variables

    // ComputeBuffer is a special list that lives in the GPU
    private ComputeBuffer argsBuffer;       // Holds arguments for drawing (how many meshes to draw)
    private ComputeBuffer matrixBuffer;     // Holds the position/rotation/scale of every single mesh
    // NativeArray is a high-performance list used by the job system
    private NativeArray<RaycastCommand> commands;   // The "to do list" of raycasts
    private NativeArray<RaycastHit> results;        // The results
    private Matrix4x4[] instanceMatrices; // Holds position data before sending it to the GPU
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0}; // Array of 5 uints required by "DrawMeshInstancedIndirect" command
    private int activeHitCount = 0; // A counter to keep track of how many rays have actually hit a wall this frame


    // Timer variables

    private float spawnTime;
    private Material instanceMaterial;


    // Visualisation mode

    private bool isGridMode = false;


    // Scan direction and angle of project (from line to cone to sphere, ranging 0 to 360 degrees)
    private Vector3 scanDirection = Vector3.forward;
    private float scanAngle = 360f; // Defaults to sphere

    // How "clumped" rays are around line of direction, 1 is not at all/uniformly distributed across cone/sphere, 0 is naturally clumped around centre
    // Allows for customization, sound in cone in direction would naturally be more clumped along central line
    private float scanUniformity = 1.0f; 


    // Stop the rays colliding with the object that spawns them
    private GameObject objectToIgnore;  


    // Layer memory - to restore object+children's layers, after setting to IgnoreRaycast layer on first pulse, and reset before first reflections
    private Dictionary<Transform, int> layerMemory = new Dictionary<Transform, int>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Safety checks

        if (scannerMaterial == null)
        {
            Debug.LogError("EcholocationManager: 'Scanner Material' is missing! Please assign ScannerMat in the Inspector window.");
            Destroy(gameObject); //Cannot work without material
            return;
        }

        if (scannerMaterial.GetTexture("_DotTex") == null) Debug.LogError("Echolocation Manager: No Dot Texture has been assigned to the ScannerMat material!");
        if (scannerMaterial.GetTexture("_GridTex") == null) Debug.LogError("Echolocation Manager: No Grid Texture has been assigned to the ScannerMat material!");

        if (quadMesh == null)
        {
            Debug.Log("Echolocation manager: 'Quad Mesh' missing. Creating temporary primitive, please assign later.");
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        spawnTime = Time.time;

        // Clone the material - automtically inherits any textures assigned to scannerMaterial (ScannerMat)
        // Required for individual pulses to have their own fading "schedule"
        instanceMaterial = new Material(scannerMaterial);
        instanceMaterial.enableInstancing = true; // Force turn on instancing

        // Get visualisation mode
        float mode = scannerMaterial.GetFloat("_UseMesh");
        isGridMode= (mode > 0.5f);

        // Calculate falloff - only used in grid mode
        float totalFalloff = gridOffset + gridDepth;
        instanceMaterial.SetFloat("_Falloff", totalFalloff);

        long totalMaxHits = (long) raysPerScan * (maxBounces + 1);                                          // Calculate max number of rays/hits, including initial pulse and subsequent reflections
        int safeBufferSize = (int)Mathf.Min(totalMaxHits, 1000000);                                         // Prevenet a single pulse event from taking up to much VRAM

        instanceMatrices = new Matrix4x4[safeBufferSize];                                                      // Intialise theh array to hold "raysPerScan" number of matrices (positions)
        matrixBuffer = new ComputeBuffer(safeBufferSize, 64);                                                  // Create the GPU buffer - 64 is the "stride" (size of one 4x4 matrix in bytes = 16 floats * 4 bytes each)
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments); // Create arguments buffer, needs to hold 5 uints, the type tells the GPU this buffer doesn't contain 3D model data, only instructions for how to draw

        PerformScan();
        Destroy(gameObject, pulseDuration); // Destory this instance once the pulse duration has ended
    }

    // Update is called once per frame
    void Update()
    {
        if (activeHitCount <= 0 || instanceMaterial == null) return;

        // Fade out over time
        float lifePercent = (Time.time - spawnTime) / pulseDuration;
        float currentAlpha = Mathf.Lerp(1.0f, 0.0f, lifePercent);

        instanceMaterial.SetFloat("_GlobalVisibility", currentAlpha);
        RenderVisuals();
    }


    // Custom helper to calulate ray directions that are spherically and uniformally distributed, using a Fibonacci Sphere
    // Returns a Vector3 direction
    Vector3 GetFibonacciSphereDirection(int index, int totalPoints)
    {
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;            // Calculate and store the golden ratio
        float angleIncrement = 2 * Mathf.PI * goldenRatio;      // Calculate the angle step based on the golden ratio
        float t = (float)index / totalPoints;                   // Normalised height (from 0 to 1)
        float inclination = Mathf.Acos(1 - 2 * t);              // (arccos) Calculates inclination - up/down angle
        float azimuth = angleIncrement * index;                 // Calculates rotation around the vertical axis

        // Convert sphericle angles to Cartesian Coordinates
        float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
        float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
        float z = Mathf.Cos(inclination);

        return new Vector3(x, y, z);
    }

    // Defaults to uniform rays
    public void SetupScan(GameObject ignoreMe, Vector3 direction, float angle, float uniformity = 1.0f, int numRays = 4000, float volume = 10f)
    {
        // Check to prevent LookRotation(0,0,0) errors
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;

        scanDirection = direction.normalized;
        scanAngle = angle;
        scanUniformity = Mathf.Clamp01(uniformity);     // Make sure is in valid range
        raysPerScan = Mathf.Clamp(numRays, 0, 100000);   // Make sure is in (currently chosen) valid range
        objectToIgnore = ignoreMe;
        // TODO: remove this when multiple ray bounces have been implemented
        // for now just make the monster hear the sound
        INoiseSensitive sensitiveTarget = GameObject.FindGameObjectWithTag("Monster").GetComponent<INoiseSensitive>();
        sensitiveTarget.OnHeardScan(transform, volume);
    }



    // Hide and restore object that spawns the rays and itss children in hirearchy functions
    void HideAndSave(GameObject obj, int tempLayer)
    {
        layerMemory.Clear(); // Clear any stored layerMemrory, should be empty
        RecursiveHide(obj.transform, tempLayer);
    }

    void RecursiveHide(Transform t, int tempLayer)
    {
        // Save original layer if we havent seen this object before
        if (!layerMemory.ContainsKey(t))
        {
            layerMemory.Add(t, t.gameObject.layer);
        }

        t.gameObject.layer = tempLayer; // Temp change to layer 2/ignore raycast layer

        // Call for all children of the object
        foreach (Transform child in t)
        {
            RecursiveHide(child, tempLayer);
        }        
    }

    void RestoreLayers()
    {
        foreach (var kvp in layerMemory)                // Get each key-value pair in layer memory
        {
            if (kvp.Key != null)                        // Safety check
            {
                kvp.Key.gameObject.layer = kvp.Value;   // Restore original layers
            }
        }

        layerMemory.Clear();                            // Clear dictionary when done
    }



    // Fires the rays
    void PerformScan()
    {

        GameObject sourceObj = (objectToIgnore != null) ? objectToIgnore : this.gameObject;

        if (objectToIgnore != null)
        {
            HideAndSave(sourceObj, 9);  // Layer 9 is the custom Ignore Echolocation layer
            Physics.SyncTransforms();   // Force any layer changes to be applied now
        }

        // Create temporary memory for the job
        NativeList<Vector3> rayOrigins = new NativeList<Vector3>(raysPerScan, Allocator.TempJob);   // Allocator.TempJob keeps the buffer for 4 frames - however must return the key (call .Dispose()) when done to avoid memory leak warnings
        NativeList<Vector3> rayDirections = new NativeList<Vector3>(raysPerScan, Allocator.TempJob);
        NativeList<float> rayRanges = new NativeList<float>(raysPerScan, Allocator.TempJob);

        Vector3 startOrigin = transform.position; // Gets the position at which this instance of the EhcolocationSystem.prefab was instantiated in GlobalEchoSystem.cs
 
        // Prepare first layer of raycasts
        for (int i = 0; i < raysPerScan; i++)
        {
            rayOrigins.Add(startOrigin);    // Initial fire before reflections have the same origin
            rayRanges.Add(maxDistance);     // And start with the same max distance to travel
            
            Vector3 worldDir;

            if (scanAngle >= 360f)
            {
                worldDir = UnityEngine.Random.onUnitSphere;
            }
            else
            {
                // Uniform cone distribution math - using Archimedes theorem
                // Any slice of a sphere with the same height, has the same surface area on the "crust" of the sphere
                // Therefore if you imagine those heights getting really small ~ 0, it produces a circular ring on the sphere's surface
                // If you uniformly pick  heights/rings, all of which have the same surface area, and uniformly pick points on the rings
                // You will uniformly distribute points an the surface of the sphere
                // And if you limit the height to be picked along a line from the sphere's centre, say 1 at surface of a unit sphere, to 0.866 
                // Where the "width" of the sphere at that point corresponds to a 60 degree cone
                // You can uniformly distribute rays within a cone without clumping at the centre/pole
                // And then you can convert to the space of the "direction" of the cone

                float halfAngleRad = (scanAngle / 2f) * Mathf.Deg2Rad;      // Split angle to half on either side of line and convert to radians
                float minZ = Mathf.Cos(halfAngleRad);                       // Get the height corresponding to the angle of the cone on the sphere

                float exponent = Mathf.Lerp(8.0f, 1.0f, scanUniformity);    // How clumped the rays should be

                float rng = UnityEngine.Random.value;                       // Random value 0 to 1
                float biasedT = Mathf.Pow(rng, 1.0f / exponent);            // Warp random value using exponent, root effect for smooth hill like distribution

                float z = Mathf.Lerp(minZ, 1.0f, biasedT);                  // Uniformity = 1 means exponent = 1 means 1 / exponent = 1 means even distribution, higher exponent/lower uniformity means more clumped

                float radiusAtHeight = Mathf.Sqrt(1f - z * z);              // Get the radius of the ring at that point
                float phi = UnityEngine.Random.Range(0f, 2f * Mathf.PI);    // Randomly pick an angle around the ring (polar coordinates)

                // Convert to cartesian
                Vector3 localDir = new Vector3(
                    radiusAtHeight * Mathf.Cos(phi),
                    radiusAtHeight * Mathf.Sin(phi),
                    z
                );

                // Rotate to direction specified (prevent LookRotation(0,0,0) errors)
                if (scanDirection != Vector3.forward)
                {
                    Quaternion lookRot = Quaternion.LookRotation(scanDirection);
                    worldDir = lookRot * localDir;
                }
                else
                {
                    worldDir = localDir;
                }
            }

            rayDirections.Add(worldDir);    // Add world direction for this ray
        }


        // Reflection loop

        activeHitCount = 0;

        for (int bounce = 0; bounce <= maxBounces; bounce++)
        {
            int rayCount = rayOrigins.Length;   // Initialise to current number of rays in "generation
            if (rayCount == 0) break;           // Stop if none remaining

            // Prepare job memory
            commands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob); 
            results = new NativeArray<RaycastHit>(rayCount, Allocator.TempJob);

            // Prepare commands for all current rays
            for (int i = 0; i < rayCount; i++)
            {
                // Set up the settings package
                QueryParameters queryParams = QueryParameters.Default;
                queryParams.layerMask = scanLayers;                                         // Tells raycasts what they're "allowed" to hit, scanLyers is set in Unity
                queryParams.hitBackfaces = false;                                           // Dont't hit the insides of objects

                commands[i] = new RaycastCommand(rayOrigins[i], rayDirections[i], queryParams, rayRanges[i]);    // Start at ray origin, go in direciton of ray, use these settings, limit distance to remaining from max distance
            }

            // Fire rays
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));  // Schedule the job, "ScheduleBatch" tells Unity to split this work across all CPU cores
            handle.Complete();                                                                          // Forces the main thread to wait until the job is finished 

            // Reset object+children layers after initial projections (so reflections can hit it)
            if (bounce == 0 && objectToIgnore != null)
            {
                RestoreLayers();
            }

            // Lists for next bounce
            NativeList<Vector3> nextOrigins = new NativeList<Vector3>(raysPerScan, Allocator.TempJob);   
            NativeList<Vector3> nextDirections = new NativeList<Vector3>(raysPerScan, Allocator.TempJob);
            NativeList<float> nextRanges = new NativeList<float>(raysPerScan, Allocator.TempJob);



            // Process Hits
            for (int i = 0; i < rayCount; i++)
            {
                // If the collider is not null, the ray hit something
                if (results[i].collider != null)
                {
                    RaycastHit hit = results[i];    // Get hit data


                    // NOTE: if you don't want reflected rays to be visualised change the if to if (bounce == 0)
                    // ALSO: update the total max hit thing to reduce buffer size

                    if (activeHitCount < instanceMatrices.Length)   // Safety check, currently has more than enough space so should have no problems
                    {
                        Quaternion rotation = Quaternion.LookRotation(-hit.normal); // Create a rotation that looks "up" away from the surface normal - makes the quad lie flat on the wall
                    
                        Vector3 position;
                        float scale;
                        if (isGridMode)
                        {
                            position = hit.point + (hit.normal * gridOffset);   // Calculate position of the quad - hitpoint + offset
                            scale = gridQuadSize;                               // Get scale factor for quad
                        }
                        else
                        {
                            position = hit.point + (hit.normal * dotOffset);    // Calculate position of the quad - hitpoint + offset
                            scale = dotScale;                                   // Get scale factor for quad
                        }

                        instanceMatrices[activeHitCount] = Matrix4x4.TRS(position, rotation, Vector3.one * scale);          // Create the matrix (position, rotation, scale) for this instance
                        activeHitCount++;                                                                                   // Increment the counter
                    }
                    
                    // TODO: add this when we have multiple ray bounces working
                    // INoiseSensitive sensitiveTarget = results[i].collider.GetComponent<INoiseSensitive>();
                    // if (sensitiveTarget != null) // If the thing hit (the monster) has an implementaion of INoiseSensitive (not null) then it wants this info so send
                    //{
                    //  sensitiveTarget.OnHeardScan(sourceObj.transform); // Send the original source object even after reflections 
                    //}

                    // Calculate reflections
                    if (bounce < maxBounces)
                    {
                        float distanceTravelled = hit.distance;
                        float remainingRange = rayRanges[i] - distanceTravelled; // Calculate remaining distance

                        // Only bounce if range left
                        if (remainingRange > 0f)
                        {
                            Vector3 incomingDir = rayDirections[i];
                            Vector3 reflectedDir = Vector3.Reflect(incomingDir, hit.normal);

                            // Add to next batch of rays
                            nextOrigins.Add(hit.point + (hit.normal * 0.01f)); // Add offset to spawn point to prevent self collision
                            nextDirections.Add(reflectedDir);
                            nextRanges.Add(remainingRange);
                        }
                    }
                }
            }

            // Cleanup current "generation"
            commands.Dispose();
            results.Dispose();
            rayOrigins.Dispose();
            rayDirections.Dispose();
            rayRanges.Dispose();

            // Swap to next generations
            rayOrigins = nextOrigins;
            rayDirections = nextDirections;
            rayRanges = nextRanges;
        }


        // Final cleanup
        rayOrigins.Dispose();
        rayDirections.Dispose();
        rayRanges.Dispose();
        

        // Always set the arguments so (incorrect) values from previous calls of perform scan aren't kept
        // Set the arguments for the indirect draw call
        args[0] = (uint)quadMesh.GetIndexCount(0);  // How many vertices per mesh
        args[1] = (uint)activeHitCount;             // How many meshes to draw total
        args[2] = (uint)quadMesh.GetIndexStart(0);  // Start of the mesh index
        args[3] = (uint)quadMesh.GetBaseVertex(0);  // Base vertex location
        // 5th arg is left to default 0 (it's an offset) and is added to SV_InstanceID in the shader

        argsBuffer.SetData(args); // Send arguments to the args buffer

        // Only upload to GPU if something was actually hit
        if (activeHitCount > 0)
        {
            // Zeroes are, respectively, source index (start reading at beginning of C# array) and destination index (start writing at the beginning of the GPU buffer)
            matrixBuffer.SetData(instanceMatrices, 0, 0, activeHitCount); // Send the matrices to the GPU buffer
        }

        Debug.Log("Scan fired! Hits: " + activeHitCount);
    }

    // Function that actually draws the graphics
    void RenderVisuals()
    {
        if (instanceMaterial == null || quadMesh == null) return;

        instanceMaterial.SetBuffer("_InstanceMatrices", matrixBuffer);   // Tell the material where to find the position data (the matrix buffer)

        // Issue the draw command - "DrawMeshInstancedIndirect" is the most efficient way to draw lots of objects
        // Reads the count from args buffer instead of CPU telling it a number
        // In order parameters mean/are (use this shape, 0 - use the first sub-mesh, paint it with this shader, (explained below), use the argsBuffer to find how many to draw)
        // "Bounds(transform.position, Vector3.one * 1000)" -  Is a safety net, normally Unity calculates the size of the object to decide if it's on screen, if it's behind you it culls it for performance
        // Due to Indirect, positions are calculated on the GPU, so Unity's CPU has no idea where dots/grid are (behind or in front)
        // Fix - create a giant, fake bounding box that is 1000 metres wide centered on the spawn point of the rays
        // Unity asks if this giant box is on screen, so the rest can easily be left to the GPU 
        Graphics.DrawMeshInstancedIndirect(quadMesh, 0, instanceMaterial, new Bounds(transform.position, Vector3.one * 1000), argsBuffer);
    }

    // Runs when the object is deleted or the game stops
    void OnDestroy()
    {
        // Must manually release buffers or they'll stay in VRAM forever - memory leak
        if (matrixBuffer != null) matrixBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
