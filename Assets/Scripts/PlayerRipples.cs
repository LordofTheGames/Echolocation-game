using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float RippleSize = 5f;
    public float RippleLifetime = 0.1f;
    
    [Header("References")]
    public ParticleSystem ripplePrefab;
 ParticleSystem ripple;
    public LayerMask WaterLayer;
    public LayerMask GroundLayer;

    private CharacterController cc;
    private Vector3 lastPlayerPos;
    private bool inWater;
    private RaycastHit isGround;

    void Start()
    {
        // Grab the CharacterController from the object this is attached to
        cc = GetComponent<CharacterController>();
        lastPlayerPos = transform.position;
        ripple = Instantiate(ripplePrefab);
    }

    void Update()
    {
        CalculateMovement();
        HandleRipplePositioning();
        CheckWaterStatus();
        
        // Update the global shader variable
        Shader.SetGlobalVector("_Player", transform.position);
    }

    void CalculateMovement()
    {
        // 1. Calculate Horizontal Distance moved
        Vector3 currentPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastPosFlat = new Vector3(lastPlayerPos.x, 0, lastPlayerPos.z);
        float distanceMoved = Vector3.Distance(currentPosFlat, lastPosFlat);

        // 2. Calculate actual Speed (Meters per Second)
        // This is much more reliable than just distance
        float horizontalSpeed = distanceMoved / Time.deltaTime;
        lastPlayerPos = transform.position;

        // 3. Grounded Check (Prevents ripples while jumping)
        bool isGrounded = cc.isGrounded; 

        // 4. THE LOGIC FIX:
        // Only ripple if in water AND grounded AND speed is significant (e.g., > 0.5m/s)
        if (inWater && isGrounded && horizontalSpeed > 0.5f)
        {
            ripple.Play();
            if (Time.renderedFrameCount % 5 == 0) // Slightly lower frequency for better look
            {
                ripple.Emit(transform.position, Vector3.zero, RippleSize, RippleLifetime, Color.white);
            }
        }
        else
        {
            ripple.Stop();
        }
    }

    void HandleRipplePositioning()
    {
        // Check ground for ripple offset
        Physics.Raycast(transform.position, Vector3.down, out isGround, 2.7f, GroundLayer);

        if (isGround.collider) 
            ripple.transform.position = transform.position + transform.forward;
        else 
            ripple.transform.position = transform.position;
    }

    void CheckWaterStatus()
    {
        float height = cc.height + cc.radius;
        inWater = Physics.Raycast(transform.position + Vector3.up * height, Vector3.down, height * 2, WaterLayer);

        // Toggle the ripple system visibility
        if (ripple.gameObject.activeSelf != inWater)
        {
            ripple.gameObject.SetActive(inWater);
        }
    }

    // --- Collision Logic ---

    private void OnTriggerEnter(Collider other)
    {
        // Check if the layer of 'other' is the water layer (Layer 4)
        if (other.gameObject.layer == 4)
        {
            ripple.Emit(transform.position, Vector3.zero, RippleSize, RippleLifetime, Color.white);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            ripple.Emit(transform.position, Vector3.zero, RippleSize, RippleLifetime, Color.white);
        }
    }
    public void CreateRipple(int StartAngle, int EndAngle, int Delta, float Speed, float Size, float Lifetime)
    {
        Vector3 angles = ripple.transform.eulerAngles;
        angles.y = StartAngle;
        ripple.transform.eulerAngles = angles;

        for (int i = StartAngle; i < EndAngle; i += Delta)
        {
            ripple.Emit(transform.position + ripple.transform.forward * 1.15f, ripple.transform.forward * Speed, Size, Lifetime, Color.white);
            ripple.transform.Rotate(Vector3.up * Delta, Space.World);
        }
    }
}