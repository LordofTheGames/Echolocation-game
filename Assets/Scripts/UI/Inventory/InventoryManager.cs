using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged, OnSelecting;
    private Dictionary<ItemType, int> counts = new Dictionary<ItemType, int>();

    // currently selected item to throw/use
    public ItemType Selected { get; private set; } = ItemType.None;
    //currently highlighted item in inventory
    public ItemType Selecting { get; private set; } =ItemType.None;
    [SerializeField] private InventoryToggleCursor toggle;

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

        Select(ItemType.None);

        //add items so tutorial got something to switch to
        Add(ItemType.Rock, 3);
        Add(ItemType.Plank, 1);

    }

    public void Add(ItemType type, int amount = 1)
    {
        counts[type] += amount;

        if(Selecting == ItemType.None)
        {
            Selecting = type;
            Selected = type;
            OnSelecting?.Invoke();
        }

        OnInventoryChanged?.Invoke();
    }

    //is this function used?
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

        if(counts[type] == 0) 
        {
            HandleNext();
            Selected = Selecting;
        }
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
        Debug.Log(Selected);
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.performed && Instance.toggle.UIopen())
        {
            Instance.HandleNext();
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.performed && Instance.toggle.UIopen())
        {
            Instance.HandlePrevious();
        }
    }

    private void HandleNext()
    {
        ItemType looper = Selecting;

        //loop through the inventory except the selecting
        for(int i=0; i<counts.Count-1; i++)
        {
            if(looper != ItemType.Key && looper != ItemType.None) looper++;
            else looper = ItemType.Rock;

            if(counts[looper] > 0)
            {
                Selecting = looper;
                break;
            }
        }

        if(counts[Selecting] == 0) Selecting = ItemType.None;

        Debug.Log(Selecting);
        OnSelecting?.Invoke();
    }

    private void HandlePrevious()
    {
        ItemType looper = Selecting;

        for(int i=0; i<counts.Count-1; i++)
        {
            if(looper != ItemType.Rock && looper != ItemType.None) looper--;
            else looper = ItemType.Key;

            if(counts[looper] > 0)
            {
                Selecting = looper;
                break;
            }
        }

        if(counts[Selecting] == 0) Selecting = ItemType.None;

        Debug.Log(Selecting);
        OnSelecting?.Invoke();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        Instance.Select(Instance.Selecting);
        Instance.toggle.CloseInventory();
    }
}

