using UnityEngine;

[SelectionBase]
public class BreakableGlass : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] GameObject brokenBox;
    BoxCollider bc;

    private void Awake()
    {
        box.SetActive(true);
        brokenBox.SetActive(false);
        bc = GetComponent<BoxCollider>();
    }

    private void OnMouseDown()
    {
        Break();
    }

    private void Break()
    {
        box.SetActive(false);
        brokenBox.SetActive(true);
        bc.enabled = false;
    }
}
