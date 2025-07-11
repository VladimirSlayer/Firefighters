[System.Serializable]
public class InventorySlot
{
    public Item item;
    public bool IsEmpty => item == null;

    public void Set(Item i)  => item = i;
    public void Clear()      => item = null;
}
