using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchPrompt : MonoBehaviour
{
    public List<GameObject> controllerPrompts;
    public List<GameObject> keyboardPrompts;

    void Start()
    {
        UpdateButtonPrompts(GameObject.Find("Player").GetComponent<PlayerInput>().currentControlScheme);
        BroadcastSwitchEvent.OnDeviceChanged += UpdateButtonPrompts;
    }

    // void OnEnable()
    // {
    //     BroadcastSwitchEvent.OnDeviceChanged += UpdateButtonPrompts;
    // }

    // void OnDisable()
    // {
    //     BroadcastSwitchEvent.OnDeviceChanged -= UpdateButtonPrompts;
    // }

    private void UpdateButtonPrompts(string newDevice)
    {
        if (newDevice == "Gamepad")
        {
            foreach (GameObject prompt in keyboardPrompts)
                prompt.SetActive(false);
            foreach (GameObject prompt in controllerPrompts)
                prompt.SetActive(true);
        }
        else if (newDevice == "Keyboard&Mouse")
        {
            foreach (GameObject prompt in controllerPrompts)
                prompt.SetActive(false);
            foreach (GameObject prompt in keyboardPrompts)
                prompt.SetActive(true);
        }
    }
}
