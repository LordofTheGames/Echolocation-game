using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class BroadcastSwitchEvent : MonoBehaviour
{
    public static event Action<string> OnDeviceChanged; 

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void HandleControlsChanged()
    {
        OnDeviceChanged?.Invoke(playerInput.currentControlScheme);
    }
}
