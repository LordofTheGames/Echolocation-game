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

// using UnityEngine;
// using UnityEngine.InputSystem;

// public class InventoryToggleCursor : MonoBehaviour
// {
//     [SerializeField] private GameObject inventoryUI;
//     [SerializeField] private MouseLook playerLook;
//     [SerializeField] private DetectObjectOutline outlineDetector;

//     [Header("Input Action Map names (from your InputActions asset)")]
//     [SerializeField] private string gameplayMapName = "Player"; 
//     [SerializeField] private string uiMapName = "UI";

//     private bool isOpen;
//     private InputAction openInventory, closeInventory;

//     private InputActionMap gameplayMap;
//     private InputActionMap uiMap;

//     private void Start()
//     {
//         openInventory = InputSystem.actions.FindAction("Open Inventory");
//         closeInventory = InputSystem.actions.FindAction("Close Inventory");

//         gameplayMap = InputSystem.actions.FindActionMap(gameplayMapName, throwIfNotFound: false);
//         uiMap = InputSystem.actions.FindActionMap(uiMapName, throwIfNotFound: false);

//         SetOpen(false);
//     }

//     private void Update()
//     {
//         if (!isOpen && openInventory != null && openInventory.WasPressedThisFrame())
//         {
//             SetOpen(true);
//             return;
//         }

//         if (isOpen && closeInventory != null && closeInventory.WasPressedThisFrame())
//         {
//             SetOpen(false);
//             return;
//         }
//     }

//     public void CloseInventory() => SetOpen(false);
//     public void OpenInventory() => SetOpen(true);

//     private void SetOpen(bool open)
//     {
//         isOpen = open;

//         if (inventoryUI) inventoryUI.SetActive(open);

//         if (playerLook) playerLook.enabled = !open;
//         if (outlineDetector) outlineDetector.SetEnabled(!open);

    
//         if (open)
//         {
//             if (gameplayMap != null) gameplayMap.Disable();
//             if (uiMap != null) uiMap.Enable();
//         }
//         else
//         {
//             if (uiMap != null) uiMap.Disable();        
//             if (gameplayMap != null) gameplayMap.Enable();
//         }

//         Cursor.visible = open;
//         Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

//         if (open) ResetAllSlotsByChildName();
//     }

//     private void ResetAllSlotsByChildName()
//     {
//         if (inventoryUI == null) return;

//         var all = inventoryUI.GetComponentsInChildren<Transform>(true);
//         foreach (var t in all)
//         {
//             if (t.name == "Original") t.gameObject.SetActive(true);
//             if (t.name == "LightBG") t.gameObject.SetActive(false);
//         }
//     }
// }

// using UnityEngine;
// using UnityEngine.InputSystem;

// public class InventoryToggleCursor : MonoBehaviour
// {
//     [SerializeField] private GameObject inventoryUI;

//     [SerializeField] private MouseLook playerLook;

//     [SerializeField] private DetectObjectOutline outlineDetector; 
//     private bool isOpen;
//     InputAction openInventory, closeInventory;

//     private void Start()
//     {
//         SetOpen(false);
//         openInventory = InputSystem.actions.FindAction("Open Inventory");
//         closeInventory = InputSystem.actions.FindAction("Close Inventory");
//     }

//     private void Update()
//     {
//         if (!isOpen && openInventory.WasPressedThisFrame())
//         {
//             SetOpen(true);
//         }
//         else if (isOpen && closeInventory.WasPressedThisFrame())
//         {
//             SetOpen(false);
//         }
//     }

//     private void SetOpen(bool open)
//     {
//         isOpen = open;

//         if (inventoryUI) inventoryUI.SetActive(open);
//         if (playerLook) playerLook.enabled = !open;
//         if (outlineDetector)
//             outlineDetector.SetEnabled(!open);

//         Cursor.visible = open;
//         Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

//         if (outlineDetector) outlineDetector.SetEnabled(!open);
//         if (open) ResetAllSlotsByChildName();
//     }


//     private void ResetAllSlotsByChildName()
//     {
//         if (inventoryUI == null) return;

//         var all = inventoryUI.GetComponentsInChildren<Transform>(true);

//         foreach (var t in all)
//         {
//             if (t.name == "Original")
//                 t.gameObject.SetActive(true);

//             if (t.name == "LightBG")
//                 t.gameObject.SetActive(false);
//         }
//     }
// }
