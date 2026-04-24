using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

public class AcousticsManager : MonoBehaviour
{
    public static AcousticsManager Instance;

    [Header("Acoustic Settings")]
    public int raysPerPulse = 500; 
    public int maxBounces = 4;
    public float speedOfSound = 343f;
    public float maxDistance = 100f; // Max distance rays travel
    public LayerMask acousticBounceLayers;

    [Header("Material Reflection Coefficients (BandEnergy9)")]
    // 1.0 = Perfect Reflection, 0.0 = Perfect Absorption. 
    // Order: 63Hz, 125Hz, 250Hz, 50Hz 1KHz, 2KHz, 4KHz, 8KHz, 16Hz
    // Default: Set to match Rock for a consistent cave feel
    public BandEnergy9 refDefault = new BandEnergy9 { b63=0.95f, b125=0.92f, b250=0.90f, b500=0.88f, b1k=0.85f, b2k=0.80f, b4k=0.75f, b8k=0.65f, b16k=0.50f };

    // Metal: Highly reflective, very "bright" and "pingy"
    public BandEnergy9 refMetal = new BandEnergy9 { b63=0.98f, b125=0.98f, b250=0.97f, b500=0.96f, b1k=0.95f, b2k=0.94f, b4k=0.92f, b8k=0.90f, b16k=0.85f }; // Category 3 & 7

    // Dirt: Extremely absorbent, sounds very "dead"
    public BandEnergy9 refDirt = new BandEnergy9 { b63=0.40f, b125=0.30f, b250=0.20f, b500=0.15f, b1k=0.10f, b2k=0.08f, b4k=0.05f, b8k=0.03f, b16k=0.01f };  // Category 4

    // Wood: "Warm" sound, reflects bass but absorbs treble
    public BandEnergy9 refWood = new BandEnergy9 { b63=0.85f, b125=0.80f, b250=0.75f, b500=0.70f, b1k=0.60f, b2k=0.50f, b4k=0.40f, b8k=0.30f, b16k=0.15f };  // Category 5

    // Rock: Hard cave walls, some high-frequency scattering
    public BandEnergy9 refRock = new BandEnergy9 { b63=0.95f, b125=0.92f, b250=0.90f, b500=0.88f, b1k=0.85f, b2k=0.80f, b4k=0.75f, b8k=0.65f, b16k=0.50f };  // Category 8

    // Water: Very reflective, but absorbs ultra-high "splash" frequencies
    public BandEnergy9 refWater = new BandEnergy9 { b63=0.98f, b125=0.98f, b250=0.97f, b500=0.96f, b1k=0.95f, b2k=0.90f, b4k=0.85f, b8k=0.80f, b16k=0.70f }; // Category 9
    
    // FRP (Fibreglass Reinforced Plastic): Lab panels. Absorbs bass, highly reflective mids, harsh plastic "slap".
    public BandEnergy9 refFRP = new BandEnergy9 { b63=0.85f, b125=0.88f, b250=0.92f, b500=0.95f, b1k=0.96f, b2k=0.95f, b4k=0.92f, b8k=0.85f, b16k=0.75f }; // Category 6 - used for lab

    private NativeQueue<AcousticHit> successfulHitsQueue;


    void Awake()
    {
        Instance = this;
    }

    public void TriggerAcousticPulse(Vector3 origin, AudioClip clip, NativeHashMap<int, int> colliderColorMap)
    {
        if (AcousticReceiver.Instance == null) return;

        successfulHitsQueue = new NativeQueue<AcousticHit>(Allocator.TempJob);

        NativeList<AcousticRayData> currentRays = new NativeList<AcousticRayData>(raysPerPulse, Allocator.TempJob);
        currentRays.ResizeUninitialized(raysPerPulse);

        // Generate initial spherical rays, using fibonacci now :) for uniformity
        var genJob = new GenerateAcousticRaysJob
        {
            startOrigin = origin,
            rays = currentRays.AsArray()
        };
        genJob.Schedule(raysPerPulse, 64).Complete();

        // Bounce loop
        var qp = QueryParameters.Default;
        qp.layerMask = acousticBounceLayers;
        qp.hitBackfaces = false;
        qp.hitTriggers = QueryTriggerInteraction.Collide;

        for (int bounce = 0; bounce <= maxBounces; bounce++)
        {
            int rayCount = currentRays.Length;
            if (rayCount == 0) break;

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob);
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayCount, Allocator.TempJob);

            // Setup raycasts
            var setupJob = new SetupAcousticRaycastJob
            {
                rays = currentRays.AsArray(),
                qp = qp,
                commands = commands,
                maxDistance = maxDistance
            };
            JobHandle setupHandle = setupJob.Schedule(rayCount, 64);

            // Run physics
            JobHandle rayHandle = RaycastCommand.ScheduleBatch(commands, results, 32, setupHandle);
            rayHandle.Complete();

            NativeList<AcousticRayData> nextRays = new NativeList<AcousticRayData>(rayCount, Allocator.TempJob);

            // Process hits and apply material Absorption
            var processJob = new ProcessAcousticHitsJob
            {
                results = results,
                currentRays = currentRays.AsArray(),
                colliderColorMap = colliderColorMap,
                receiverInstanceId = AcousticReceiver.Instance.gameObject.GetComponent<Collider>().GetInstanceID(),
                speedOfSound = speedOfSound,

                refDefault = refDefault,
                refMetal = refMetal,
                refDirt = refDirt,
                refWood = refWood,
                refFRP = refFRP,
                refRock = refRock,
                refWater = refWater,

                nextRays = nextRays.AsParallelWriter(),
                successfulHits = successfulHitsQueue.AsParallelWriter()
            };
            processJob.Schedule(rayCount, 64).Complete();

            commands.Dispose();
            results.Dispose();
            currentRays.Dispose();
            currentRays = nextRays;
        }

        currentRays.Dispose();
        ProcessAcousticHits();
    }

    // --- Burst Jobs ---

    [BurstCompile]
    struct GenerateAcousticRaysJob : IJobParallelFor
    {
        public float3 startOrigin;
        public NativeArray<AcousticRayData> rays;

        public void Execute(int i)
        {
            // --- Optimised Fibonacci sphere ---
            float phi = math.PI * (3f - math.sqrt(5f));
            float y = 1 - (i / (float)(rays.Length - 1)) * 2;
            float radius = math.sqrt(1 - y * y);
            float theta = phi * i;

            float3 dir = new float3(math.cos(theta) * radius, y, math.sin(theta) * radius);

            rays[i] = new AcousticRayData
            {
                origin = startOrigin,
                direction = math.normalize(dir),
                distanceTraveled = 0f,
                energy = new BandEnergy9 { b63=1f, b125=1f, b250=1f, b500=1f, b1k=1f, b2k=1f, b4k=1f, b8k=1f, b16k=1f } // Start with full energy in all bands
            };
        }
    }

    [BurstCompile]
    struct SetupAcousticRaycastJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AcousticRayData> rays;
        public QueryParameters qp;
        public NativeArray<RaycastCommand> commands;
        public float maxDistance;

        public void Execute(int i)
        {
            commands[i] = new RaycastCommand(rays[i].origin, rays[i].direction, qp, maxDistance);
        }

    }

    [BurstCompile]
    struct ProcessAcousticHitsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> results;
        [ReadOnly] public NativeArray<AcousticRayData> currentRays;
        [ReadOnly] public NativeHashMap<int, int> colliderColorMap; 

        public int receiverInstanceId;
        public float speedOfSound;

        // Reflection vectors for different materials
        public BandEnergy9 refDefault, refMetal, refDirt, refWood, refFRP, refRock, refWater;

        public NativeList<AcousticRayData>.ParallelWriter nextRays;
        public NativeQueue<AcousticHit>.ParallelWriter successfulHits;

        public void Execute(int i)
        {
            if (results[i].colliderInstanceID == 0) return;

            RaycastHit hit = results[i];
            AcousticRayData incomingRay = currentRays[i];
            float newDistance = incomingRay.distanceTraveled + hit.distance;

            // Did it hit the "ear"/listener collider
            if (hit.colliderInstanceID == receiverInstanceId)
            {
                successfulHits.Enqueue(new AcousticHit
                {
                    finalEnergy = incomingRay.energy,
                    timeDelay = newDistance / speedOfSound,
                    arrivalDirection = -incomingRay.direction
                });
                return;
            }

            // Hit a wall - get material
            int category = 0;
            if (colliderColorMap.IsCreated) colliderColorMap.TryGetValue(hit.colliderInstanceID, out category);

            BandEnergy9 reflectionCoeff = refDefault; // set to default reflection coefficient
            switch (category)
            {
                case 3: case 7: reflectionCoeff = refMetal; break; // Metal & Rail
                case 4: reflectionCoeff = refDirt; break;          // Dirt
                case 5: reflectionCoeff = refWood; break;          // Wood
                case 6: reflectionCoeff = refFRP; break;           // Lab (FRP Panels)
                case 8: reflectionCoeff = refRock; break;          // Rock
                case 9: reflectionCoeff = refWater; break;         // Water
            }

            // Multpily the 9 bands using custom struct helper
            BandEnergy9 newEnergy = BandEnergy9.Multiply(incomingRay.energy, reflectionCoeff);

            // Air absorption (high frequencies decay over distance)
            // Can change values
            newEnergy.b4k = math.max(0f, newEnergy.b4k - (0.005f * hit.distance));
            newEnergy.b8k = math.max(0f, newEnergy.b8k - (0.010f * hit.distance));
            newEnergy.b16k = math.max(0f, newEnergy.b16k - (0.020f * hit.distance));

            // Early exit if silent (check if highest energy band is below 1%)
            float maxEnergy = math.max(newEnergy.b63, math.max(newEnergy.b125, math.max(newEnergy.b250, math.max(newEnergy.b500, 
                              math.max(newEnergy.b1k, math.max(newEnergy.b2k, math.max(newEnergy.b4k, math.max(newEnergy.b8k, newEnergy.b16k))))))));

            if (maxEnergy < 0.01f) return;

            float3 incomingDir = incomingRay.direction;
            float3 normal = hit.normal;
            float3 reflectedDir = incomingDir - 2 * math.dot(incomingDir, normal) * normal;

            nextRays.AddNoResize(new AcousticRayData
            {
                origin = (float3)hit.point + (normal * 0.01f),
                direction = reflectedDir,
                distanceTraveled = newDistance,
                energy = newEnergy
            });
        }        
    }

    // --- Main Thread Audio Synthesis ---

    private void ProcessAcousticHits()
    {
        if (successfulHitsQueue.IsCreated)
        {
            if (!successfulHitsQueue.IsEmpty())
            {
                NativeArray<AcousticHit> hits = successfulHitsQueue.ToArray(Allocator.Temp);

                foreach (var hit in hits)
                {
                    // Pass the data directly to the player's csound script
                    AcousticReceiver.Instance.ReceiveAcousticHit(hit);
                }

                hits.Dispose();
            }

            successfulHitsQueue.Dispose();
        }
    }

}