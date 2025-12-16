using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public int inventoryCapacity = 40;
    public int hotbarSize = 10;

    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public GameObject slotPrefab;
    public RectTransform backpackParent;
    public RectTransform hotbarParent;
    public GameObject inventoryPanelRoot;

    public Transform dropPoint;
    public float dropForwardForce = 1.5f;
    public float dropUpForce = 0.5f;

    public PlayerStats playerStats;
    public ItemData healthPotionItem;
    public float healthPotionAmount = 40f;
    public ItemData healthRegenPotionItem;
    public float healthRegenTotalAmount = 50f;
    public float healthRegenDuration = 10f;

    public ItemData meatItem;
    public float meatHealAmount = 10f;
    public float meatHungerAmount = 25f;

    public int currentHotbarIndex;

    public static bool IsInventoryOpen = false;

    void Start()
    {
        // Create the internal list of inventory slots
        for (int i = 0; i < inventoryCapacity; i++)
            inventorySlots.Add(new InventorySlot());

        RefreshUI();

        // Start with inventory closed
        if (inventoryPanelRoot != null)
            inventoryPanelRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        IsInventoryOpen = false;

        // Try to find the player and their stats
        if (playerStats == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                playerStats = p.GetComponent<PlayerStats>();
        }

        currentHotbarIndex = 0;
    }

    void Update()
    {
        // Toggle inventory on/off with the I key
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();

        if (Keyboard.current == null) return;

        // Hotbar selection with number keys 1–0
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetHotbarIndex(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetHotbarIndex(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetHotbarIndex(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SetHotbarIndex(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SetHotbarIndex(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SetHotbarIndex(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SetHotbarIndex(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) SetHotbarIndex(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) SetHotbarIndex(8);
        if (Keyboard.current.digit0Key.wasPressedThisFrame) SetHotbarIndex(9);

        // R key: use item in current hotbar slot
        if (Keyboard.current.rKey.wasPressedThisFrame)
            UseCurrentHotbarSlot();
    }

    void ToggleInventory()
    {
        if (inventoryPanelRoot == null) return;

        bool currentlyActive = inventoryPanelRoot.activeSelf;
        bool willBeActive    = !currentlyActive;

        inventoryPanelRoot.SetActive(willBeActive);
        IsInventoryOpen = willBeActive;

        // When inventory is open free the mouse
        if (willBeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    // Adds an item into the inventory trying to stack and then fill empty slots
    public bool AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null || amount <= 0) return false;

        int hotbarEndIndex     = Mathf.Min(hotbarSize, inventorySlots.Count);
        int backpackStartIndex = hotbarEndIndex;

        // First try to stack into existing slots if the item is stackable
        if (itemToAdd.isStackable)
        {
            amount = StackIntoExisting(itemToAdd, amount, 0, inventorySlots.Count);
            if (amount <= 0)
            {
                RefreshUI();
                return true;
            }
        }

        // Decide whether to prefer hotbar or backpack for new stacks
        if (itemToAdd.displayInHotbar)
        {
            amount = PlaceInEmptySlots(itemToAdd, amount, 0, hotbarEndIndex);
            if (amount > 0)
                amount = PlaceInEmptySlots(itemToAdd, amount, backpackStartIndex, inventorySlots.Count);
        }
        else
        {
            amount = PlaceInEmptySlots(itemToAdd, amount, backpackStartIndex, inventorySlots.Count);
        }

        RefreshUI();
        return amount <= 0;
    }

    // Try to add to stacks that already contain this item
    int StackIntoExisting(ItemData item, int amount, int startIndex, int endIndex)
    {
        startIndex = Mathf.Clamp(startIndex, 0, inventorySlots.Count);
        endIndex   = Mathf.Clamp(endIndex,   0, inventorySlots.Count);

        for (int i = startIndex; i < endIndex && amount > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];
            if (!slot.IsEmpty() && slot.item == item && slot.count < slot.item.maxStackSize)
            {
                int space = slot.item.maxStackSize - slot.count;
                int toAdd = Mathf.Min(space, amount);
                slot.count += toAdd;
                amount     -= toAdd;
            }
        }

        return amount;
    }

    // Place new stacks in empty slots within the given range
    int PlaceInEmptySlots(ItemData item, int amount, int startIndex, int endIndex)
    {
        startIndex = Mathf.Clamp(startIndex, 0, inventorySlots.Count);
        endIndex   = Mathf.Clamp(endIndex,   0, inventorySlots.Count);

        for (int i = startIndex; i < endIndex && amount > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];
            if (slot.IsEmpty())
            {
                int toAdd = Mathf.Min(amount, item.maxStackSize);
                slot.item  = item;
                slot.count = toAdd;
                amount    -= toAdd;
            }
        }

        return amount;
    }

    // Called from UI when a slot is clicked
    public void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.IsEmpty()) return;

        // If clicked slot is in hotbar, move to backpack; otherwise move to hotbar
        if (slotIndex < hotbarSize)
        {
            MoveStackToBackpack(slotIndex);
        }
        else
        {
            MoveStackToHotbar(slotIndex);
        }

        RefreshUI();
    }

    // Move a whole stack from backpack into the first empty hotbar slot
    void MoveStackToHotbar(int fromIndex)
    {
        InventorySlot src = inventorySlots[fromIndex];

        for (int i = 0; i < hotbarSize; i++)
        {
            InventorySlot dst = inventorySlots[i];
            if (dst.IsEmpty())
            {
                dst.item  = src.item;
                dst.count = src.count;

                src.item  = null;
                src.count = 0;
                return;
            }
        }
    }

    // Move a whole stack from hotbar into the first empty backpack slot
    void MoveStackToBackpack(int fromIndex)
    {
        InventorySlot src = inventorySlots[fromIndex];
        int backpackStartIndex = Mathf.Min(hotbarSize, inventorySlots.Count);

        for (int i = backpackStartIndex; i < inventorySlots.Count; i++)
        {
            InventorySlot dst = inventorySlots[i];
            if (dst.IsEmpty())
            {
                dst.item  = src.item;
                dst.count = src.count;

                src.item  = null;
                src.count = 0;
                return;
            }
        }
    }

    // Rebuilds the entire UI based on current inventory data
    public void RefreshUI()
    {
        if (backpackParent == null || hotbarParent == null || slotPrefab == null) return;

        // Clear old UI slots
        foreach (Transform child in backpackParent)
            Destroy(child.gameObject);
        foreach (Transform child in hotbarParent)
            Destroy(child.gameObject);

        // Recreate all slots for hotbar and backpack
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Transform parentToUse = (i < hotbarSize) ? hotbarParent : backpackParent;
            GameObject newSlot = Instantiate(slotPrefab, parentToUse);

            InventorySlot dataSlot = inventorySlots[i];
            Image icon = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI txt = newSlot.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

            // Hook up the UI slot script so it can notify this manager on clicks
            InventoryUISlot uiSlot = newSlot.GetComponent<InventoryUISlot>();
            if (uiSlot != null)
            {
                uiSlot.slotIndex = i;
                uiSlot.inventory = this;
            }

            // Update icon and text based on item data
            if (!dataSlot.IsEmpty())
            {
                icon.sprite = dataSlot.item.icon;
                icon.color  = Color.white;
                txt.text    = dataSlot.count.ToString();
            }
            else
            {
                icon.sprite = null;
                icon.color  = Color.clear;
                txt.text    = "";
            }
        }
    }

    void SetHotbarIndex(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        currentHotbarIndex = index;
    }

    // Use the item in the currently selected hotbar slot 
    void UseCurrentHotbarSlot()
    {
        if (IsInventoryOpen) return;
        if (playerStats == null) return;
        if (currentHotbarIndex < 0 || currentHotbarIndex >= hotbarSize) return;
        if (currentHotbarIndex >= inventorySlots.Count) return;

        InventorySlot slot = inventorySlots[currentHotbarIndex];
        if (slot == null || slot.IsEmpty()) return;
        if (slot.item == null) return;

        bool used = false;

        // Handle different item effects
        if (slot.item == healthPotionItem)
        {
            playerStats.Heal(healthPotionAmount);
            used = true;
        }
        else if (slot.item == healthRegenPotionItem)
        {
            playerStats.StartHealthRegen(healthRegenTotalAmount, healthRegenDuration);
            used = true;
        }
        else if (slot.item == meatItem)
        {
            playerStats.Heal(meatHealAmount);
            playerStats.RestoreHunger(meatHungerAmount);
            used = true;
        }

        if (!used) return;

        // Consume one item from the stack
        slot.count--;
        if (slot.count <= 0)
        {
            slot.count = 0;
            slot.item = null;
        }

        RefreshUI();
    }
}
