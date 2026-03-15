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

    [SerializeField, Range(0f, 2f)] float heightAmp = 0.35f;

    //The frequency of ups and downs
    [SerializeField, Range(0.3f, 5f)] float heightFreq = 2.2f;

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

        // if the imported bat model does not face Unity's forward axis (Z+),
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

        // route switching
        if (!mgr.IsObscuring && Vector3.Distance(transform.position, target.position) < mgr.waypointReach)
        {
            goingToB = !goingToB;
            target = goingToB ? mgr.pointB : mgr.pointA;
        }

        Vector3 pos = transform.position;

        Vector3 separation = Vector3.zero;  //Push away from nearby bats
        Vector3 alignment = Vector3.zero;   // Match direction of nearby bats
        Vector3 cohesion = Vector3.zero;  // Move toward local group center

        int nCount = 0;
        Vector3 center = Vector3.zero;
        Vector3 avgVelocity = Vector3.zero;

        for (int i = 0; i < mgr.agents.Count; i++)
        {
            var other = mgr.agents[i];
            if (!other || other == this) continue;

            Vector3 diff = other.transform.position - pos;
            float d = diff.magnitude;

            if (d <= mgr.neighborRadius)
            {
                nCount++;
                center += other.transform.position;
                avgVelocity += other.velocity;

                // If another bat is very close, generate separation force.
                // The d*d term makes very close bats repel more strongly.
                if (d <= mgr.separationRadius)
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
                // Cohesion pulls the bat toward the average neighbor position.
                cohesion = toCenter.normalized;
                // slightly strengthen cohesion to pull it back if it gets too far from the group
                if (distToCenter > 2f) cohesion *= 1f + (distToCenter - 2f) * 0.15f;
            }
            else cohesion = Vector3.zero;
            alignment = avgVelocity.sqrMagnitude > 0.01f ? avgVelocity.normalized : Vector3.zero;
        }

        float t = Time.time;
        // Calculate the bat's desired vertical oscillation height using a sine wave.
        float heightTarget = heightBase + Mathf.Sin(t * heightFreq + heightPhase) * heightAmp;

        Vector3 seekTargetPos;
        // If the flock is currently obscuring the player and this bat IS the center bat,
        // then fly toward the obscure target position (usually in front of the player camera)
        if (mgr.IsObscuring && mgr.centerBat != null && mgr.centerBat == this)
            seekTargetPos = mgr.GetObscureTargetPosition();
        else if (mgr.centerBat != null && mgr.centerBat != this)
            seekTargetPos = mgr.centerBat.transform.position;
        else
            seekTargetPos = target.position;

        // If it is the leader bat that is currently performing the action of blocking the player,
        // then its height is the same as the point that blocks the target.
        // Otherwise, continue to rise and fall at the normal flight altitude.
        float yTarget = (mgr.IsObscuring && mgr.centerBat == this) ? seekTargetPos.y : heightTarget;
        Vector3 target3D = new Vector3(seekTargetPos.x, yTarget, seekTargetPos.z);
        Vector3 seek = (target3D - pos).normalized;

        Vector3 followLeader = Vector3.zero;
        if (mgr.centerBat != null && mgr.centerBat != this)
        {
            Vector3 toLeader = mgr.centerBat.transform.position - pos;
            if (toLeader.sqrMagnitude > 0.0001f)
                followLeader = toLeader.normalized;
        }
        // Use velocity direction as current forward direction if moving,
        // otherwise use seek direction.
        Vector3 forward = (velocity.sqrMagnitude > 0.01f) ? velocity.normalized : seek;
        Vector3 avoid = ObstacleAvoidance3D(pos, forward);

        // Small vertical bobbing component for a more natural flight feel.
        float bob = Mathf.Sin(t * mgr.bobFreq + seed) * mgr.bobAmp;

        // Generate Perlin noise values so each bat gets smooth random drifting motion.
        float nx = Mathf.PerlinNoise(seed, t * mgr.noiseFreq) * 2f - 1f;
        float nz = Mathf.PerlinNoise(seed + 33.3f, t * mgr.noiseFreq) * 2f - 1f;
        float ny = Mathf.PerlinNoise(seed + 77.7f, t * mgr.noiseFreq) * 2f - 1f;

        Vector3 noise = new Vector3(nx, ny * 0.3f, nz) * (mgr.noiseAmp * 0.35f);
        Vector3 flightFeel = noise + Vector3.up * (bob * 0.2f);

        // Non-leader bats follow the leader more strongly.
        float leaderFollowWeight = (mgr.centerBat != null && mgr.centerBat != this) ? 2.5f : 1.5f;

        Vector3 sepContrib = separation.sqrMagnitude > 0.0001f
            ? separation.normalized * Mathf.Min(separation.magnitude * mgr.separationWeight, mgr.maxSeparationForce)
            : Vector3.zero;

        bool isLeader = mgr.centerBat != null && mgr.centerBat == this;
        float alignW = isLeader ? 0.2f : mgr.alignmentWeight;
        float cohesW = isLeader ? 0.2f : mgr.cohesionWeight;
        float seekW = isLeader ? mgr.routeWeight * 3f : mgr.routeWeight;

        Vector3 steer =
            sepContrib +
            alignment * alignW +
            cohesion * cohesW +
            seek * seekW +
            followLeader * leaderFollowWeight +
            avoid * mgr.obstacleWeight +
            flightFeel;

        if (steer.sqrMagnitude < 0.0001f) steer = seek;

        Vector3 desiredDir = steer.normalized;

        desiredDir.y = Mathf.Clamp(desiredDir.y, -maxClimbY, maxClimbY);
        desiredDir.Normalize();

        // Smoothly rotate current direction toward desired direction.
        Vector3 currentDir = forward;
        Vector3 newDir = Vector3.Slerp(currentDir, desiredDir, mgr.turnSpeed * Time.deltaTime).normalized;

        newDir.y = Mathf.Clamp(newDir.y, -maxClimbY, maxClimbY);
        newDir.Normalize();

        // Add a tiny speed wobble so bats do not all fly at perfectly constant speed.
        // When obscuring the player, bats fly faster; back to normal after scare.
        float speedWobble = 0.15f;
        float speedMult = mgr.IsObscuring ? 2.0f : 1f;
        float effectiveMin = mgr.minSpeed * speedMult;
        float effectiveMax = mgr.maxSpeed * speedMult;
        float desiredSpeed = Mathf.Clamp(baseSpeed * speedMult + Mathf.Sin(t * 0.6f + seed) * speedWobble, effectiveMin, effectiveMax);
        float newSpeed = Mathf.MoveTowards(velocity.magnitude, desiredSpeed, mgr.acceleration * Time.deltaTime);

        velocity = newDir * newSpeed;

        Vector3 delta = velocity * Time.deltaTime;
        MoveWithSlide(delta);

        // if the bat still overlaps geometry, push it back out.
        if (mgr.depenetrateAfterMove)
            Depenetrate();

        if (newDir.sqrMagnitude > 0.01f)
        {
            // Blend between actual movement direction and intended steering direction
            float intentBlend = mgr.lookIntentBlend;
            Vector3 intentDir = Vector3.Slerp(newDir, desiredDir, 1f - intentBlend).normalized;
            Quaternion look = Quaternion.LookRotation(intentDir, Vector3.up);

            float turnSign = Vector3.Dot(Vector3.Cross(currentDir, newDir), Vector3.up);
            float bank = -turnSign * mgr.bankAngle;
            Quaternion bankRot = Quaternion.AngleAxis(bank, intentDir);

            // Axis used for pitch (up/down tilt)
            Vector3 pitchAxis = Vector3.Cross(Vector3.up, intentDir).normalized;
            if (pitchAxis.sqrMagnitude < 0.01f) pitchAxis = Vector3.right;

            float pitchStr = mgr.pitchFromClimb;
            float dipStr = mgr.turnDipAngle;
            float climbRate = velocity.y;
            float pitch = Mathf.Clamp(climbRate * pitchStr, -25f, 25f);

            // Add a slight downward dip during sharper turns.
            float turnAmount = Vector3.Angle(currentDir, newDir);
            float turnDip = Mathf.Clamp01(turnAmount / 30f) * -dipStr;
            Quaternion pitchRot = Quaternion.AngleAxis(-pitch + turnDip, pitchAxis);

            Quaternion targetRot = look * bankRot * pitchRot;
            if (Mathf.Abs(mgr.modelForwardOffsetY) > 0.01f)
                targetRot *= Quaternion.Euler(0f, mgr.modelForwardOffsetY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
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

        Collider[] overlap = Physics.OverlapCapsule(p1, p2, r, mgr.obstacleMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlap.Length; i++)
        {
            var col = overlap[i];
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

    void InitDepenetrateCapsule()
    {
        var go = new GameObject("BatDepenetrateHelper");
        go.hideFlags = HideFlags.HideAndDontSave;
        depenetrateCapsule = go.AddComponent<CapsuleCollider>();
        depenetrateCapsule.isTrigger = true;
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