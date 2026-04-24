using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLights : MonoBehaviour
{
    public float OffToOnInterval = 1.5f;
    public float OnToOffInterval = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActivateLights()
    {
        // Prep all lights
        for (int i = 0; i < transform.childCount; ++i)
        {
            // Force the GameObject to be active
            transform.GetChild(i).gameObject.SetActive(true);

            // Grab the Light component
            Light childLight = transform.GetChild(i).GetComponent<Light>();

            // Turn the light component off, so there is no light until turned on in flashing
            if (childLight != null)
            {
                childLight.enabled = false;
            }
        }
    }

    public void flashLights()
    {
        ActivateLights();
        StartCoroutine(flashLightsCoroutine());
    }
    private IEnumerator flashLightsCoroutine()
    {
        yield return new WaitForSeconds(3);
        while (true){
            for(int i = 0; i < transform.childCount; ++i)
            {
                Light childLight = transform.GetChild(i).GetComponent<Light>();

                if (childLight != null)
                {
                    childLight.enabled = true;
                }
            }
            yield return new WaitForSeconds(OnToOffInterval);
            for(int i = 0; i < transform.childCount; ++i)
            {
                Light childLight = transform.GetChild(i).GetComponent<Light>();

                if (childLight != null)
                {
                    childLight.enabled = false;
                }
            }
            yield return new WaitForSeconds(OffToOnInterval);
        }
    }
}
