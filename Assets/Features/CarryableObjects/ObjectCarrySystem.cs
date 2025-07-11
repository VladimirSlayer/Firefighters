using UnityEngine;
using Unity.Netcode;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class ObjectCarrySystem : NetworkBehaviour
    {
        [Header("Настройки захвата")]
        public float interactDistance = 3f;
        public float holdDistance     = 1.5f;

        private CarryableObject carriedObject;
        private Transform        cameraTransform;
        private PlayerController pc;

        private void Awake()
        {
            pc             = GetComponent<PlayerController>();
            cameraTransform = pc.GetComponentInChildren<Camera>()?.transform;
        }

        private void Update()
        {
            if (!IsOwner || !pc.inputEnabled || cameraTransform == null) return;
            if (Input.GetMouseButtonDown(1))
            {
                if (carriedObject == null) TryGrab(); else TryDrop();
            }
        }

        private void TryGrab()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                                out var hit, interactDistance))
            {
                if (hit.collider.GetComponentInParent<CarryableObject>() is CarryableObject c)
                {
                    float share = c.weight / c.RequiredHandles;
                    pc.SetCarryLoad(share);
                    carriedObject = c;

                    if (IsServer) c.RegisterHandle(NetworkObject, hit.point);
                    else          GrabObjectServerRpc(c.NetworkObject, hit.point);

                    int need = c.RequiredHandles - c.HandleCount;
                    if (need > 0) Debug.Log($"Нужно ещё {need} игрок(ов)");
                }
            }
        }

        private void TryDrop()
        {
            if (carriedObject == null) return;
            pc.SetCarryLoad(0f);
            if (IsServer) carriedObject.UnregisterHandle(OwnerClientId);
            else          DropObjectServerRpc(carriedObject.NetworkObject);
            carriedObject = null;
        }

        public Vector3 GetHoldPointWorld()
            => cameraTransform.position + cameraTransform.forward * holdDistance;

        [ServerRpc(RequireOwnership = false)]
        private void GrabObjectServerRpc(NetworkObjectReference objRef, Vector3 hitPoint)
        {
            if (objRef.TryGet(out var obj))
                obj.GetComponent<CarryableObject>()
                   ?.RegisterHandle(NetworkObject, hitPoint);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DropObjectServerRpc(NetworkObjectReference objRef)
        {
            if (objRef.TryGet(out var obj))
                obj.GetComponent<CarryableObject>()
                   ?.UnregisterHandle(OwnerClientId);
        }
    }
}
