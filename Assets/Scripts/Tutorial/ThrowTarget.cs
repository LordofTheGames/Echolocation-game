using UnityEngine;
using System;


public class ThrowTarget : MonoBehaviour
{
    public event Action<string> OnCollision;

    private void OnTriggerEnter(Collider other)
    {
        string hitTag = other.gameObject.tag;

        if(hitTag == "Throwable Rock" || hitTag == "Throwable Emitter" || hitTag == "Throwable Grenade")
        {
            OnCollision?.Invoke(hitTag);
        }
    }
}
