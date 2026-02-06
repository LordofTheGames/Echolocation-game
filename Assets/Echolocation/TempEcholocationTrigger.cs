using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempEcholocationTrigger : MonoBehaviour
{
    [Header("Pulse Origins")]
    public Transform cameraTransform; // Assign main camera here

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
            GlobalEchoSystem.Ping(cameraTransform.position);
        }
    }
}
