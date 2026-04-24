using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Light))]
public class LightTrigger : MonoBehaviour
{
    private Light spotLight;

    private float delayTime = 0.5f;

    void Start()
    {
        // Get the Light component attached to this GameObject
        spotLight = GetComponentInParent<Light>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Here");
        // Check if the object that entered the trigger is the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hello");
            // Turn off the light
            Invoke("TurnOffLight", delayTime);
        }
    }

    void TurnOffLight()
    {
        if (spotLight != null)
        {
            Debug.Log("Turning light off");
            // This turns off the entire GameObject holding the light,
            // so the FlashLights script cannot force it to turn on again
            spotLight.gameObject.SetActive(false);
        }
    }
}