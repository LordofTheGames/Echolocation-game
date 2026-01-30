using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject lightBG; 
    public GameObject original;

    private void Start()
    {
        if (lightBG != null) lightBG.SetActive(false);
        if (original != null) original.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHighlight();
    }

    public void ShowHighlight()
    {
        if (lightBG != null) lightBG.SetActive(true);
        if (original != null) original.SetActive(false);
    }

    public void HideHighlight()
    {
        if (lightBG != null) lightBG.SetActive(false);
        if (original != null) original.SetActive(true);
    }
}