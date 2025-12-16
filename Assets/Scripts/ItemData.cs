using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;
    public int maxStackSize = 50;

    public GameObject equipmentPrefab;
    public bool displayInHotbar;
    public GameObject worldDropPrefab;
    public bool canEquip = false;
    public GameObject equippedPrefab;

    public ToolType toolType = ToolType.Hand;

    public bool canPlace = true;
    public GameObject placePrefab;
}
