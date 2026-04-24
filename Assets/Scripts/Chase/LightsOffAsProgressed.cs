using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LightTrigger : MonoBehaviour
{
    private GameObject spotLight;

    private float delayTime = 0.5f;

    void Start()
    {
        if (transform.parent != null)
        {
            spotLight = transform.parent.gameObject;
        }
        else
        {
            Debug.LogError("LightTrigger script is on an object with no parent!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the Player
        if (other.CompareTag("Player"))
        {
            // Turn off the light
            Invoke("TurnOffLight", delayTime);
        }
    }

    void TurnOffLight()
    {
        if (spotLight != null)
        {
            // This turns off the entire GameObject holding the light,
            // so the FlashLights script cannot force it to turn on again
            spotLight.SetActive(false);
        }
    }
}