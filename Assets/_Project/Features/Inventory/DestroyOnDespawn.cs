using UnityEngine;
using Unity.Netcode;

public class DestroyOnDespawn : NetworkBehaviour
{
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Destroy(gameObject);
    }
}
