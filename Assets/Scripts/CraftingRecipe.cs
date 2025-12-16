using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Game/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public ItemData resultItem;
    public int resultAmount = 1;

    public CraftingIngredient[] ingredients;
}
