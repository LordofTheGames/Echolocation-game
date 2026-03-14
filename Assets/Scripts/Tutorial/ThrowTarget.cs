using UnityEngine;
using System;


public class ThrowTarget : MonoBehaviour
{
    public event Action OnCollision;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Throwable")
        {
            OnCollision?.Invoke();
        }
    }
}
