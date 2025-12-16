using UnityEngine;

public enum ResourceInteractionType
{
    Harvest,
    Drink
}

public class ResourceNode : MonoBehaviour
{
    public ResourceInteractionType interactionType = ResourceInteractionType.Harvest; 
    public string promptText = "";                                                   
    public ToolType toolRequired = ToolType.Hand;                                    // Tool needed to harvest
    public int resourceHealth = 3;                                                   

    public string animationTrigger = "Chop";                                         
    public float gatherDelay = 0.5f;                                                 // Delay before resource is given

    public float drinkAmount = 25f;                                                  // How much hydration is restored when drinking

    InventoryManager inventory;
    ItemDrop dropData;
    PlayerEquipment playerEquipment;
    PlayerStats playerStats;

    void Start()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
        playerEquipment = FindFirstObjectByType<PlayerEquipment>();
        dropData = GetComponent<ItemDrop>();
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    // Check if the player has the right tool for this node
    public bool HasCorrectTool()
    {
        // Drinking does not require a tool
        if (interactionType == ResourceInteractionType.Drink)
            return true;

        // Harvest that only needs hands
        if (toolRequired == ToolType.Hand)
            return true;

        ToolType equippedTool = ToolType.Hand;

        // Try to read the tool type from the equipped item
        if (playerEquipment != null)
        {
            ItemData equippedItem = playerEquipment.GetEquippedItem();
            if (equippedItem != null)
                equippedTool = equippedItem.toolType;
        }

        return equippedTool == toolRequired;
    }

    // Called when the player finishes the gather interaction
    public void GatherResource()
    {
        // Drinking nodes restore water and do not drop items
        if (interactionType == ResourceInteractionType.Drink)
        {
            if (playerStats != null)
                playerStats.DrinkWater(drinkAmount);
            return;
        }

        if (inventory == null || dropData == null || dropData.droppedItem == null) return;
        if (!HasCorrectTool()) return;

        // Reduce node health (number of hits left)
        resourceHealth--;

        // When the node is broken, give items to the player
        if (resourceHealth <= 0)
        {
            int amount = Mathf.Max(1, dropData.stackCount);
            bool success = inventory.AddItem(dropData.droppedItem, amount);

            // If inventory has space, destroy the node
            if (success)
                Destroy(gameObject);
             // Keep it at 1 so the node isn't gone while inventory is full
            else
                resourceHealth = 1;
        }
    }
}
