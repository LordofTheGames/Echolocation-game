using UnityEngine;
using Unity.Collections;            // Native collections (NativeArray) for high-performance memory management
using Unity.Jobs;                   // The job system - allows us to run code on multiple CPU cores
using System.Collections.Generic;   // For use of dictionary
using Unity.Profiling;
using Unity.Mathematics;
using Unity.Burst;

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
    public bool visualiseAllBounces = true;

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


    [Header("Colour Palettes")]
    public Color[] monsterColors = new Color[3] {Color.red, new Color(0.8f, 0f, 0f), new Color(0.6f, 0f, 0)};
    public Color[] interactableColors = new Color[3] {Color.green, new Color(0f, 0.8f, 0f), new Color(0f, 0.6f, 0f)};
    public Color[] defaultColors = new Color[3] {Color.cyan, new Color(0f, 0.8f, 0.8f), new Color(0f, 0.6f, 0.6f)};

    [Header("Layers To Detect (for different colour dots/squares)")]
    public LayerMask monsterLayer;
    public LayerMask interactableLayer;
    public LayerMask outlinedObjectLayer;


    // Hidden GPU variables

    // ComputeBuffer is a special list that lives in the GPU
    private ComputeBuffer argsBuffer;       // Holds arguments for drawing (how many meshes to draw)
    private ComputeBuffer matrixBuffer;     // Holds the position/rotation/scale of every single mesh
    private ComputeBuffer colorBuffer;      // Holds the colour for each dot/square


    // NativeArray is a high-performance list used by the job system
    private NativeArray<RaycastCommand> commands;       // The "to do list" of raycasts
    private NativeArray<RaycastHit> results;            // The results
    private NativeArray<Matrix4x4> instanceMatrices;    // Holds position data before sending it to the GPU
    private NativeArray<Vector4> instanceColors;        // Holds color data before sending it to GPU
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

    // For profiling main loop - bounces and stuff
    static readonly ProfilerMarker scanMarker = new ProfilerMarker("Burst_HeavyScanLoop");


    // Used to store render bounds so bounds aren't rebuilt every frame
    private Bounds renderBounds;

    // A small data packet the job will send back to main thread
    public struct VisualHit
    {
        public int originalRayIndex;
        public Matrix4x4 matrix;
        public int colorVariant; //  0, 1, or 2
    };

    // Struct to hold ray data to prevent scrambling in paralllel section
    public struct RayData
    {
        public Vector3 origin;
        public Vector3 direction;
        public float range;
    }

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

        long totalMaxHits = (long) raysPerScan * (maxBounces + 1);      // Calculate max number of rays/hits, including initial pulse and subsequent reflections
        int safeBufferSize = (int)Mathf.Min(totalMaxHits, 1000000);     // Prevenet a single pulse event from taking up to much VRAM

        instanceMatrices = new NativeArray<Matrix4x4>(safeBufferSize, Allocator.Persistent);    // Intialise the array to hold "safeBufferSize" number of matrices (positions)
        instanceColors = new NativeArray<Vector4>(safeBufferSize, Allocator.Persistent);        // Intialise the array to hold "safeBufferSize" number of colors
        
        matrixBuffer = new ComputeBuffer(safeBufferSize, 64);                                                   // Create the GPU buffer - 64 is the "stride" (size of one 4x4 matrix in bytes = 16 floats * 4 bytes each)
        colorBuffer = new ComputeBuffer(safeBufferSize, 16);                                                    // Create GPU buffer for colours, 16 = 4 floats * 4 bytes
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);     // Create arguments buffer, needs to hold 5 uints, the type tells the GPU this buffer doesn't contain 3D model data, only instructions for how to draw

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
    public void SetupScan(GameObject ignoreMe, Vector3 direction, float angle, float uniformity = 1.0f, int numRays = 4000, float maxDist = 50f, float volume = 10f, bool isFootsteps = false)
    {
        // Check to prevent LookRotation(0,0,0) errors
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;

        scanDirection = direction.normalized;
        scanAngle = angle;
        scanUniformity = Mathf.Clamp01(uniformity);     // Make sure is in valid range
        raysPerScan = Mathf.Clamp(numRays, 0, 100000);  // Make sure is in (currently chosen) valid range
        maxDistance = maxDist;
        objectToIgnore = ignoreMe;
        // TODO: remove this when multiple ray bounces have been implemented
        // for now just make the monster hear the sound
        GameObject monster = GameObject.FindGameObjectWithTag("Monster");
        if (monster != null) {
            INoiseSensitive sensitiveTarget = monster.GetComponent<INoiseSensitive>();
            if (sensitiveTarget != null) sensitiveTarget.OnHeardScan(transform, volume, isFootsteps);
        }
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
        NativeList<RayData> currentRays = new NativeList<RayData>(raysPerScan, Allocator.TempJob);  // Allocator.TempJob keeps the buffer for 4 frames - however must return the key (call .Dispose()) when done to avoid memory leak warnings
        currentRays.Resize(raysPerScan, NativeArrayOptions.UninitializedMemory);                    // Pre-size array without initialising memory
    
        // Prepare first layer of raycasts
        var genJob  = new GenerateRaysJob
        {
            seed = (uint)(Time.frameCount * 1000 + GetInstanceID()),
            scanAngle = scanAngle,
            scanDirection = scanDirection,
            scanUniformity = scanUniformity,
            maxDistance = maxDistance,
            startOrigin = transform.position,   // Gets the position at which this instance of the EhcolocationSystem.prefab was instantiated in GlobalEchoSystem.cs
            rays = currentRays.AsArray()
        };
        JobHandle genHandle = genJob.Schedule(raysPerScan, 64);

        // Reflection loop

        activeHitCount = 0;

        using (scanMarker.Auto())
        {
            for (int bounce = 0; bounce <= maxBounces; bounce++)
            {
                int rayCount = currentRays.Length;   // Initialise to current number of rays in "generation"
                if (rayCount == 0) break;            // Stop if none remaining

                // Prepare job memory
                commands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob); 
                results = new NativeArray<RaycastHit>(rayCount, Allocator.TempJob);

                // Setup physics job
                var setupJob = new SetupRaycastJob
                {
                    rays = currentRays.AsArray(), // Pass the combined list
                    layerMask = scanLayers, 
                    commands = commands
                };
                JobHandle setupHandle = setupJob.Schedule(rayCount, 64, bounce == 0 ? genHandle : default); // Has dependancy - on the first bounce wait until intial ray generation job has completed 

                // Run physics
                JobHandle rayHandle = RaycastCommand.ScheduleBatch(commands, results, 1, setupHandle);  // Schedule the job, "ScheduleBatch" tells Unity to split this work across all CPU cores
                rayHandle.Complete();                                                                   // Main thread waits here till complete


                // Reset object+children layers after initial projections (so reflections can hit it)
                if (bounce == 0 && objectToIgnore != null)
                {
                    RestoreLayers();
                }

                // List for next bounce
                NativeList<RayData> nextRays = new NativeList<RayData>(rayCount, Allocator.TempJob);   

                // Collect list of indices of rays that actually hit something
                NativeList<int> hitIndices = new NativeList<int>(rayCount, Allocator.TempJob);
                NativeList<VisualHit> visualHits = new NativeList<VisualHit>(rayCount, Allocator.TempJob);

                var processJob = new ProcessHitsJob
                {
                    results = results,
                    currentRays = currentRays.AsArray(),
                    bounce = bounce,
                    maxBounces = maxBounces,
                    isGridMode = isGridMode,
                    offset = isGridMode ? gridOffset : dotOffset,
                    scale = isGridMode ? gridQuadSize : dotScale,

                    visualiseAllBounces = visualiseAllBounces,

                    nextRays = nextRays.AsParallelWriter(),
                    hitIndices = hitIndices.AsParallelWriter(),
                    visualHits = visualHits.AsParallelWriter(),
                };

                JobHandle processHandle = processJob.Schedule(rayCount, 64);
                processHandle.Complete();

                // --- Assign colours (main thread unpacking) ---
                int currentBounceHits = visualHits.Length;

                // Unpack visual data and assign colours
                for (int k  = 0; k < currentBounceHits; k++)
                {
                    VisualHit vHit = visualHits[k];

                    // Use global index so bounce 1 doesn't overwrite bounce 0
                    int globalIndex = activeHitCount + k;

                    if (globalIndex >= instanceMatrices.Length) break; // Safety if goes past safeBufferSize

                    instanceMatrices[globalIndex] = vHit.matrix;

                    RaycastHit hit = results[vHit.originalRayIndex];
                    int hitLayerMask = 1 << hit.collider.gameObject.layer;  // Get hit layer and convert to bitmask
                    int variantIndex = vHit.colorVariant;                   // Get random index for colour within monster/interactable/default colours

                    if ((monsterLayer.value & hitLayerMask) > 0) // Bit wise comparison
                    {
                        instanceColors[globalIndex] = monsterColors[variantIndex];
                    }
                    else if ((interactableLayer.value & hitLayerMask) > 0 || (outlinedObjectLayer.value & hitLayerMask) > 0) // Check if interactable or currently outlined (only happens to interactables)
                    {
                        instanceColors[globalIndex] = interactableColors[variantIndex];
                    }
                    else
                    {
                        instanceColors[globalIndex] = defaultColors[variantIndex];
                    }
                }

                activeHitCount += currentBounceHits;
                        
                // TODO: add this when we have multiple ray bounces working
                // for (int k = 0; k < hitIndices.Length; k++)
                // {
                //     int originalRayIndex = hitIndices[k];
                //     RaycastHit hit = results[originalRayIndex];

                //     INoiseSensitive sensitiveTarget = hit.collider.GetComponent<INoiseSensitive>();

                //     if (sensitiveTarget != null)
                //     {
                //         sensitiveTarget.OnHeardScan(sourceObj.transform);
                //     }
                // }

                // Cleanup current "generation"
                commands.Dispose();
                results.Dispose();
                currentRays.Dispose();
                hitIndices.Dispose();
                visualHits.Dispose();

                // Swap to next generation
                currentRays = nextRays;
            }
        }

        // Final cleanup
        currentRays.Dispose();

        

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
            matrixBuffer.SetData(instanceMatrices, 0, 0, activeHitCount);   // Send the matrices to the GPU buffer
            colorBuffer.SetData(instanceColors, 0, 0, activeHitCount);      // Send the colours to the GPU buffer
        }

        //Debug.Log("Scan fired! Hits: " + activeHitCount);

        // SetBuffers moved from RenderVisuals to end of perform scan so only called once
        // RenderVisuals is ran every frame and buffers don't change after initial setting
        instanceMaterial.SetBuffer("_InstanceMatrices", matrixBuffer);  // Tell the material where to find the position data (the matrix buffer)
        instanceMaterial.SetBuffer("_InstanceColors", colorBuffer);     // Tell the material where to find the colours 

        // Create render bounds
        renderBounds = new Bounds(transform.position, Vector3.one * 1000);
    }

    // --- Burst Jobs ---

    [BurstCompile]
    struct GenerateRaysJob : IJobParallelFor
    {
        public uint seed;
        public float scanAngle;
        public float scanUniformity;
        public float3 scanDirection;
        public float maxDistance;
        public float3 startOrigin;
        public NativeArray<RayData> rays;

        public void Execute(int i)
        {
            // Each thread gets a unique, deterministic seed
            var rng = new Unity.Mathematics.Random(math.hash(new uint2(seed, (uint)i)) | 1u);
            float3 worldDir;

            if (scanAngle >= 360f)
            {
                worldDir = rng.NextFloat3Direction();
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

                float halfAngleRad = math.radians(scanAngle * 0.5f);        // Split angle to half on either side of line and convert to radians
                float minZ = math.cos(halfAngleRad);                        // Get the height corresponding to the angle of the cone on the sphere

                float exponent = math.lerp(8.0f, 1.0f, scanUniformity);     // How clumped the rays should be

                float biasedT = math.pow(rng.NextFloat(), 1.0f / exponent); // Warp random value using exponent, root effect for smooth hill like distribution

                float z = math.lerp(minZ, 1.0f, biasedT);                   // Uniformity = 1 means exponent = 1 means 1 / exponent = 1 means even distribution, higher exponent/lower uniformity means more clumped

                float radiusAtHeight = math.sqrt(1f - z * z);              // Get the radius of the ring at that point
                float phi = rng.NextFloat() * math.PI2;                     // Randomly pick an angle around the ring (polar coordinates)

                // Convert to cartesian
                float3 localDir = new float3(
                    radiusAtHeight * math.cos(phi),
                    radiusAtHeight * math.sin(phi),
                    z
                );

                // Rotate to direction specified (prevent LookRotation(0,0,0) errors)
                if (!scanDirection.Equals(math.forward()))
                {
                    quaternion lookRot = quaternion.LookRotation(scanDirection, math.up());
                    worldDir = math.rotate(lookRot, localDir);
                }
                else
                {
                    worldDir = localDir;
                }
            }

            // Add combined package
            rays[i] = new RayData{ origin = startOrigin, direction = worldDir, range = maxDistance };
        }
    }

    [BurstCompile]
    struct SetupRaycastJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RayData> rays;
        public LayerMask layerMask;
        public NativeArray<RaycastCommand> commands;

        public void Execute(int i)
        {
            QueryParameters qp = QueryParameters.Default;
            qp.layerMask = layerMask;   // Tells raycasts what they're "allowed" to hit, scanLyers is set in Unity
            qp.hitBackfaces = false;    // Dont't hit the insides of objects
            commands[i] = new RaycastCommand(rays[i].origin, rays[i].direction, qp, rays[i].range);
        }

    };

    [BurstCompile]
    struct ProcessHitsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> results;
        [ReadOnly] public NativeArray<RayData> currentRays;

        public int bounce;
        public int maxBounces;
        public bool isGridMode;
        public float offset;
        public float scale;

        public bool visualiseAllBounces;

        public NativeList<RayData>.ParallelWriter nextRays;
        
        public NativeList<int>.ParallelWriter hitIndices;
        public NativeList<VisualHit>.ParallelWriter visualHits;

        public void Execute(int i)
        {
            if (results[i].colliderInstanceID == 0) return;     // Unity's job system returns a colliderInstanceID of 0 if ray didn't hit anything

            RaycastHit hit = results[i];

            hitIndices.AddNoResize(i);

            if ((bounce == 0 && !visualiseAllBounces) || visualiseAllBounces)
            {
                // High performance SIMD math
                float3 forward = -hit.normal;
                float3 up = new float3(0, 1, 0);

                // If ray hits flat floor or ceiling, the normal is parallel to our "up" vector
                // This casuses NaN error. To fix, temporarily use x-axis as "up"
                if (math.abs(forward.y) > 0.99f)
                {
                    up = new float3(1, 0, 0);
                }
                quaternion rot = quaternion.LookRotation(forward, up);
                float3 pos = (float3)hit.point + (float3)(hit.normal * offset);

                // Hash for random colour variants in range 0-2 
                uint hash = math.hash(new int2(i, bounce));
                int colorVariant = (int)(hash % 3); 

                // Save indices so main thread can do layer detection for applying colour correctly
                visualHits.AddNoResize(new VisualHit
                {
                    originalRayIndex = i, 
                    matrix = Matrix4x4.TRS(pos, rot, new Vector3(scale, scale, scale)),
                    colorVariant = colorVariant
                });
            }

            if (bounce < maxBounces)
            {
                float remainingRange = currentRays[i].range - hit.distance;
                if (remainingRange > 0.0f)
                {
                    float3 incoming = currentRays[i].direction;
                    float3 normal = hit.normal;
                    float3 reflected = incoming - 2 * math.dot(incoming, normal) * normal;

                    // Group in struct so they aren't scrambled
                    nextRays.AddNoResize(new RayData
                    {
                        origin = hit.point + (hit.normal * 0.01f),
                        direction = reflected,
                        range = remainingRange
                    });
                }
            }
        }
    }

    // Function that actually draws the graphics
    void RenderVisuals()
    {
        if (instanceMaterial == null || quadMesh == null) return;

        // Issue the draw command - "DrawMeshInstancedIndirect" is the most efficient way to draw lots of objects
        // Reads the count from args buffer instead of CPU telling it a number
        // In order parameters mean/are (use this shape, 0 - use the first sub-mesh, paint it with this shader, (explained below), use the argsBuffer to find how many to draw)
        // "Bounds(transform.position, Vector3.one * 1000)" (now set at the end of PerformScan -  Is a safety net, normally Unity calculates the size of the object to decide if it's on screen, if it's behind you it culls it for performance
        // Due to Indirect, positions are calculated on the GPU, so Unity's CPU has no idea where dots/grid are (behind or in front)
        // Fix - create a giant, fake bounding box that is 1000 metres wide centered on the spawn point of the rays
        // Unity asks if this giant box is on screen, so the rest can easily be left to the GPU 
        Graphics.DrawMeshInstancedIndirect(quadMesh, 0, instanceMaterial, renderBounds, argsBuffer);
    }

    // Runs when the object is deleted or the game stops
    void OnDestroy()
    {
        // Must manually release buffers or they'll stay in VRAM forever - memory leak
        if (matrixBuffer != null) matrixBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
        if (colorBuffer != null) colorBuffer.Release();

        if (instanceMatrices.IsCreated) instanceMatrices.Dispose();
        if (instanceColors.IsCreated) instanceColors.Dispose();
    }
}
