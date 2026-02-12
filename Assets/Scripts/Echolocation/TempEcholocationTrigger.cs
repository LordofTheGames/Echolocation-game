using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempEcholocationTrigger : MonoBehaviour
{
    [Header("Pulse Origins")]
    public Transform cameraTransform; // Assign main camera here

    [Header("Echo Projection Uniformity (0 = clumped, 1 = uniform)")]
    public float uniformity = 1.0f;

    [Header("Number of Rays")]
    public int numRays = 4000;

    [Header("Angle of projection (0 = line, 60 = cone, 360 = sphere)")]
    public float angle = 60f;

    private InputAction echolocateAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        echolocateAction = InputSystem.actions.FindAction("Echolocate");

        if (echolocateAction == null)
        {
            Debug.Log("Error: Could not find input action 'Echolocate'.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (echolocateAction != null && echolocateAction.WasPressedThisFrame())
        {
            GlobalEchoSystem.Ping(this.gameObject, cameraTransform.position, cameraTransform.forward, angle, uniformity, numRays);
        }
    }
}
