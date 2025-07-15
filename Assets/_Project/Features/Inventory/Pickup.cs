using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class Pickup : NetworkBehaviour
{
    [SerializeField] private Item item;

    bool isInRange;
    NetworkObject playerNetObj;


    void OnTriggerEnter(Collider other)
    {
        if (!IsClient || !other.CompareTag("Player")) return;
        isInRange = true;
        playerNetObj = other.GetComponent<NetworkObject>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsClient || !other.CompareTag("Player")) return;
        isInRange = false;
        playerNetObj = null;
    }

    void Update()
    {
        if (!isInRange || playerNetObj == null || !playerNetObj.IsLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            var inv = playerNetObj.GetComponentInChildren<Inventory>();
            if (inv != null)
            {
                if (!IsServer)              
                    inv.AddItem(item);      

                TryPickUp_ServerRpc(playerNetObj.OwnerClientId); 
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    void TryPickUp_ServerRpc(ulong clientId)
    {
        if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var cl)) return;

        var inv = cl.PlayerObject.GetComponentInChildren<Inventory>();
        if (inv == null) return;
        inv.AddItem(item);
        NetworkObject.Despawn(false);
    }

    [ClientRpc]  
    void UpdateUISlotClientRpc(int slotIdx, ulong targetClientId, ClientRpcParams p = default)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var inv = NetworkManager.LocalClient.PlayerObject.GetComponentInChildren<Inventory>();
        inv?.ClientSetItem(slotIdx, inv.slots[slotIdx].item);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
    }
}
