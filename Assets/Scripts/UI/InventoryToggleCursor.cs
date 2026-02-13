using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleCursor : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private MouseLook playerLook;
    [SerializeField] private DetectObjectOutline outlineDetector;

    private bool isOpen;

    private void Start()
    {
        SetOpen(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        bool pressedB   = Keyboard.current.bKey.wasPressedThisFrame;
        bool pressedTab = Keyboard.current.tabKey.wasPressedThisFrame;
        bool pressedEsc = Keyboard.current.escapeKey.wasPressedThisFrame;

        if (!isOpen)
        {
            if (pressedB || pressedTab)
                SetOpen(true);
        }
        else
        {
            if (pressedB || pressedTab || pressedEsc)
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
