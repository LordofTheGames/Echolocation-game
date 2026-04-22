using UnityEngine;

public class BatMovement : MonoBehaviour
{
    private BatFlockManager mgr;
    // Random seed used to give each bat slightly different motion.
    private float seed;
    private Transform target;
    private bool goingToB = true;

    [HideInInspector] public Vector3 velocity;
    private float baseSpeed;
    private float heightBase;
    // Random phase offset so all bats do not bob up and down in sync.
    private float heightPhase;
    // Maximum vertical component allowed in the movement direction.
    private float maxClimbY = 0.65f;

    [SerializeField] float capsuleHalfHeight = 0.22f;
    [SerializeField] float originUpOffset = 0.25f;
    [SerializeField] float originForwardOffset = 0.15f;
    // When avoiding obstacles, randomly try out how many candidate directions
    [SerializeField] int avoidSamples = 18;

    const int DepenetrateOverlapCapacity = 16;

    [SerializeField, Range(0f, 2f)] float heightAmp = 0.35f;

    //The frequency of ups and downs
    [SerializeField, Range(0.3f, 5f)] float heightFreq = 2.2f;

    Vector3 flockSeparation;
    Vector3 flockAlignment;
    Vector3 flockCohesion;
    Vector3 steerDesiredDir;
    Vector3 steerNewDir;
    Vector3 steerCurrentDir;

    public void Init(BatFlockManager manager, float randomSeed)
    {
        mgr = manager;
        seed = randomSeed;

        goingToB = true;
        target = mgr.pointB;

        Vector3 dir = (mgr.pointB.position - mgr.pointA.position).normalized;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;

        baseSpeed = Random.Range(mgr.minSpeed, mgr.maxSpeed);
        velocity = dir * baseSpeed;

        Quaternion initRot = Quaternion.LookRotation(dir, Vector3.up);

        // apply an extra Y rotation offset.
        if (Mathf.Abs(mgr.modelForwardOffsetY) > 0.01f)
            initRot *= Quaternion.Euler(0f, mgr.modelForwardOffsetY, 0f);

        transform.rotation = initRot;

        heightBase = (mgr.pointA.position.y + mgr.pointB.position.y) * 0.5f + mgr.flightHeightOffset;
        heightPhase = Random.Range(0f, 1000f);
        heightAmp = heightAmp * Random.Range(0.7f, 1.3f);
        heightFreq = heightFreq * Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        if (!mgr || !mgr.pointA || !mgr.pointB) return;

        RefreshFlockingFromManager();
        UpdateSteeringAndVelocity();
        ApplyMovement();
        ApplyRotation();
    }

    void RefreshFlockingFromManager()
    {
        if (!mgr.TryGetFlockingPreprocess(this, out flockSeparation, out flockAlignment, out flockCohesion))
            flockSeparation = flockAlignment = flockCohesion = Vector3.zero;
    }

    void UpdateSteeringAndVelocity()
    {
        TryAdvanceRouteWaypoint();
        float t = Time.time;
        Vector3 pos = transform.position;

        float heightTarget = SampleHeightTarget(t);
        Vector3 seekDir = BuildSeekDirection(pos, heightTarget);
        Vector3 followLeader = BuildFollowLeaderDirection(pos);
        Vector3 facing = FacingForwardForSteering(seekDir);
        Vector3 avoid = ObstacleAvoidance3D(pos, facing);
        Vector3 flightFeel = BuildFlightFeel(t);
        Vector3 steer = CombineSteering(seekDir, followLeader, avoid, flightFeel);

        ApplyDirectionAndSpeed(t, seekDir, facing, steer);
    }

    void TryAdvanceRouteWaypoint()
    {
        float waypointReachSq = mgr.waypointReach * mgr.waypointReach;
        if (!mgr.IsObscuring && (transform.position - target.position).sqrMagnitude < waypointReachSq)
        {
            goingToB = !goingToB;
            target = goingToB ? mgr.pointB : mgr.pointA;
        }
    }

    float SampleHeightTarget(float t)
    {
        return heightBase + Mathf.Sin(t * heightFreq + heightPhase) * heightAmp;
    }

    Vector3 BuildSeekDirection(Vector3 pos, float heightTarget)
    {
        Vector3 seekTargetPos;
        if (mgr.IsObscuring && mgr.centerBat != null && mgr.centerBat == this)
            seekTargetPos = mgr.GetObscureTargetPosition();
        else if (mgr.centerBat != null && mgr.centerBat != this)
            seekTargetPos = mgr.centerBat.transform.position;
        else
            seekTargetPos = target.position;

        float yTarget = (mgr.IsObscuring && mgr.centerBat == this) ? seekTargetPos.y : heightTarget;
        Vector3 target3D = new Vector3(seekTargetPos.x, yTarget, seekTargetPos.z);
        return (target3D - pos).normalized;
    }

    Vector3 BuildFollowLeaderDirection(Vector3 pos)
    {
        if (mgr.centerBat == null || mgr.centerBat == this)
            return Vector3.zero;
        Vector3 toLeader = mgr.centerBat.transform.position - pos;
        return toLeader.sqrMagnitude > 0.0001f ? toLeader.normalized : Vector3.zero;
    }

    Vector3 FacingForwardForSteering(Vector3 seekFallback)
    {
        return (velocity.sqrMagnitude > 0.01f) ? velocity.normalized : seekFallback;
    }

    Vector3 BuildFlightFeel(float t)
    {
        float bob = Mathf.Sin(t * mgr.bobFreq + seed) * mgr.bobAmp;
        float nx = Mathf.PerlinNoise(seed, t * mgr.noiseFreq) * 2f - 1f;
        float nz = Mathf.PerlinNoise(seed + 33.3f, t * mgr.noiseFreq) * 2f - 1f;
        float ny = Mathf.PerlinNoise(seed + 77.7f, t * mgr.noiseFreq) * 2f - 1f;
        Vector3 noise = new Vector3(nx, ny * 0.3f, nz) * (mgr.noiseAmp * 0.35f);
        return noise + Vector3.up * (bob * 0.2f);
    }

    Vector3 CombineSteering(Vector3 seek, Vector3 followLeader, Vector3 avoid, Vector3 flightFeel)
    {
        float leaderFollowWeight = (mgr.centerBat != null && mgr.centerBat != this) ? 2.5f : 1.5f;

        Vector3 sepContrib = flockSeparation.sqrMagnitude > 0.0001f
            ? flockSeparation.normalized * Mathf.Min(flockSeparation.magnitude * mgr.separationWeight, mgr.maxSeparationForce)
            : Vector3.zero;

        bool isLeader = mgr.centerBat != null && mgr.centerBat == this;
        float alignW = isLeader ? 0.2f : mgr.alignmentWeight;
        float cohesW = isLeader ? 0.2f : mgr.cohesionWeight;
        float seekW = isLeader ? mgr.routeWeight * 3f : mgr.routeWeight;

        Vector3 steer =
            sepContrib +
            flockAlignment * alignW +
            flockCohesion * cohesW +
            seek * seekW +
            followLeader * leaderFollowWeight +
            avoid * mgr.obstacleWeight +
            flightFeel;

        if (steer.sqrMagnitude < 0.0001f)
            steer = seek;
        return steer;
    }

    void ApplyDirectionAndSpeed(float t, Vector3 seek, Vector3 forward, Vector3 steer)
    {
        steerDesiredDir = steer.normalized;
        steerDesiredDir.y = Mathf.Clamp(steerDesiredDir.y, -maxClimbY, maxClimbY);
        steerDesiredDir.Normalize();

        steerCurrentDir = forward;
        steerNewDir = Vector3.Slerp(steerCurrentDir, steerDesiredDir, mgr.turnSpeed * Time.deltaTime).normalized;
        steerNewDir.y = Mathf.Clamp(steerNewDir.y, -maxClimbY, maxClimbY);
        steerNewDir.Normalize();

        float speedWobble = 0.15f;
        float speedMult = mgr.IsObscuring ? 2.0f : 1f;
        float effectiveMin = mgr.minSpeed * speedMult;
        float effectiveMax = mgr.maxSpeed * speedMult;
        float desiredSpeed = Mathf.Clamp(baseSpeed * speedMult + Mathf.Sin(t * 0.6f + seed) * speedWobble, effectiveMin, effectiveMax);
        float newSpeed = Mathf.MoveTowards(velocity.magnitude, desiredSpeed, mgr.acceleration * Time.deltaTime);

        velocity = steerNewDir * newSpeed;
    }

    void ApplyMovement()
    {
        Vector3 delta = velocity * Time.deltaTime;
        MoveWithSlide(delta);

        if (mgr.depenetrateAfterMove)
            Depenetrate();
    }

    void ApplyRotation()
    {
        if (steerNewDir.sqrMagnitude <= 0.01f)
            return;

        float intentBlend = mgr.lookIntentBlend;
        Vector3 intentDir = Vector3.Slerp(steerNewDir, steerDesiredDir, 1f - intentBlend).normalized;
        Quaternion look = Quaternion.LookRotation(intentDir, Vector3.up);

        float turnSign = Vector3.Dot(Vector3.Cross(steerCurrentDir, steerNewDir), Vector3.up);
        float bank = -turnSign * mgr.bankAngle;
        Quaternion bankRot = Quaternion.AngleAxis(bank, intentDir);

        Vector3 pitchAxis = Vector3.Cross(Vector3.up, intentDir).normalized;
        if (pitchAxis.sqrMagnitude < 0.01f) pitchAxis = Vector3.right;

        float pitchStr = mgr.pitchFromClimb;
        float dipStr = mgr.turnDipAngle;
        float climbRate = velocity.y;
        float pitch = Mathf.Clamp(climbRate * pitchStr, -25f, 25f);

        float turnAmount = Vector3.Angle(steerCurrentDir, steerNewDir);
        float turnDip = Mathf.Clamp01(turnAmount / 30f) * -dipStr;
        Quaternion pitchRot = Quaternion.AngleAxis(-pitch + turnDip, pitchAxis);

        Quaternion targetRot = look * bankRot * pitchRot;
        if (Mathf.Abs(mgr.modelForwardOffsetY) > 0.01f)
            targetRot *= Quaternion.Euler(0f, mgr.modelForwardOffsetY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
    }
    // This function checks whether there is an obstacle in front of the bat.
    // If the path ahead is clear, it returns Vector3.zero (no avoidance needed).
    // If an obstacle is detected, it samples multiple candidate directions inside a cone
    // and chooses the best direction with the most free space and the least turning cost.
    Vector3 ObstacleAvoidance3D(Vector3 pos, Vector3 forward)
    {
        forward = forward.normalized;
        float r = Mathf.Max(0.02f, mgr.agentRadius);
        Vector3 origin = CastOrigin(pos, forward);

        if (!Physics.SphereCast(origin, r, forward, out _, mgr.lookAhead, mgr.obstacleMask, QueryTriggerInteraction.Ignore))
            return Vector3.zero;

        Vector3 bestDir = -forward;
        float bestScore = -999f;

        Evaluate(forward);

        float coneAngle = Mathf.Clamp(mgr.sideAngle, 10f, 160f);
        for (int i = 0; i < avoidSamples; i++)
        {
            Vector3 dir = RandomUnitInCone(forward, coneAngle);
            Evaluate(dir);
        }

        return bestDir;

        void Evaluate(Vector3 dir)
        {
            dir.Normalize();
            float freeDist = mgr.lookAhead;

            if (Physics.SphereCast(origin, r, dir, out RaycastHit hit, mgr.lookAhead, mgr.obstacleMask, QueryTriggerInteraction.Ignore))
                freeDist = hit.distance;

            float angle = Vector3.Angle(forward, dir);
            float score = freeDist - angle * 0.01f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }
    }

    // This function generates a random unit direction inside a cone around the given forward direction.
    // It is used by obstacle avoidance to try different possible directions
    // when the bat needs to find a way around an obstacle.
    Vector3 RandomUnitInCone(Vector3 forward, float coneAngleDeg)
    {
        Vector3 v = Random.onUnitSphere;

        Quaternion q = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
        v = (q * v).normalized;

        float maxAngle = coneAngleDeg;
        float a = Vector3.Angle(forward, v);
        if (a > maxAngle)
        {
            float t = maxAngle / a;
            v = Vector3.Slerp(forward, v, t).normalized;
        }

        return v;
    }

    // This function is a safety correction step used after movement.
    // If the bat is still overlapping with an obstacle after moving,
    // it pushes the bat out of the obstacle using Unity's penetration resolution.
    // This helps prevent bats from getting stuck inside walls or geometry.
    void Depenetrate()
    {
        if (depenetrateCapsule == null) InitDepenetrateCapsule();
        if (depenetrateOverlapBuffer == null)
            depenetrateOverlapBuffer = new Collider[DepenetrateOverlapCapacity];

        float r = Mathf.Max(0.02f, mgr.agentRadius);
        Vector3 pos = transform.position;
        Vector3 p1, p2;
        CapsuleEndpoints(pos, out p1, out p2);

        float halfH = Vector3.Distance(p1, p2) * 0.5f;
        Vector3 center = (p1 + p2) * 0.5f;
        depenetrateCapsule.radius = r;
        depenetrateCapsule.height = halfH * 2f + r * 2f;
        depenetrateCapsule.direction = 1;
        depenetrateCapsule.center = Vector3.zero;

        int overlapCount = Physics.OverlapCapsuleNonAlloc(
            p1, p2, r, depenetrateOverlapBuffer, mgr.obstacleMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlapCount; i++)
        {
            var col = depenetrateOverlapBuffer[i];
            if (Physics.ComputePenetration(
                depenetrateCapsule, center, Quaternion.identity,
                col, col.transform.position, col.transform.rotation,
                out Vector3 direction, out float distance))
            {
                pos += direction * (distance + mgr.skin);
                transform.position = pos;
                CapsuleEndpoints(pos, out p1, out p2);
                center = (p1 + p2) * 0.5f;
            }
        }
    }

    CapsuleCollider depenetrateCapsule;
    Collider[] depenetrateOverlapBuffer;

    void InitDepenetrateCapsule()
    {
        var go = new GameObject("BatDepenetrateHelper");
        go.hideFlags = HideFlags.HideAndDontSave;
        depenetrateCapsule = go.AddComponent<CapsuleCollider>();
        depenetrateCapsule.isTrigger = true;
        depenetrateOverlapBuffer = new Collider[DepenetrateOverlapCapacity];
    }
    // This function moves the bat while handling collisions in a smooth way.
    // Instead of letting the bat pass through walls or stop abruptly,
    // it uses capsule casting to detect obstacles and then slides along surfaces.
    // This makes movement around cave walls feel more natural.
    void MoveWithSlide(Vector3 delta)
    {
        if (delta.sqrMagnitude < 1e-10f) return;

        float r = Mathf.Max(0.02f, mgr.agentRadius);
        float skin = Mathf.Max(0.001f, mgr.skin);

        Vector3 pos = transform.position;
        Vector3 remaining = delta;

        for (int iter = 0; iter < 3; iter++)
        {
            float dist = remaining.magnitude;
            if (dist < 1e-6f) break;

            Vector3 dir = remaining / dist;

            Vector3 p1, p2;
            CapsuleEndpoints(pos, out p1, out p2);

            if (Physics.CapsuleCast(p1, p2, r, dir, out RaycastHit hit,
                dist + skin, mgr.obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float moveDist = Mathf.Max(0f, hit.distance - skin);
                pos += dir * moveDist;

                Vector3 leftover = remaining - dir * moveDist;
                Vector3 slide = Vector3.ProjectOnPlane(leftover, hit.normal);

                if (slide.sqrMagnitude < 1e-10f)
                {
                    remaining = Vector3.zero;
                    break;
                }

                remaining = slide;
            }
            else
            {
                pos += remaining;
                remaining = Vector3.zero;
                break;
            }
        }

        transform.position = pos;
    }

    void CapsuleEndpoints(Vector3 basePos, out Vector3 p1, out Vector3 p2)
    {
        Vector3 center = basePos + Vector3.up * originUpOffset + transform.forward * originForwardOffset;
        float half = Mathf.Max(0.05f, capsuleHalfHeight);
        p1 = center + Vector3.up * half;
        p2 = center - Vector3.up * half;
    }

    Vector3 CastOrigin(Vector3 pos, Vector3 forward)
    {
        return pos + Vector3.up * originUpOffset + forward * originForwardOffset;
    }
}