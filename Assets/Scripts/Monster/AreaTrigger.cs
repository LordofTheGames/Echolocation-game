using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public GameObject Area;
    private Movement movementScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movementScript = GameObject.Find("Monster").GetComponent<Movement>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            movementScript.SetArea(Area.name);
        }
    }
}
