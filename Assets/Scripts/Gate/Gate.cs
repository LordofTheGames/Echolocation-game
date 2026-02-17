using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private GameObject gateRootToDisable; 
    
    [SerializeField] private int keyCost = 1;

    private bool unlocked;

    private void Awake()
    {
        if (gateRootToDisable == null)
            gateRootToDisable = gameObject;
    }

    public bool HasKey()
    {
        return InventoryManager.Instance != null
            && InventoryManager.Instance.GetCount(ItemType.Key) >= keyCost;
    }

    public bool TryUnlock()
    {
        if (unlocked) return true;
        if (InventoryManager.Instance == null) return false;

        if (!InventoryManager.Instance.TryConsume(ItemType.Key, keyCost))
            return false;

        unlocked = true;

        if (gateRootToDisable) gateRootToDisable.SetActive(false);

        return true;
    }
}
