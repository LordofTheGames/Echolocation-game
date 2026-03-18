using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class BlinkRealLight : MonoBehaviour
{
    [Tooltip("How long the light stays on during a ping")]
    public float flashDuration = 0.5f; 
    
    private Light myLight;

    void Awake()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false; 
    }    
    
    public void TriggerFlash()
    {
        StopAllCoroutines(); 
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        myLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        myLight.enabled = false;
    }
}