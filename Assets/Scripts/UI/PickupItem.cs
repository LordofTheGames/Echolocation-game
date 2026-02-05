using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public void Interact() { 
        gameObject.SetActive(false);
    }
}