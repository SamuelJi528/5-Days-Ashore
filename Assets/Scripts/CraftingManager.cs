using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    InventoryManager inventory;

    void Awake()
    {
        // Make sure there is only one CraftingManager in the scene
        if (Instance != null && Instance != this)
        {
        }
        Instance = this;
    }

    void Start()
    {
        // Find the player's inventory when the game starts
        inventory = FindFirstObjectByType<InventoryManager>();
    }

    public void TryCraftByIndex(int index)
    {
        // Make sure the index is within the recipe list
        if (index < 0 || index >= recipes.Count) return;

        // Try crafting the recipe
        Craft(recipes[index]);
    }

    public void Craft(CraftingRecipe recipe)
    {
        // Safety checks
        if (recipe == null || inventory == null) return;

        // Stop if the player doesn't have the ingredients
        if (!CanCraft(recipe))
        {
            return;
        }

        // Remove the needed items from the inventory
        RemoveIngredients(recipe);

        // Give the player the crafted item
        inventory.AddItem(recipe.resultItem, recipe.resultAmount);
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        // Basic safety checks
        if (recipe == null || inventory == null) return false;

        // Check every ingredient in the recipe
        foreach (var cost in recipe.ingredients)
        {
            if (cost.item == null || cost.amount <= 0) continue;

            int owned = 0;

            // Count how many of this ingredient the player has
            foreach (InventorySlot slot in inventory.inventorySlots)
            {
                if (!slot.IsEmpty() && slot.item == cost.item)
                    owned += slot.count;
            }

            // If the player doesn't have enough, crafting is not possible
            if (owned < cost.amount)
                return false;
        }

        return true;
    }

    void RemoveIngredients(CraftingRecipe recipe)
    {
        // More safety checks
        if (recipe == null || inventory == null) return;

        // Go through each ingredient and remove it
        foreach (var cost in recipe.ingredients)
        {
            if (cost.item == null || cost.amount <= 0) continue;

            int remaining = cost.amount;

            // Remove items from inventory slots until the requirement is met
            foreach (InventorySlot slot in inventory.inventorySlots)
            {
                if (!slot.IsEmpty() && slot.item == cost.item)
                {
                    // Slot has more than needed
                    if (slot.count > remaining)
                    {
                        slot.count -= remaining;
                        remaining = 0;
                    }
                    else
                    {
                        // Slot does not have enough; empty it
                        remaining -= slot.count;
                        slot.count = 0;
                        slot.item = null;
                    }

                    // Finished removing this ingredient
                    if (remaining <= 0)
                        break;
                }
            }
        }

        // Update the inventory UI after the changes
        inventory.RefreshUI();
    }
}
