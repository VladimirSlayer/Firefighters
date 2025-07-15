using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Netcode;
using UnityEditor.PackageManager;

public class Inventory : NetworkBehaviour
{
    [Header("UI Setup")]
    public GameObject cellPrefab;
    public Transform cellParent;

    [Header("Settings")]
    public int maxSlots = 2;
    private int selectedSlot = -1;

    public int SelectedSlot => selectedSlot;

    public List<InventorySlot> slots;

    [SerializeField] private Transform equipPoint;
    private NetworkObject equippedNetObj;

    private InventoryCell[] cells;

    public void selectedSlotReset()
    {
        ToggleSelection(selectedSlot);
        selectedSlot = -1;
    }

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

    void Update()
    {
        if (cells == null) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleSelection(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleSelection(1);
    }

    public void ToggleSelection(int index)
    {
        if (index < 0 || index >= cells.Length) return;

        if (slots[index].IsEmpty) return;

        if (selectedSlot == index)
        {
            cells[index].SetSelected(false);
            selectedSlot = -1;
            UnequipItemServerRpc();
        }
        else
        {
            if (selectedSlot >= 0)
                cells[selectedSlot].SetSelected(false);
            cells[index].SetSelected(true);
            selectedSlot = index;
            EquipItemServerRpc(index);
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

    [ServerRpc(RequireOwnership = false)]
    private void EquipItemServerRpc(int slotIndex, ServerRpcParams rpcParams = default)
    {
        var item = slots[slotIndex].item;
        if (item == null || item.equippedPrefab == null) return;

        var go = Instantiate(item.equippedPrefab);
        var netObj = go.GetComponent<NetworkObject>() ?? go.AddComponent<NetworkObject>();
        netObj.Spawn(true);

        equippedNetObj = netObj;

        ulong playerObjId = this.NetworkObject.NetworkObjectId;
        ulong itemObjId = netObj.NetworkObjectId;
        EquipItemClientRpc(itemObjId, playerObjId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnequipItemServerRpc(ServerRpcParams rpcParams = default)
    {
        if (equippedNetObj != null)
        {
            equippedNetObj.Despawn(false);
            Destroy(equippedNetObj.gameObject);
            equippedNetObj = null;
        }
    }

    [ClientRpc]
    private void EquipItemClientRpc(ulong itemObjectId, ulong playerObjectId, ClientRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemObjectId, out var itemNetObj))
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjectId, out var playerNetObj))
            return;

        itemNetObj.TrySetParent(playerNetObj);

        Vector3 localPos = playerNetObj.transform.InverseTransformPoint(equipPoint.position);
        Quaternion localRot = Quaternion.Inverse(playerNetObj.transform.rotation)
                             * equipPoint.rotation;

        itemNetObj.transform.localPosition = localPos;
        itemNetObj.transform.localRotation = localRot;
    }
}
