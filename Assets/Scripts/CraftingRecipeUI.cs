using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeUI : MonoBehaviour
{
    [Header("Data")]
    public CraftingRecipe recipe;

    [Header("Result UI")]
    public Image resultIcon;
    public TextMeshProUGUI resultNameText;

    [Header("Ingredient List")]
    public Transform ingredientsParent;
    public GameObject ingredientRowPrefab;

    [Header("Button")]
    public Button craftButton;

    // A small helper class to keep references to each ingredient row in the UI
    class IngredientRow
    {
        public CraftingIngredient ingredient;
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI countText;
    }

    List<IngredientRow> rows = new List<IngredientRow>();
    InventoryManager inventory;

    void Start()
    {
        // Find the player's inventory and build the UI once
        inventory = FindFirstObjectByType<InventoryManager>();
        BuildStaticUI();
    }

    void BuildStaticUI()
    {
        // No recipe means nothing to show
        if (recipe == null)
        {
            return;
        }

        // Set the main result icon and name
        if (resultIcon != null && recipe.resultItem != null)
            resultIcon.sprite = recipe.resultItem.icon;

        if (resultNameText != null)
            resultNameText.text = recipe.recipeName;

        // Clear old ingredient rows
        if (ingredientsParent != null)
        {
            foreach (Transform child in ingredientsParent)
                Destroy(child.gameObject);
        }

        rows.Clear();

        // Create UI rows for each ingredient in the recipe
        if (ingredientsParent != null && ingredientRowPrefab != null)
        {
            foreach (var ing in recipe.ingredients)
            {
                GameObject rowObj = Instantiate(ingredientRowPrefab, ingredientsParent);

                var icon = rowObj.transform.Find("Icon")?.GetComponent<Image>();
                var nameText = rowObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var countText = rowObj.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();

                IngredientRow row = new IngredientRow
                {
                    ingredient = ing,
                    icon = icon,
                    nameText = nameText,
                    countText = countText
                };

                // Set the ingredient icon
                if (row.icon != null && ing.item != null)
                    row.icon.sprite = ing.item.icon;

                // Set the ingredient name
                if (row.nameText != null)
                    row.nameText.text = ing.item != null ? ing.item.itemName : "???";

                rows.Add(row);
            }
        }

        // Hook up the craft button to the crafting system
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftClicked);
        }
    }

    void Update()
    {
        // Update ingredient counts and see if the player can craft
        UpdateCountsAndButton();
    }

    void UpdateCountsAndButton()
    {
        if (inventory == null || recipe == null) return;

        bool canCraft = true;

        // Check each ingredient and show how many the player owns
        foreach (var row in rows)
        {
            if (row.ingredient == null || row.ingredient.item == null) continue;

            int owned = 0;

            // Count items in the inventory
            foreach (InventorySlot slot in inventory.inventorySlots)
            {
                if (!slot.IsEmpty() && slot.item == row.ingredient.item)
                    owned += slot.count;
            }

            // Update "owned / required" text
            if (row.countText != null)
                row.countText.text = owned + "/" + row.ingredient.amount;

            // If the player doesn't have enough, crafting is disabled
            if (owned < row.ingredient.amount)
                canCraft = false;
        }

        // Enable or disable the craft button
        if (craftButton != null)
            craftButton.interactable = canCraft;
    }

    void OnCraftClicked()
    {
        // Ask the CraftingManager to craft this recipe
        if (CraftingManager.Instance == null || recipe == null) return;
        CraftingManager.Instance.Craft(recipe);
    }
}
