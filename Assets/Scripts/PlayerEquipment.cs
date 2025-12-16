using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    public Transform handSocket;          // Where equipped items will appear
    public InventoryManager inventory;    // Used to read what the player has in hotbar

    GameObject equippedInstance;          // The currently equipped object in the hand
    int currentHotbarIndex = -1;          // Which hotbar slot we are using

    void Start()
    {
        // Find the inventory manager if not assigned
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();
    }

    void Update()
    {
        // Do not change equipment while inventory is open
        if (InventoryManager.IsInventoryOpen) return;
        if (Keyboard.current == null) return;

        int wantedIndex = -1;

        // Hotbar selection with number keys
        if (Keyboard.current.digit1Key.wasPressedThisFrame) wantedIndex = 0;
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) wantedIndex = 1;
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) wantedIndex = 2;
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) wantedIndex = 3;
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) wantedIndex = 4;
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) wantedIndex = 5;
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) wantedIndex = 6;
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) wantedIndex = 7;
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) wantedIndex = 8;
        else if (Keyboard.current.digit0Key.wasPressedThisFrame) wantedIndex = 9;

        // If the player pressed a number key, swap equipment
        if (wantedIndex >= 0)
            SelectHotbarSlot(wantedIndex);
    }

    public void SelectHotbarSlot(int hotbarIndex)
    {
        // Make sure the index is valid
        if (inventory == null) return;
        if (hotbarIndex < 0 || hotbarIndex >= inventory.hotbarSize) return;

        currentHotbarIndex = hotbarIndex;
        EquipFromCurrentSlot();
    }

    void EquipFromCurrentSlot()
    {
        // Check for valid state
        if (inventory == null) return;
        if (currentHotbarIndex < 0 || currentHotbarIndex >= inventory.hotbarSize) return;

        InventorySlot slot = inventory.inventorySlots[currentHotbarIndex];

        // Remove the old equipped item
        if (equippedInstance != null)
        {
            Destroy(equippedInstance);
            equippedInstance = null;
        }

        // If the slot is empty or the item cannot be equipped,stop
        if (slot == null || slot.IsEmpty()) return;
        if (slot.item == null || !slot.item.canEquip || slot.item.equippedPrefab == null) return;

        // Spawn the equipped prefab in the player's hand
        Transform parent = handSocket != null ? handSocket : transform;
        equippedInstance = Instantiate(slot.item.equippedPrefab, parent);
    }

    // Returns the ItemData that is currently equipped
    public ItemData GetEquippedItem()
    {
        if (inventory == null) return null;
        if (currentHotbarIndex < 0 || currentHotbarIndex >= inventory.hotbarSize) return null;

        InventorySlot slot = inventory.inventorySlots[currentHotbarIndex];
        return slot != null ? slot.item : null;
    }

    // Returns the current hotbar index
    public int GetCurrentHotbarIndex()
    {
        return currentHotbarIndex;
    }
}
