using UnityEngine;

public class InventoryToggleCursor : MonoBehaviour
{
    [Header("UI Root (Canvas/Panel)")]
    [SerializeField] private GameObject inventoryUI;

    [Header("Optional: disable look when inventory is open")]
    [SerializeField] private MonoBehaviour playerLook;

    private bool isOpen;

    private void Start()
    {
        SetOpen(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B))
        {
            SetOpen(!isOpen);
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
        }
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (inventoryUI) inventoryUI.SetActive(open);
        if (playerLook) playerLook.enabled = !open;

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
            if (t.name == "Original")
                t.gameObject.SetActive(true);

            if (t.name == "LightBG")
                t.gameObject.SetActive(false);
        }
    }
}
