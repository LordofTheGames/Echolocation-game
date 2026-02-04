using System;
using System.Collections;
using UnityEngine;

public class AISenseManager : MonoBehaviour
{
    public GameObject Target;
    public bool targetSeen;
    public Vector3 lastLocation;
    public Quaternion lastDirection;

    FieldOfViewChecker[] FOVScripts;
    Vector3 secondToLastLocation;
    bool targetSeenLastFrame;

    void Start()
    {
        FOVScripts = GetComponents<FieldOfViewChecker>();
    }

    void Update()
    {
        for (int i = 0; i < FOVScripts.Length; i++)
        {
            if (FOVScripts[i].FieldOfViewCheck(Target))
            {
                targetSeen = true;
                targetSeenLastFrame = true;
                secondToLastLocation = lastLocation;
                lastLocation = Target.transform.position;
                return;
            }
        }
        if (targetSeenLastFrame)
        {
            // update last seen direction - only do this once, after the target has moved out of field of view
            // so that we are not calculating quaternions for every frame that the target is in fov
            Quaternion rotation = Quaternion.LookRotation(lastLocation - secondToLastLocation);
            // Limit rotation to y-axis
            rotation.x = 0;
            rotation.z = 0;
            lastDirection = rotation;

            targetSeenLastFrame = false;
        }
        targetSeen = false;
    }

}