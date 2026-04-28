using TMPro;
using UnityEngine;

public class ItemBacklog : MonoBehaviour
{
    [SerializeField] private TMP_Text rock;
    [SerializeField] private TMP_Text soundEmitter;
    [SerializeField] private TMP_Text sonicGrenade;

    [SerializeField] private string normalHex = "#FFFFFF";
    [SerializeField] private string selectedHex = "#00FF40";

    private bool subscribed = false;

    private void Update()
    {
        if (!subscribed && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            InventoryManager.Instance.OnSelecting += Refresh;
            subscribed = true;

            Refresh();
        }
    }

    private void OnDisable()
    {
        if (subscribed && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
            InventoryManager.Instance.OnSelecting -= Refresh;
        }

        subscribed = false;
    }

    public void Refresh()
    {
        if (InventoryManager.Instance == null) return;

        UpdateLine(rock, ItemType.Rock, "Rock");
        UpdateLine(soundEmitter, ItemType.EchoBeacon, "Sound Emitter");
        UpdateLine(sonicGrenade, ItemType.SonicGrenade, "Sonic Grenade");
    }

    private void UpdateLine(TMP_Text targetText, ItemType itemType, string displayName)
    {
        if (targetText == null) return;

        int count = InventoryManager.Instance.GetCount(itemType);
        bool isSelected = InventoryManager.Instance.Selected == itemType;
        string nameColor = isSelected ? selectedHex : normalHex;

        targetText.text = $"<color={nameColor}>{displayName}</color>: {count}";
    }
}
