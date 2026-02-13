using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;

    private Dictionary<ItemType, int> counts = new Dictionary<ItemType, int>();

    // currently selected item to throw/use
    public ItemType Selected { get; private set; } = ItemType.Rock;

    private void Awake()
    {
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;

    foreach (ItemType t in (ItemType[])Enum.GetValues(typeof(ItemType)))
    {
        counts[t] = 0;
    }
    }

    public void Add(ItemType type, int amount = 1)
    {
        counts[type] += amount;
        OnInventoryChanged?.Invoke();
    }

    public void Remove(ItemType type, int amount = 1)
    {
        counts[type] -= amount;
        if (counts[type] < 0) counts[type] = 0;

        OnInventoryChanged?.Invoke();
    }

    public bool TryConsume(ItemType type, int amount = 1)
    {
        if (GetCount(type) < amount) return false;

        counts[type] -= amount;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetCount(ItemType type)
    {
        return counts[type];
    }

    public void Select(ItemType type)
    {
        if (GetCount(type) <= 0) return;
        Selected = type;
    }
}

