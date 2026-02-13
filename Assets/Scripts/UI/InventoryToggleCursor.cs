using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleCursor : MonoBehaviour
{
    [Header("Input Setup")]
    [SerializeField] private PlayerInput playerInput;   // Drag player object here
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string inventoryMap = "Inventory";

    [Header("References")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private MouseLook playerLook;
    [SerializeField] private DetectObjectOutline outlineDetector;

    private bool isOpen;

    private void Start()
    {
        SetOpen(false);
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed && !isOpen)
        {
            SetOpen(true);
        }
    }

    public void OnCloseInventory(InputAction.CallbackContext context)
    {
        if (context.performed && isOpen)
        {
            SetOpen(false);
        }
    }

    public void CloseInventory() => SetOpen(false);
    public void OpenInventory()  => SetOpen(true);

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (inventoryUI) inventoryUI.SetActive(open);

        if (playerLook) playerLook.enabled = !open;
        if (outlineDetector) outlineDetector.SetEnabled(!open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerInput != null)
        {
            if (open)   playerInput.SwitchCurrentActionMap(inventoryMap);
            else        playerInput.SwitchCurrentActionMap(gameplayMap);
        }

        if (open) ResetAllSlotsByChildName();
    }

    private void ResetAllSlotsByChildName()
    {
        if (inventoryUI == null) return;

        var all = inventoryUI.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t.name == "Original") t.gameObject.SetActive(true);
            if (t.name == "LightBG") t.gameObject.SetActive(false);
        }
    }
}
