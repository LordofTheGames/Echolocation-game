using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple first-person controller for Test scene. WASD move, mouse look.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class TestPlayerController : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f;

    private CharacterController _controller;
    private Transform _camera;
    private float _xRotation;
    private Vector3 _velocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>()?.transform;
        if (_camera == null)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(transform);
                cam.transform.localPosition = new Vector3(0, 1.6f, 0);
                _camera = cam.transform;
            }
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (_camera == null) return;

        // Move
        float h = 0, v = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) v += 1;
            if (Keyboard.current.sKey.isPressed) v -= 1;
            if (Keyboard.current.aKey.isPressed) h -= 1;
            if (Keyboard.current.dKey.isPressed) h += 1;
        }
        Vector3 move = transform.right * h + transform.forward * v;
        if (move.sqrMagnitude > 1) move.Normalize();
        _controller.Move(move * walkSpeed * Time.deltaTime);

        // Gravity
        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = 0;
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

        // Look
        if (Mouse.current != null)
        {
            var delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
            _xRotation -= delta.y;
            _xRotation = Mathf.Clamp(_xRotation, -89f, 89f);
            _camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            transform.Rotate(Vector3.up * delta.x);
        }
    }
}
