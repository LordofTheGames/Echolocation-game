using UnityEngine;
using System;


public class Target : MonoBehaviour
{
    public event Action OnCollision;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            OnCollision?.Invoke();
        }
    }
}
