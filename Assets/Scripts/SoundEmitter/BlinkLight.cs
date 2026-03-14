using UnityEngine;

[RequireComponent(typeof(Light))]
public class BlinkRealLight : MonoBehaviour
{
    public float TimeOn = 0.5f;
    public float TimeOff = 1.0f;

    private float timePassed = 0f;
    private Light myLight;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        timePassed += Time.deltaTime;

        if (myLight.enabled && timePassed >= TimeOn)
        {
            myLight.enabled = false;
            timePassed = 0f;
        }
        else if (!myLight.enabled && timePassed >= TimeOff)
        {
            myLight.enabled = true;
            timePassed = 0f;
        }
    }
}