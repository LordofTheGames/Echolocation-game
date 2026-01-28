using UnityEngine;

public class WaterRipples : MonoBehaviour
{
    private Material waterMat;
    public float rippleStrngth = 0.5f;      
    public float smoothTime = 2.0f;                                 // speed water returns back to its still state

    private void Start()
    {
        waterMat = GetComponent<Renderer>().material;
        waterMat.SetFloat("_RippleScale", 0);                       // no ripples at start
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            waterMat.SetFloat("_RippleScale", rippleStrngth);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            waterMat.SetFloat("_RippleScale", 0);
        }
    }
}
