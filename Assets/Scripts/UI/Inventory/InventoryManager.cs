using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged, OnSelecting;
    private Dictionary<ItemType, int> counts = new Dictionary<ItemType, int>();
    private float time = 0;
    private bool tempInventory = false;

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
        Add(ItemType.Rock, 4);
        Add(ItemType.SonicGrenade, 4);

    }

    private void Update()
    {
        if(time > 0f && tempInventory){ 
            time -= Time.deltaTime;
        }

        if(time <= 0 && toggle.UIopen() && tempInventory)
        {
            toggle.CloseInventory();
            time = 0f;
            tempInventory = false;
        }
    }

    private void DelayClosing()
    {
        toggle.OpenInventory();
        tempInventory = true;
        time = 1f;
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
            HandleNext(ItemType.None);
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
        Selection(type);
    }

    public void Selection(ItemType type)
    {
        if (GetCount(type) <= 0) return;
        Selecting = type;
        OnSelecting?.Invoke();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Instance.HandleNext(Instance.Selecting);
            Instance.DelayClosing();
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Instance.HandlePrevious(Instance.Selecting);
            Instance.DelayClosing();
        }
    }

    private void HandleNext(ItemType start)
    {
        ItemType looper = start;

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

        Select(Selecting);
    }

    private void HandlePrevious(ItemType start)
    {
        ItemType looper = start;

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

        Select(Selecting);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if(context.performed && Instance.toggle.UIopen()){
            Instance.Select(Instance.Selecting);
            Instance.toggle.CloseInventory();
        }
    }

}

