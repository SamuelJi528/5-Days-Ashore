using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUISlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    public InventoryManager inventory;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventory == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        inventory.OnSlotClicked(slotIndex);
    }
}
