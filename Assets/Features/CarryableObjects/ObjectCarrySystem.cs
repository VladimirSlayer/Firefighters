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
        private PlayerController controller;

        private void Awake()
        {
            controller      = GetComponent<PlayerController>();
            cameraTransform = controller.GetComponentInChildren<Camera>()?.transform;
        }

        private void Update()
        {
            if (!IsOwner || !controller.inputEnabled || cameraTransform == null)
                return;

            if (Input.GetMouseButtonDown(1))
                TryGrab();
            else if (Input.GetMouseButtonUp(1))
                TryDrop();
        }

        private void TryGrab()
        {
            if (carriedObject != null)
                return;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                                out var hit, interactDistance))
            {
                if (hit.collider.GetComponentInParent<CarryableObject>() is CarryableObject c)
                {
                    GrabObjectServerRpc(c.NetworkObject, hit.point);
                    carriedObject = c;
                }
            }
        }

        private void TryDrop()
        {
            if (carriedObject == null) return;
            DropObjectServerRpc(carriedObject.NetworkObject);
            carriedObject = null;
        }

        public Vector3 GetHoldPointWorld()
            => cameraTransform.position + cameraTransform.forward * holdDistance;


        [ServerRpc]
        private void GrabObjectServerRpc(NetworkObjectReference objRef, Vector3 hitPoint)
        {
            if (objRef.TryGet(out var obj))
                obj.GetComponent<CarryableObject>()
                   ?.RegisterHandle(NetworkObject, hitPoint);
        }

        [ServerRpc]
        private void DropObjectServerRpc(NetworkObjectReference objRef)
        {
            if (objRef.TryGet(out var obj))
                obj.GetComponent<CarryableObject>()
                   ?.UnregisterHandle(OwnerClientId);
        }
    }
}
