using UnityEngine;

public class CirclingBatMovement : MonoBehaviour
{
    private CirclingBatManager mgr;
    private float seed;
    float angleDeg;
    float radius;

    [HideInInspector] public Vector3 velocity;

    public void Init(CirclingBatManager manager, float randomSeed)
    {
        mgr = manager;
        seed = randomSeed;

        radius = mgr.circleRadius * Random.Range(0.8f, 1.1f);
        angleDeg = Random.Range(0f, 360f);

        Vector3 center = mgr.centerPoint != null ? mgr.centerPoint.position : transform.position;
        Vector3 offset = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), 0f, Mathf.Sin(angleDeg * Mathf.Deg2Rad)) * radius;
        transform.position = center + offset;

        float speed = Random.Range(mgr.minSpeed, mgr.maxSpeed);
        velocity = transform.forward * speed;
    }

    void Update()
    {
        if (mgr == null || mgr.centerPoint == null) return;

        float dt = Time.deltaTime;
        float t = Time.time;
        Vector3 pos = transform.position;

        if (!mgr.IsScared)
        {
            float angularSpeed = mgr.baseAngularSpeed * Mathf.Lerp(0.8f, 1.2f, Mathf.PerlinNoise(seed, t * 0.2f));
            angleDeg += angularSpeed * dt;

            Vector3 center = mgr.centerPoint.position;
            Vector3 circleOffset = new Vector3(
                Mathf.Cos(angleDeg * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(angleDeg * Mathf.Deg2Rad)
            ) * radius;

            float bob = Mathf.Sin(t * 2.2f + seed) * 0.4f;
            Vector3 circleTargetPos = center + circleOffset + Vector3.up * bob;

            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;

            int nCount = 0;
            Vector3 neighborsCenter = Vector3.zero;
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
                    neighborsCenter += other.transform.position;
                    avgVelocity += other.velocity;

                    if (d <= mgr.separationRadius && d > 0.001f)
                        separation -= diff / (d * d);
                }
            }

            if (nCount > 0)
            {
                neighborsCenter /= nCount;
                avgVelocity /= nCount;

                Vector3 toCenter = neighborsCenter - pos;
                float distToCenter = toCenter.magnitude;
                if (distToCenter > 0.0001f)
                {
                    cohesion = toCenter.normalized;
                    if (distToCenter > 2f) cohesion *= 1f + (distToCenter - 2f) * 0.15f;
                }
                else cohesion = Vector3.zero;

                alignment = avgVelocity.sqrMagnitude > 0.01f ? avgVelocity.normalized : Vector3.zero;
            }

            Vector3 seek = (circleTargetPos - pos).normalized;

            Vector3 pullBack = Vector3.zero;
            Vector3 toCenterXZ = center - pos;
            toCenterXZ.y = 0f;
            float distXZ = toCenterXZ.magnitude;
            if (distXZ > mgr.maxRadiusFromCenter && distXZ > 0.001f)
            {
                pullBack = (toCenterXZ / distXZ) * mgr.pullBackWeight;
            }

            Vector3 followLeader = Vector3.zero;
            if (mgr.centerBat != null && mgr.centerBat != this)
            {
                Vector3 toLeader = mgr.centerBat.transform.position - pos;
                if (toLeader.sqrMagnitude > 0.0001f)
                    followLeader = toLeader.normalized;
            }

            Vector3 forward = (velocity.sqrMagnitude > 0.01f) ? velocity.normalized : seek;

            Vector3 avoid = ObstacleAvoidance(pos, forward);

            float leaderFollowWeight = (mgr.centerBat != null && mgr.centerBat != this) ? 2.0f : 1.2f;

            Vector3 steer =
                separation.normalized * mgr.separationWeight +
                alignment * mgr.alignmentWeight +
                cohesion * mgr.cohesionWeight +
                seek * 1.2f +
                followLeader * leaderFollowWeight +
                avoid * mgr.obstacleWeight +
                pullBack;


            Vector3 desiredDir = steer.normalized;

            Vector3 currentDir = forward;
            Vector3 newDir = Vector3.Slerp(currentDir, desiredDir, 5f * dt).normalized;

            float speedWobble = 0.15f;
            float baseSpeed = Mathf.Lerp(mgr.minSpeed, mgr.maxSpeed, Mathf.PerlinNoise(seed + 10f, t * 0.6f));
            float desiredSpeed = Mathf.Clamp(baseSpeed + Mathf.Sin(t * 0.6f + seed) * speedWobble, mgr.minSpeed, mgr.maxSpeed);
            float newSpeed = Mathf.MoveTowards(velocity.magnitude, desiredSpeed, 5f * dt);

            velocity = newDir * newSpeed;
        }
        else
        {
            if (mgr.endPoint == null)
            {
                mgr.NotifyAgentDespawn(this);
                Destroy(gameObject);
                return;
            }

            Vector3 escapeTarget = mgr.endPoint.position;
            Vector3 toEscape = escapeTarget - pos;
            float dist = toEscape.magnitude;

            if (dist < mgr.despawnDistance)
            {
                mgr.NotifyAgentDespawn(this);
                Destroy(gameObject);
                return;
            }

            Vector3 dir = dist > 0.001f ? toEscape / dist : transform.forward;
            float targetSpeed = Mathf.Max(mgr.maxSpeed * mgr.escapeSpeedMultiplier, mgr.minSpeed);
            float newSpeed = Mathf.MoveTowards(velocity.magnitude, targetSpeed, 8f * dt);
            velocity = dir * newSpeed;
        }

        pos += velocity * dt;
        transform.position = pos;

        if (velocity.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            look *= Quaternion.Euler(0f, 90f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * dt);
        }
    }

    Vector3 ObstacleAvoidance(Vector3 pos, Vector3 forward)
    {
        if (mgr.obstacleMask == 0) return Vector3.zero;

        forward = forward.normalized;
        float r = Mathf.Max(0.02f, mgr.agentRadius);
        float checkDist = mgr.lookAhead;

        if (!Physics.SphereCast(pos, r, forward, out RaycastHit hit, checkDist, mgr.obstacleMask, QueryTriggerInteraction.Ignore))
            return Vector3.zero;

        Vector3 normal = hit.normal;
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.Cross(normal, Vector3.right);

        tangent.Normalize();

        Vector3 dirNormal = Vector3.ProjectOnPlane(forward + normal, Vector3.up).normalized;
        Vector3 dirTangent = Vector3.ProjectOnPlane(forward + tangent, Vector3.up).normalized;

        float pick = Mathf.PerlinNoise(seed, Time.time * 0.5f);
        Vector3 best = pick < 0.5f ? dirNormal : dirTangent;
        return best.normalized;
    }
}

