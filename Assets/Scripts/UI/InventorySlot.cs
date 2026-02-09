using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public GameObject lightBG;
    public GameObject original;

    [SerializeField] private ItemType itemType = ItemType.Rock;

    [SerializeField] private TMP_Text countText;   

    [SerializeField] private Color originalColor = Color.white;
    [SerializeField] private Color lightColor = Color.black;

    [SerializeField] private InventoryToggleCursor toggle;

    private int currentCount;

    private void OnEnable()
    {
        HideHighlight();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshCount;

        RefreshCount();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshCount;
    }

    public void RefreshCount()
    {
        if (InventoryManager.Instance == null) return;

        currentCount = InventoryManager.Instance.GetCount(itemType);

        if (countText != null)
            countText.text = currentCount.ToString();

        if (currentCount <= 0)
            HideHighlight();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentCount <= 0) return;
        ShowHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHighlight();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null) return;
        if (currentCount <= 0) return;

        InventoryManager.Instance.Select(itemType);

        if (toggle != null)
            toggle.CloseInventory();
    }

    public void ShowHighlight()
    {
        if (lightBG != null) lightBG.SetActive(true);
        if (original != null) original.SetActive(false);

        if (countText != null)
            countText.color = lightColor;   
    }

    public void HideHighlight()
    {
        if (lightBG != null) lightBG.SetActive(false);
        if (original != null) original.SetActive(true);

        if (countText != null)
            countText.color = originalColor;  
    }
}
