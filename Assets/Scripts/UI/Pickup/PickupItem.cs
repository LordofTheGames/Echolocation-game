using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Rock;
    [SerializeField] private int amount = 1;

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

        gameObject.SetActive(false);
    }
}
