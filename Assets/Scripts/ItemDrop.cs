using UnityEngine;
using TMPro;

public class ItemDrop : MonoBehaviour
{
    // The ItemData blueprint 
    public ItemData droppedItem;
    public int stackCount = 1;
    public TextMeshProUGUI countLabel;

    void Start()
    {
        UpdateVisuals();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateVisuals();
    }
#endif

    public void SetStackCount(int amount)
    {
        stackCount = Mathf.Max(1, amount);
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        string visibleName = droppedItem != null && stackCount > 1
            ? $"{droppedItem.itemName} ({stackCount})"
            : droppedItem != null ? droppedItem.itemName : gameObject.name;

        gameObject.name = visibleName;

        if (countLabel != null)
        {
            countLabel.text = visibleName;
        }
    }
}