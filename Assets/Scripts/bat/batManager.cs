using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-100)]
public class BatFlockManager : MonoBehaviour
{
    public GameObject batPrefab;
    public Transform pointA;
    public Transform pointB;

    public BatMovement centerBat;

    public int count = 25;
    public float spawnRadius = 0.5f;

    public LayerMask obstacleMask;
    public float agentRadius = 0.35f;

    public float minSpeed = 5.5f;
    public float maxSpeed = 7.5f;

    //maximum distance which another bat is considered a neighbor
    public float neighborRadius = 5.0f;
    //distance which bats begin pushing away from each other
    public float separationRadius = 0.35f;
    public float separationWeight = 1.2f;
    // force applied to the bats for seperation
    public float maxSeparationForce = 2f;
    //weight of the alignment steering force
    //higher values make bats match the average direction of nearby bats more strongly
    public float alignmentWeight = 2.8f;
    public float cohesionWeight = 5.5f;

    // weight of the force that pulls bats toward the route target.
    public float routeWeight = 1.2f;
    // distance from the current route point required before switching to the other route point
    public float waypointReach = 1.2f;

    //How far ahead a bat checks for obstacles
    public float lookAhead = 6.0f;
    // Weight of the obstacle avoidance steering force.
    public float obstacleWeight = 8.0f;
    // When bat encounters a wall, to what extent is it allowed to deviate from the current direction left, right, up and down
    public float sideAngle = 90f;

    public float turnSpeed = 7.0f;
    public float acceleration = 8.0f;
    public float flightHeightOffset = 1.5f;
    // Amplitude of subtle vertical bobbing motion
    public float bobAmp = 0.04f;
    public float bobFreq = 1.4f;
    //Amplitude of random motion noise added to bat flight.
    public float noiseAmp = 0.06f;
    public float noiseFreq = 0.8f;
    // The angle at which the body leans when turning
    public float bankAngle = 25f;

    // Controls how much the bat looks toward its current movement direction
    // versus its intended steering direction.
    [Range(0.5f, 1f)] public float lookIntentBlend = 0.82f;

    //The extent to which the head lifts or presses up and down when bat rises or falls
    [Range(0f, 25f)] public float pitchFromClimb = 12f;
    //When bat turns, will it slightly "press its head down/sink a little"?
    [Range(0f, 15f)] public float turnDipAngle = 6f;

    public float modelForwardOffsetY = 90f;

    public bool depenetrateAfterMove = true;
    //A certain safe distance/buffer thickness retained during a collision
    public float skin = 0.05f;

    public Transform playerCam;
    public float obscureTriggerDistance = 10f;
    public float obscureDistance = 1.5f;

    [Range(0f, 1f)]
    [SerializeField] float minRelativeVolume = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] float minRelativePitch = 0.35f;
    [SerializeField] float holdTimeToScare = 1.0f;

    [SerializeField] bool showSpatialCellsDebug;

    Material spatialCellsDebugMaterial;
    Mesh spatialCellsDebugCubeMesh;

    InputAction scareBatsAction;
    bool isObscuring;
    float scaredUntil;
    public bool IsObscuring => isObscuring;

    private MicInput micInput;
    float micHoldTimer;

    float obscureTriggerDistanceSq;
    float separationRadiusSq;
    float neighborRadiusSq;

    sealed class BatFlockFrameData
    {
        public Vector3 separation;
        public Vector3 alignment;
        public Vector3 cohesion;
    }

    readonly Dictionary<BatMovement, BatFlockFrameData> flockFrameByBat = new();
    readonly HashSet<BatMovement> flockAgentSetScratch = new();
    readonly List<BatMovement> flockDictPruneScratch = new();
    readonly List<BatMovement> flockNeighborScratch = new(32);

    public Vector3 GetObscureTargetPosition()
    {
        if (!playerCam)
        {
            return centerBat != null ? centerBat.transform.position : transform.position;
        }
        return playerCam.position + playerCam.forward * obscureDistance;
    }

    [HideInInspector] public readonly List<BatMovement> agents = new();

    readonly Dictionary<Vector3Int, List<BatMovement>> spatialCells = new();
    readonly Stack<List<BatMovement>> spatialListPool = new();

    void Awake()
    {
        obscureTriggerDistanceSq = obscureTriggerDistance * obscureTriggerDistance;
        separationRadiusSq = separationRadius * separationRadius;
        neighborRadiusSq = neighborRadius * neighborRadius;

        var micGo = GameObject.Find("MicInput");
        if (micGo != null)
            micInput = micGo.GetComponent<MicInput>();
    }

    void Update()
    {
        RebuildSpatialHash();
        PrecomputeFlockingForAllBats();

        if (!centerBat || !playerCam) return;

        float distSq = (playerCam.position - centerBat.transform.position).sqrMagnitude;
        bool micProximity = distSq < obscureTriggerDistanceSq;

        if (micInput != null)
        {
            if (!micProximity)
            {
                micHoldTimer = 0f;
            }
            else if (micInput.relativePitch >= minRelativePitch && micInput.relativeVolume >= minRelativeVolume)
            {
                micHoldTimer += Time.deltaTime;
                if (micHoldTimer >= holdTimeToScare)
                {
                    micHoldTimer = 0f;
                    isObscuring = false;
                    scaredUntil = Time.time + 4f;
                }
            }
            else
            {
                micHoldTimer = 0f;
            }
        }

        if (!isObscuring && Time.time > scaredUntil && micProximity)
            isObscuring = true;
    }

    void Start()
    {
        scareBatsAction = InputSystem.actions.FindAction("ScareBats");
        if (scareBatsAction != null)
        {
            scareBatsAction.performed += OnScareBats;
            scareBatsAction.Enable();
        }

        if (!pointA || !pointB) return;

        agents.Clear();

        if (centerBat != null)
        {
            centerBat.transform.position = pointA.position;
            // random value which is given to each bat
            centerBat.Init(this, Random.value * 9999f);
            agents.Add(centerBat);
        }

        int needSpawn = Mathf.Max(0, count - agents.Count);
        if (needSpawn <= 0 || !batPrefab) return;

        // random gereration of bats within a sphere around the spawn point
        for (int i = 0; i < needSpawn; i++)
        {
            Vector3 pos = pointA.position + Random.insideUnitSphere * spawnRadius;
            float spawnHeight = (pointA.position.y + pointB.position.y) * 0.5f + flightHeightOffset;
            pos.y = spawnHeight + Random.Range(-0.2f, 0.2f);

            var go = Instantiate(batPrefab, pos, Quaternion.identity);
            var globalEcho = GameObject.Find("GlobalEchoSystem");
            if (globalEcho != null)
            {
                var system = globalEcho.GetComponent<GlobalEchoSystem>();
                var meshCol = go.GetComponent<MeshCollider>();
                if (system != null && meshCol != null)
                    system.RegisterCollider(meshCol);
            }

            var agent = go.GetComponent<BatMovement>();
            if (!agent) agent = go.AddComponent<BatMovement>();

            agent.Init(this, Random.value * 9999f);
            agents.Add(agent);
        }
    }

    void OnScareBats(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isObscuring = false;
            scaredUntil = Time.time + 4f;
        }
    }

    void OnDestroy()
    {
        if (scareBatsAction != null)
        {
            scareBatsAction.performed -= OnScareBats;
        }

        if (spatialCellsDebugMaterial != null)
        {
            Destroy(spatialCellsDebugMaterial);
            spatialCellsDebugMaterial = null;
        }
    }

    void LateUpdate()
    {
        if (!showSpatialCellsDebug || !Application.isPlaying)
            return;

        float cellSize = Mathf.Max(0.01f, neighborRadius);

        InitSpatialCellsDebugDrawResources();
        if (spatialCellsDebugMaterial != null && spatialCellsDebugCubeMesh != null)
        {
            foreach (var kv in spatialCells)
            {
                Vector3 center = WorldCenterFromCell(kv.Key, cellSize);
                Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * cellSize);
                Graphics.DrawMesh(
                    spatialCellsDebugCubeMesh,
                    matrix,
                    spatialCellsDebugMaterial,
                    0,
                    null,
                    0,
                    null,
                    ShadowCastingMode.Off,
                    false);
            }
        }

        var wire = new Color(1f, 0.2f, 0.2f, 1f);
        foreach (var kv in spatialCells)
            DrawSpatialCellWireCube(WorldCenterFromCell(kv.Key, cellSize), cellSize, wire);
    }

    static void DrawSpatialCellWireCube(Vector3 center, float cellSize, Color color)
    {
        float h = cellSize * 0.5f;
        var ax = new Vector3(h, 0f, 0f);
        var ay = new Vector3(0f, h, 0f);
        var az = new Vector3(0f, 0f, h);

        Vector3 v000 = center - ax - ay - az;
        Vector3 v100 = center + ax - ay - az;
        Vector3 v110 = center + ax + ay - az;
        Vector3 v010 = center - ax + ay - az;
        Vector3 v001 = center - ax - ay + az;
        Vector3 v101 = center + ax - ay + az;
        Vector3 v111 = center + ax + ay + az;
        Vector3 v011 = center - ax + ay + az;

        Debug.DrawLine(v000, v100, color);
        Debug.DrawLine(v100, v110, color);
        Debug.DrawLine(v110, v010, color);
        Debug.DrawLine(v010, v000, color);

        Debug.DrawLine(v001, v101, color);
        Debug.DrawLine(v101, v111, color);
        Debug.DrawLine(v111, v011, color);
        Debug.DrawLine(v011, v001, color);

        Debug.DrawLine(v000, v001, color);
        Debug.DrawLine(v100, v101, color);
        Debug.DrawLine(v110, v111, color);
        Debug.DrawLine(v010, v011, color);
    }

    void InitSpatialCellsDebugDrawResources()
    {
        if (spatialCellsDebugCubeMesh == null)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spatialCellsDebugCubeMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        if (spatialCellsDebugMaterial != null)
            return;

        Shader shader =
            Shader.Find("BatFlock/SpatialCellsDebug")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");

        if (shader == null)
            return;

        spatialCellsDebugMaterial = new Material(shader);
        var fill = new Color(1f, 0f, 0f, 0.22f);
        if (spatialCellsDebugMaterial.HasProperty("_Color"))
            spatialCellsDebugMaterial.SetColor("_Color", fill);
        if (spatialCellsDebugMaterial.HasProperty("_BaseColor"))
            spatialCellsDebugMaterial.SetColor("_BaseColor", fill);

        spatialCellsDebugMaterial.renderQueue = 3000;
        spatialCellsDebugMaterial.enableInstancing = true;
    }

    static Vector3 WorldCenterFromCell(Vector3Int cell, float cellSize)
    {
        return new Vector3(
            (cell.x + 0.5f) * cellSize,
            (cell.y + 0.5f) * cellSize,
            (cell.z + 0.5f) * cellSize);
    }

    void OnDrawGizmos()
    {
        if (!showSpatialCellsDebug || !Application.isPlaying)
            return;

        float cellSize = Mathf.Max(0.01f, neighborRadius);
        foreach (var kv in spatialCells)
        {
            Vector3 center = WorldCenterFromCell(kv.Key, cellSize);
            Gizmos.color = new Color(1f, 0f, 0f, 0.22f);
            Gizmos.DrawCube(center, Vector3.one * cellSize);
            Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.65f);
            Gizmos.DrawWireCube(center, Vector3.one * cellSize);
        }
    }

    public bool TryGetFlockingPreprocess(BatMovement bat, out Vector3 separation, out Vector3 alignment, out Vector3 cohesion)
    {
        separation = alignment = cohesion = Vector3.zero;
        if (bat == null || !flockFrameByBat.TryGetValue(bat, out var data))
            return false;
        separation = data.separation;
        alignment = data.alignment;
        cohesion = data.cohesion;
        return true;
    }

    void PrecomputeFlockingForAllBats()
    {
        var agentSet = flockAgentSetScratch;
        agentSet.Clear();
        for (int i = 0; i < agents.Count; i++)
        {
            var a = agents[i];
            if (a) agentSet.Add(a);
        }

        flockDictPruneScratch.Clear();
        foreach (var kv in flockFrameByBat)
        {
            if (kv.Key == null || !agentSet.Contains(kv.Key))
                flockDictPruneScratch.Add(kv.Key);
        }

        for (int i = 0; i < flockDictPruneScratch.Count; i++)
            flockFrameByBat.Remove(flockDictPruneScratch[i]);

        for (int i = 0; i < agents.Count; i++)
        {
            var bat = agents[i];
            if (!bat) continue;

            if (!flockFrameByBat.TryGetValue(bat, out var data))
            {
                data = new BatFlockFrameData();
                flockFrameByBat[bat] = data;
            }

            Vector3 pos = bat.transform.position;
            AccumulateNeighborsInto(bat, pos, flockNeighborScratch);

            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;

            var neighbors = flockNeighborScratch;
            int nCount = 0;
            Vector3 center = Vector3.zero;
            Vector3 avgVelocity = Vector3.zero;

            for (int j = 0; j < neighbors.Count; j++)
            {
                var other = neighbors[j];
                if (!other) continue;

                Vector3 diff = other.transform.position - pos;
                float dSq = diff.sqrMagnitude;

                nCount++;
                center += other.transform.position;
                avgVelocity += other.velocity;

                if (dSq <= separationRadiusSq && dSq > 1e-12f)
                {
                    float d = Mathf.Sqrt(dSq);
                    separation -= diff / (d * d);
                }
            }

            if (nCount > 0)
            {
                center /= nCount;
                avgVelocity /= nCount;

                Vector3 toCenter = center - pos;
                float distToCenter = toCenter.magnitude;
                if (distToCenter > 0.0001f)
                {
                    cohesion = toCenter.normalized;
                    if (distToCenter > 2f)
                        cohesion *= 1f + (distToCenter - 2f) * 0.15f;
                }

                alignment = avgVelocity.sqrMagnitude > 0.01f ? avgVelocity.normalized : Vector3.zero;
            }

            data.separation = separation;
            data.alignment = alignment;
            data.cohesion = cohesion;
        }
    }

    void AccumulateNeighborsInto(BatMovement self, Vector3 pos, List<BatMovement> buffer)
    {
        buffer.Clear();
        float cellSize = Mathf.Max(0.01f, neighborRadius);
        float r2 = neighborRadiusSq;
        Vector3Int origin = WorldToCell(pos, cellSize);

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    Vector3Int key = new Vector3Int(origin.x + dx, origin.y + dy, origin.z + dz);
                    if (!spatialCells.TryGetValue(key, out var bucket))
                        continue;
                    for (int k = 0; k < bucket.Count; k++)
                    {
                        var other = bucket[k];
                        if (!other || other == self)
                            continue;
                        Vector3 diff = other.transform.position - pos;
                        if (diff.sqrMagnitude <= r2)
                            buffer.Add(other);
                    }
                }
            }
        }
    }

    static Vector3Int WorldToCell(Vector3 pos, float cellSize)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize),
            Mathf.FloorToInt(pos.z / cellSize));
    }

    void RebuildSpatialHash()
    {
        foreach (var list in spatialCells.Values)
        {
            list.Clear();
            spatialListPool.Push(list);
        }
        spatialCells.Clear();

        float cellSize = Mathf.Max(0.01f, neighborRadius);
        for (int i = 0; i < agents.Count; i++)
        {
            var a = agents[i];
            if (!a)
                continue;
            Vector3Int c = WorldToCell(a.transform.position, cellSize);
            if (!spatialCells.TryGetValue(c, out var bucket))
            {
                bucket = spatialListPool.Count > 0 ? spatialListPool.Pop() : new List<BatMovement>(8);
                spatialCells[c] = bucket;
            }
            bucket.Add(a);
        }
    }
}