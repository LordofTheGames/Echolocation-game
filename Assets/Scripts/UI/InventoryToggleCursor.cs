using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleCursor : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    [SerializeField] private MonoBehaviour playerLook;

    [SerializeField] private RayOutlineDetector outlineDetector; 
    private bool isOpen;
    InputAction openInventory, closeInventory;

    private void Start()
    {
        SetOpen(false);
        openInventory = InputSystem.actions.FindAction("Open Inventory");
        closeInventory = InputSystem.actions.FindAction("Close Inventory");
    }

    private void Update()
    {
        if (!isOpen && openInventory.WasPressedThisFrame())
        {
            SetOpen(true);
        }
        else if (isOpen && closeInventory.WasPressedThisFrame())
        {
            SetOpen(false);
        }
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (inventoryUI) inventoryUI.SetActive(open);
        if (playerLook) playerLook.enabled = !open;
        if (outlineDetector)
            outlineDetector.SetEnabled(!open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (outlineDetector) outlineDetector.SetEnabled(!open);
        if (open) ResetAllSlotsByChildName();
    }


    private void ResetAllSlotsByChildName()
    {
        if (inventoryUI == null) return;

        var all = inventoryUI.GetComponentsInChildren<Transform>(true);

        foreach (var t in all)
        {
            if (t.name == "Original")
                t.gameObject.SetActive(true);

            if (t.name == "LightBG")
                t.gameObject.SetActive(false);
        }
    }
}
