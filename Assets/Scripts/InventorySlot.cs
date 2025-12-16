using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item; 
    public int count; 

    public InventorySlot()
    {
        item = null;
        count = 0;
    }

    public bool IsEmpty()
    {
        return item == null || count <= 0;
    }
}