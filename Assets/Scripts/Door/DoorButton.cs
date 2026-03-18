using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] private LabDoor targetDoor;
    [SerializeField] private Breakable requiredBreakable;

    public bool CanInteract
    {
        get
        {
            if (requiredBreakable == null) return true;
            return requiredBreakable.HasBroken;
        }
    }

    public bool IsDoorOpen
    {
        get
        {
            if (targetDoor == null) return false;
            return targetDoor.IsOpen;
        }
    }

    public bool IsDoorMoving
    {
        get
        {
            if (targetDoor == null) return false;
            return targetDoor.IsMoving;
        }
    }

    public void Interact()
    {
        if (!CanInteract) return;
        if (targetDoor == null) return;

        targetDoor.Interact();
    }
}