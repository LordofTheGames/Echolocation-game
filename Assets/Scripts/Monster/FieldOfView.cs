using System;
using System.Collections;
using UnityEngine;

public class FieldOfViewChecker : MonoBehaviour
{
    public float Radius = 15f;
    [Range(0,360)]
    public float Angle = 100f;
    public float AgentEyeHeight = 3.9f;
    public float TargetEyeHeight = 1.8f;
    public LayerMask ObstructionMask;
    public bool ShowDebugVisuals;

    // Returns true if Target can be seen, false otherwise
    public bool FieldOfViewCheck(GameObject target)
    {
        Vector3 eyePos = transform.position + Vector3.up * AgentEyeHeight;
        Vector3 targetEyePos = target.transform.position + Vector3.up * TargetEyeHeight;

        float distanceToTarget = Vector3.Distance(eyePos, targetEyePos);
        if (distanceToTarget < Radius)
        {
            Vector3 directionToTarget = targetEyePos - eyePos;
            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                bool viewObstructed = Physics.Raycast(eyePos, directionToTarget, distanceToTarget, ObstructionMask);
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
            Vector3 eyePos = transform.position + Vector3.up * AgentEyeHeight;
            Gizmos.color = Color.red;
            Vector3 viewAngle01 = DirectionFromAngle(transform.eulerAngles.y, -Angle / 2);
            Vector3 viewAngle02 = DirectionFromAngle(transform.eulerAngles.y, Angle / 2);

            Gizmos.DrawLine(eyePos, eyePos + viewAngle01 * Radius);
            Gizmos.DrawLine(eyePos, eyePos + viewAngle02 * Radius);
            Gizmos.DrawWireSphere(eyePos, Radius);
        }
    }
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}