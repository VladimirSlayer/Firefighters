using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject cellPrefab;
    public Transform cellParent;

    [Header("Settings")]
    public int maxSlots = 2;

    public List<InventorySlot> slots;

    private InventoryCell[] cells;

    void Awake()
    {
        slots = new List<InventorySlot>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
            slots.Add(new InventorySlot());
    }

    public void InitUI()
    {
        if (cellParent == null)
        {
            var found = transform.Find("InventoryUI/InventoryContainer");
            if (found == null) { Debug.LogWarning("InventoryContainer не найден"); return; }
            cellParent = found;
        }

        cells = new InventoryCell[maxSlots];
        for (int i = 0; i < maxSlots; i++)
        {
            var go = Instantiate(cellPrefab, cellParent);
            var cell = go.GetComponent<InventoryCell>();
            cells[i] = cell;
            if (slots[i].IsEmpty) cell.Clear();
            else cell.SetItem(slots[i].item);
        }
    }


    private void InitializeUI()
    {
        cells = new InventoryCell[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject cellGO = Instantiate(cellPrefab, cellParent.transform);
            InventoryCell cell = cellGO.GetComponent<InventoryCell>();
            cell.Clear();
            cells[i] = cell;
        }
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].Set(item);

                if (cells != null && cells[i] != null)
                    cells[i].SetItem(item);

                return true;
            }
        }
        Debug.Log("Инвентарь заполнен");
        return false;
    }

    public void ClientSetItem(int slotIndex, Item item)
    {
        if (cells == null || slotIndex >= cells.Length) return;

        slots[slotIndex].Set(item);          
        cells[slotIndex].SetItem(item);     
    }

    public void RemoveItem(Item item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].Clear();
                if (cells != null && cells[i] != null)
                    cells[i].Clear();
                return;
            }
        }
    }
}
