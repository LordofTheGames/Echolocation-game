using Unity.Mathematics;
using UnityEngine;


// Custom struct to hold 9 frequency bands, compatible with burst copmiler
[System.Serializable] // Let variables of this type be editable in inspector
public struct BandEnergy9
{
    public float b63, b125, b250, b500, b1k, b2k, b4k, b8k, b16k;

    // Helper to multiply energy when hitting a wall
    public static BandEnergy9 Multiply(BandEnergy9 current, BandEnergy9 material)
    {
        return new BandEnergy9
        {
            b63 = current.b63 * material.b63,
            b125 = current.b125 * material.b125,
            b250 = current.b250 * material.b250,
            b500 = current.b500 * material.b500,
            b1k = current.b1k * material.b1k,
            b2k = current.b2k * material.b2k,
            b4k = current.b4k * material.b4k,
            b8k = current.b8k * material.b8k,
            b16k = current.b16k * material.b16k
        };
    }
}

// Data passed back to the main thread when a ray reaches the player
public struct AcousticHit
{
    public BandEnergy9 finalEnergy;
    public float timeDelay;    // Distance / Speed of Sound
    public Vector3 arrivalDirection;
}

// Data used entirely inside the Job System
public struct AcousticRayData
{
    public float3 origin;
    public float3 direction;    
    public float distanceTraveled;
    public BandEnergy9 energy;
}