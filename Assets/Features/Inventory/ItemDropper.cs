using UnityEngine;
using Unity.Netcode;
using Features.Player;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerController))]
public class ItemDropper : NetworkBehaviour
{
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float forwardForce = 2f;
    [SerializeField] private float upwardForce = 2f;

    private Camera playerCam;

    private Inventory inventory;

    private void Awake()
    {
        inventory = GetComponentInChildren<Inventory>();
        if (inventory == null)
            Debug.Log($"[{name}] Не найден Inventory на игроке для дропа");
        playerCam = GetComponentInChildren<PlayerController>().PlayerCam.GetComponent<Camera>();
        if (playerCam == null)
        {
            Debug.Log($"[{name}] Не найден Camera на игроке для дропа");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q) && inventory.SelectedSlot >= 0)
        {
            TryLocalDrop();
        }
    }

    private void TryLocalDrop()
    {
        int slot = inventory.SelectedSlot;
        if (slot < 0 || slot >= inventory.slots.Count) return;

        Item item = inventory.slots[slot].item;
        if (item == null) return;

        if (!IsServer)
            inventory.RemoveItem(item);

        inventory.selectedSlotReset();
        DropItemServerRpc(slot);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropItemServerRpc(int slotIndex, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var cl)) return;

        var inv = cl.PlayerObject.GetComponentInChildren<Inventory>();
        if (inv == null) return;

        Item item = inv.slots[slotIndex].item;
        if (item == null) return;
        inv.RemoveItem(item);

        if (item.worldPrefab != null)
        {
            Vector3 spawnPos = playerCam.transform.position + playerCam.transform.forward * dropDistance;
            GameObject go = Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

            var netObj = go.GetComponent<NetworkObject>() ?? go.AddComponent<NetworkObject>();

            netObj.Spawn();

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();

            Vector3 impulse = transform.forward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
