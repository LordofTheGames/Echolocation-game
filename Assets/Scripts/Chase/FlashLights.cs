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

    public void flashLights()
    {
        StartCoroutine(flashLightsCoroutine());
    }
    private IEnumerator flashLightsCoroutine()
    {
        yield return new WaitForSeconds(3);
        while (true){
            for(int i = 0; i < transform.childCount; ++i)
                transform.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(OnToOffInterval);
            for(int i = 0; i < transform.childCount; ++i)
                transform.GetChild(i).gameObject.SetActive(false);
            yield return new WaitForSeconds(OffToOnInterval);
        }
    }
}
