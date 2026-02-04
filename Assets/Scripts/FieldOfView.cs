using System;
using System.Collections;
using UnityEngine;

public class FieldOfViewChecker : MonoBehaviour
{
    public float Radius;
    [Range(0,360)]
    public float Angle;
    public float DetectionHeight;
    public LayerMask ObstructionMask;
    public bool ShowDebugVisuals;

    // Returns true if Target can be seen, false otherwise
    public bool FieldOfViewCheck(GameObject target)
    {
        // TODO: change this to simple distance calculation
        // int targetMask = 1 << target.layer;
        // Collider[] collidersInRange = Physics.OverlapSphere(transform.position, Radius, targetMask);
        // if (collidersInRange.Length < 1) return false;
        // for (int i = 0; i < collidersInRange.Length; i++)
        // {
        //     if (collidersInRange[i].gameObject == target)
        //     {
        if (Vector3.Distance(target.transform.position, transform.position) < Radius)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                bool viewObstructed = Physics.Raycast(transform.position + Vector3.up * DetectionHeight, directionToTarget, distanceToTarget, ObstructionMask);
                if (!viewObstructed)
                    return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (ShowDebugVisuals)
        {
            Gizmos.color = Color.red;
            Vector3 viewAngle01 = DirectionFromAngle(transform.eulerAngles.y, -Angle / 2);
            Vector3 viewAngle02 = DirectionFromAngle(transform.eulerAngles.y, Angle / 2);

            Gizmos.DrawLine(transform.position + Vector3.up * DetectionHeight, transform.position + viewAngle01 * Radius + Vector3.up * DetectionHeight);
            Gizmos.DrawLine(transform.position + Vector3.up * DetectionHeight, transform.position + viewAngle02 * Radius + Vector3.up * DetectionHeight);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * DetectionHeight, Radius);
        }
    }
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}