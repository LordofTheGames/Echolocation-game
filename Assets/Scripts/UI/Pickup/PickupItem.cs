using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Rock;
    [SerializeField] private int amount = 1;

    public ItemType PickupItemType => itemType;

    public void Interact()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(itemType, amount);
        }

        gameObject.SetActive(false);
    }
}
