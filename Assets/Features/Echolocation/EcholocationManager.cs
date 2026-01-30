using UnityEngine;
using Unity.Collections;        // Native collections (NativeArray) for high-performance memory management
using Unity.Jobs;               // The job system - allows us to run code on multiple CPU cores
using Unity.Mathematics;        // Advanced math library - for matrix math

public class EcholocationManager : MonoBehaviour
{

    // Properties/variables (can edit in inspector windows in Unity)

    public enum VisualMode { Dots, MeshGrid };

    [Header("Grid Visualisation")]
    public VisualMode currentMode = VisualMode.MeshGrid;

    [Header("Scanner Settings")]
    public KeyCode triggerKey = KeyCode.E;  // Temporary key to press to fire rays for basic echolocation implementation
    public int raysPerScan = 4000;          // Number of rays fired for a single scan
    public float maxDistance = 50f;         // Max distance rays can travel
    public LayerMask scanLayers;

    [Header("Visual Settings")]
    // Tooltip adds pop-up info when hovering mouse over the variable in the inspector
    [Tooltip("Controls the size of the dots in dot mode.")]
    public float dotScale = 0.2f;
    [Tooltip("Controls the size of the quad 'spotlight' in Grid mode.")]
    public float gridQuadSize = 5.0f;
    [Tooltip("How far to pull the quad/'window' from the hitpoint (in metres) - higher = better for concave areas.")]
    public float quadOffset = 0.5f;
    [Tooltip("How far from the quad/'window' the grid will project the grid (how far surfaces can be before the grid isn't painted on them).")]
    public float gridDepth = 1.0f;

    [Header("References")]
    public Mesh quadMesh;               // Holds the 3D shape data (vertices and triangles) - we're using a simple flat quad
    public Material scannerMaterial;    // Holds the Shader and Textures

    [Header("Textures")]
    public Texture dotTexture;
    public Texture gridTexture;
    public Texture softMask;

    


    // Hidden GPU variables

    // ComputeBufferis a special list that lives in the GPU
    private ComputeBuffer argsBuffer;       // Holds arguments for drawing (how many meshes to draw)
    private ComputeBuffer matrixBuffer;     // Holds the position/rotation/scale of every single mesh

    // NativeArray is a high-performance list used by the job system
    private NativeArray<RaycastCommand> commands;   // The "to do list" of raycasts
    private NativeArray<RaycastHit> results;        // The results

    private Matrix4x4[] instanceMatrices; // Holds position data before sending it to the GPU

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0}; // Array of 5 uints required by "DrawMeshInstancedIndirect" command

    private int activeHitCount = 0; // A counter to keep track of how many rays have actually hit a wall this frame




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instanceMatrices = new Matrix4x4[raysPerScan];                                                      // Intialise theh array to hold "raysPerScan" number of matrices (positions)
        matrixBuffer = new ComputeBuffer(raysPerScan, 64);                                                  // Create the GPU buffer - 64 is the "stride" (size of one 4x4 matrix in bytes = 16 floats * 4 bytes each)
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments); // Create arguments buffer, needs to hold 5 uints, the type tells the GPU this buffer doesn't contain 3D model data, only instructions for how to draw

        // Make sure we didn't forget to assing textures
        if (softMask != null && scannerMaterial != null) 
        {
            scannerMaterial.SetTexture("_AlphaMask", softMask);
        }

        UpdateMaterialSettings();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMaterialSettings(); // Call every from so that if you change the dropdown while playing it changes instantly

        // Check if player pressed 'E' this frame - fire rays to perform scan
        if (Input.GetKeyDown(triggerKey))
        {
            PerformScan();
        }

        // If we had valid hits from a previous scan, draw them
        if (activeHitCount > 0)
        {
            RenderVisuals(); // Tells the GPU to paint the scans
        }
    }

    // Custom helper function to manage shader properties
    void UpdateMaterialSettings()
    {
        // If the material is missing stop immediately to prevent a crash
        if (scannerMaterial == null) return;

        scannerMaterial.SetFloat("_Falloff", quadOffset + gridDepth); // set the depth limit form the quad/window - dependent on the quad offset from the wall and grid depth so offset doesn't stop it from scanning surfaces if too high

        // Switch visualisation mode logic - checks which is selected
        if (currentMode == VisualMode.Dots)
        {
            scannerMaterial.SetTexture("_MainTex", dotTexture); // Send the dot texture to the shader
            scannerMaterial.SetFloat("_UseMesh", 0);            // Send 0 to the "_UseMesh" toggle in the shader (0 = false/unchecked)
        }
        else // Must be grid mode
        {
            scannerMaterial.SetTexture("_MainTex", gridTexture); // Send the mesh grid texture to the shader
            scannerMaterial.SetFloat("_UseMesh", 1);            // Send 1 to the "_UseMesh" toggle in the shader (1 = true/checked)
        }
    }

    // Custom helper to calulate ray directions that are spherically and uniformally distributed, using a Fibonacci Sphere
    // Returns a Vector3 direction
    Vector3 GetFibonacciSphereDirection(int index, int totalPoints)
    {
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;            // Calculate and store the golden ration which is essential for the pattern
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

    // Fires the rays
    void PerformScan()
    {
        // Create temporary memory for the job
        commands = new NativeArray<RaycastCommand>(raysPerScan, Allocator.TempJob); // Allocator.TempJob keeps the buffer for 4 frames - however must return the key (call .Dispose()) when done to avoid memory leak warnings
        results = new NativeArray<RaycastHit>(raysPerScan, Allocator.TempJob);

        Vector3 origin = transform.position; // Get the current position of the player/scanner

        // Prepare raycast commands
        for (int i = 0; i < raysPerScan; i++)
        {
            // Set up the settings package
            QueryParameters queryParams = QueryParameters.Default;
            queryParams.layerMask = scanLayers;                                         // Tells raycasts what they're "allowed" to hit, scanLyers is set in Unity
            queryParams.hitBackfaces = false;                                           // Dont't hit the insides of objects

            Vector3 dir = GetFibonacciSphereDirection(i, raysPerScan);                  // Get the "perfect" direction for this specific ray number
            commands[i] = new RaycastCommand(origin, dir, queryParams, maxDistance);    // Start at origin, go in direciton of dir, use these settings, limit distance
        }

        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));  // Schedule the job, "ScheduleBatch" tells Unity to split this work across all CPU cores
        handle.Complete();                                                                          // Forces the main thread to wait until the job is finished 
        activeHitCount = 0;                                                                         // Reset hit counter

        float currentSize = (currentMode == VisualMode.Dots) ? dotScale : gridQuadSize; // Determine how big the visuals should be depending on mode

        // See what was hit
        for (int i = 0; i < raysPerScan; i++)
        {
            // If the collider is not null, the ray hit something
            if (results[i].collider != null)
            {
                RaycastHit hit = results[i];                                                                        // Get hit data
                Quaternion rotation = Quaternion.LookRotation(hit.normal);                                          // Create a rotation that looks "up" away from the surface normal - makes hte quad lie flat on the wall
                Vector3 position = hit.point + (hit.normal * quadOffset);                                           // Calculate position of the quad - hitpoint + offset
                instanceMatrices[activeHitCount] = Matrix4x4.TRS(position, rotation, Vector3.one * currentSize);    // Create the matrix (position, rotation, scale) for this instance
                activeHitCount++;                                                                                   // Increment the counter
            }

        }

        // Only upload to GPU if something was actually hit
        if (activeHitCount > 0)
        {
            //zeroes are, respectively, source index (start reading at beginning of C# array) and destination index (start writing at the beginning of the GPU buffer)
            matrixBuffer.SetData(instanceMatrices, 0, 0, activeHitCount); // Send the matrices to the GPU buffer
            
            // Set the arguments for the indirect draw call
            args[0] = (uint)quadMesh.GetIndexCount(0);  // How many vertices per mesh
            args[1] = (uint)activeHitCount;             // How many meshes to draw total
            args[2] = (uint)quadMesh.GetIndexStart(0);  // Start of the mesh index
            args[3] = (uint)quadMesh.GetBaseVertex(0);  // Base vertex location
            // 5th arg is left to default 0 (it's an offset) and is added to SV_InstanceID in the shader

            argsBuffer.SetData(args); // Send argumnts to the args buffer
        }

        // Clean up memory to prevent leaks
        commands.Dispose();
        results.Dispose();
    }

    // Function that actually draws the graphics
    void RenderVisuals()
    {
        scannerMaterial.SetBuffer("_InstanceMatrices", matrixBuffer);   // Tell the material where to find the position data (the matrix buffer)

        // Issue the draw command - "DrawMeshInstancedIndirect" is the most efficient way to draw millions of objects
        // Reads the count from args buffer instead of CPU telling it a number
        // In order paramters mean/are (use this shape, 0 - use the first sub-mesh, paint it with this shader, (explained below), use the argsBuffer to find how many to draw)
        // "Bounds(Vector3.one, Vector3.one * 1000)" -  Is a safety net, normally Unity calculates the size of the object to decide if it's on screen, if it's behind you it culls it for performance
        // Due to Indirect, positions are calculated on the GPU, so Unity's CPU has no idea where dots/grid are (behind or in front)
        // Fix - create a giant, fake bounding box that is 1000 metres wide, Unity asks if this giant box is on screen and the answer is almost certainly yes, so the rest can easily be left to the GPU 
        Graphics.DrawMeshInstancedIndirect(quadMesh, 0, scannerMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer);
    }

    // Runs when the object is deleted or the game stops
    void OnDestroy()
    {
        // Must manually release buffers or they'll stay in VRAM forever - memory leak
        if (matrixBuffer != null) matrixBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
