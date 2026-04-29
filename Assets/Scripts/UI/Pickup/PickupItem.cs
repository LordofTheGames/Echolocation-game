// using UnityEngine;

// public class PickupItem : MonoBehaviour
// {
//     [SerializeField] private ItemType itemType = ItemType.Rock;
//     [SerializeField] private int amount = 1;

//     public void Interact()
//     {
//         if (InventoryManager.Instance != null)
//         {
//             InventoryManager.Instance.Add(itemType, amount);
//         }

//         gameObject.SetActive(false);
//     }
// }

using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Rock;
    [SerializeField] private int amount = 1;
    [SerializeField] private string tutorialTaskName = "Pick-up";

    public ItemType PickupItemType => itemType;

    [SerializeField] private bool canPickup = true;

    public bool CanPickup => canPickup;

    public void SetCanPickup(bool value)
    {
        canPickup = value;
    }

    public void Interact()
    {
        if (!canPickup) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(itemType, amount);
        }

        if (!string.IsNullOrEmpty(tutorialTaskName) && itemType == ItemType.EchoBeacon)
        {
            Debug.Log($"Item task {tutorialTaskName} Completed!");
            TutorialManager.OnTaskComplete?.Invoke(tutorialTaskName);
        }

        gameObject.SetActive(false);
    }
}