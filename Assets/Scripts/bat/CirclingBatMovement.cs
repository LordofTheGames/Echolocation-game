using UnityEngine;

public class CirclingBatMovement : MonoBehaviour
{
    private CirclingBatManager mgr;
    private float seed;
    float angleDeg;
    float radius;
    Vector3 homePosition;

    [SerializeField] float capsuleHalfHeight = 0.22f;
    [SerializeField] float originUpOffset = 0.25f;
    [SerializeField] float originForwardOffset = 0.15f;

    [HideInInspector] public Vector3 velocity;

    const int DepenetrateOverlapCapacity = 16;

    public void Init(CirclingBatManager manager, float randomSeed)
    {
        mgr = manager;
        seed = randomSeed;

        radius = mgr.circleRadius * Random.Range(0.8f, 1.1f);
        angleDeg = Random.Range(0f, 360f);

        Vector3 center = mgr.centerPoint != null ? mgr.centerPoint.position : transform.position;
        Vector3 offset = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), 0f, Mathf.Sin(angleDeg * Mathf.Deg2Rad)) * radius;
        float ySpread = Mathf.Max(0f, mgr.hoverHeightSpread);
        offset.y = Random.Range(-ySpread * 0.5f, ySpread * 0.5f);
        transform.position = center + offset;
        homePosition = transform.position;

        velocity = transform.forward * Random.Range(mgr.minSpeed, mgr.maxSpeed);
    }

    void Update()
    {
        // Safety check
        if (mgr == null) return;
        
        float dt = Time.deltaTime;
        Vector3 pos = transform.position;

        if (!mgr.IsScared)
            UpdateIdleMotion(dt, pos);
        else if (!TryUpdateEscapeMotion(dt, pos))
            return;

        ApplyMovement(dt);
        ApplyRotation(dt);
    }

    void UpdateIdleMotion(float dt, Vector3 pos)
    {
        float t = Time.time;
        ApplyIdleVerticalBob(dt, t);
        ApplyIdleHomeReturn(dt, pos);
        ClampIdleHorizontalSpeed();
        ApplyIdleHardClampPull(dt, pos);
    }

    void ApplyIdleVerticalBob(float dt, float t)
    {
        float u = Mathf.Repeat(seed * 0.318309886f + 0.271f, 1f);
        float u2 = Mathf.Repeat(seed * 0.479f + 0.631f, 1f);
        float phase1 = u * 6.283185f;
        float phase2 = u2 * 6.283185f;
        float bobHz1 = 1.05f + u * 0.55f;
        float bobHz2 = 1.75f + u2 * 0.65f;
        float w1 = Mathf.PI * 2f * bobHz1;
        float w2 = Mathf.PI * 2f * bobHz2;
        float ampMain = 0.38f + u * 0.28f;
        float ampFlutter = 0.22f;
        float targetVy = ampMain * w1 * Mathf.Cos(w1 * t + phase1) + ampFlutter * w2 * Mathf.Cos(w2 * t + phase2);

        float vyBlend = 5.5f;
        velocity.y = Mathf.Lerp(velocity.y, targetVy, Mathf.Clamp01(vyBlend * dt));
    }

    void ApplyIdleHomeReturn(float dt, Vector3 pos)
    {
        Vector3 flat = pos - homePosition;
        flat.y = 0f;
        float returnStrength = 2.2f;
        velocity.x -= flat.x * returnStrength * dt;
        velocity.z -= flat.z * returnStrength * dt;
        velocity.y += (homePosition.y - pos.y) * 1.8f * dt;

        velocity *= Mathf.Clamp01(1f - 1.1f * dt);
    }

    void ClampIdleHorizontalSpeed()
    {
        float idleMaxSpeed = Mathf.Max(1.1f, mgr.minSpeed * 0.85f);
        float vMag = velocity.magnitude;
        if (vMag > idleMaxSpeed)
            velocity = velocity / vMag * idleMaxSpeed;
    }

    void ApplyIdleHardClampPull(float dt, Vector3 pos)
    {
        Vector3 toCenter = mgr.centerPoint.position - pos;
        toCenter.y = 0f;
        float distToCenter = toCenter.magnitude;
        if (distToCenter > mgr.hardClampRadius && distToCenter > 0.001f)
            velocity += (toCenter / distToCenter) * (mgr.pullBackWeight * 0.45f) * dt;
    }

    bool TryUpdateEscapeMotion(float dt, Vector3 pos)
    {
        if (mgr.endPoint == null)
        {
            mgr.NotifyAgentDespawn(this);
            Destroy(gameObject);
            return false;
        }

        Vector3 escapeTarget = mgr.endPoint.position;
        Vector3 toEscape = escapeTarget - pos;
        float dist = toEscape.magnitude;

        if (dist < mgr.despawnDistance)
        {
            mgr.NotifyAgentDespawn(this);
            Destroy(gameObject);
            return false;
        }

        Vector3 dir = dist > 0.001f ? toEscape / dist : transform.forward;
        float targetSpeed = Mathf.Max(mgr.maxSpeed * mgr.escapeSpeedMultiplier, mgr.minSpeed);
        float newSpeed = Mathf.MoveTowards(velocity.magnitude, targetSpeed, mgr.acceleration * dt);
        velocity = dir * newSpeed;
        return true;
    }

    void ApplyMovement(float dt)
    {
        Vector3 delta = velocity * dt;
        MoveWithSlide(delta);
        if (mgr.depenetrateAfterMove) Depenetrate();
    }

    void ApplyRotation(float dt)
    {
        if (!mgr.IsScared)
            ClampWithinRadius();

        if (velocity.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            look *= Quaternion.Euler(0f, 90f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * dt);
        }
    }

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
            if (Physics.CapsuleCast(p1, p2, r, dir, out RaycastHit hit, dist + skin, mgr.obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float moveDist = Mathf.Max(0f, hit.distance - skin);
                pos += dir * moveDist;

                Vector3 leftover = remaining - dir * moveDist;
                Vector3 slide = Vector3.ProjectOnPlane(leftover, hit.normal);
                remaining = slide.sqrMagnitude < 1e-10f ? Vector3.zero : slide;
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

    CapsuleCollider depenetrateCapsule;
    Collider[] depenetrateOverlapBuffer;

    void InitDepenetrateCapsule()
    {
        var go = new GameObject("CirclingBatDepenetrateHelper");
        go.hideFlags = HideFlags.HideAndDontSave;
        depenetrateCapsule = go.AddComponent<CapsuleCollider>();
        depenetrateCapsule.isTrigger = true;
        depenetrateOverlapBuffer = new Collider[DepenetrateOverlapCapacity];
    }

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

    void ClampWithinRadius()
    {

        Vector3 pos = transform.position;
        Vector3 center = mgr.centerPoint.position;
        Vector3 offset = pos - center;
        Vector3 offsetXZ = new Vector3(offset.x, 0f, offset.z);
        float distXZ = offsetXZ.magnitude;
        if (distXZ <= mgr.hardClampRadius || distXZ < 0.0001f) return;

        Vector3 clampedXZ = offsetXZ / distXZ * mgr.hardClampRadius;
        pos.x = center.x + clampedXZ.x;
        pos.z = center.z + clampedXZ.z;
        transform.position = pos;

        Vector3 backToCenter = (center - pos).normalized;
        velocity = Vector3.Lerp(velocity, backToCenter * Mathf.Max(mgr.minSpeed, velocity.magnitude * 0.6f), 0.6f);
    }

    void CapsuleEndpoints(Vector3 basePos, out Vector3 p1, out Vector3 p2)
    {
        Vector3 center = basePos + Vector3.up * originUpOffset + transform.forward * originForwardOffset;
        float half = Mathf.Max(0.05f, capsuleHalfHeight);
        p1 = center + Vector3.up * half;
        p2 = center - Vector3.up * half;
    }
}