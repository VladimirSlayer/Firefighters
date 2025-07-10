using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Features.Player;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class CarryableObject : NetworkBehaviour
{
    [Header("Пружина захвата")]
    public float springForce   = 500f;
    public float springDamper  = 50f;
    [Tooltip("Макс. расстояние между точкой захвата и удержания, до которого связь разрывается")]
    public float breakDistance = 2f;

    private Rigidbody rb;

    class HandleInfo
    {
        public ObjectCarrySystem system;
        public Vector3           localPoint;
        public ConfigurableJoint joint;
        public Transform         anchorTransform;
    }
    private Dictionary<ulong, HandleInfo> handles = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation            = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode   = CollisionDetectionMode.ContinuousDynamic;
    }

    public void RegisterHandle(NetworkObject playerObj, Vector3 worldHitPoint)
    {
        if (!IsServer || handles.ContainsKey(playerObj.OwnerClientId))
            return;

        var system = playerObj.GetComponent<ObjectCarrySystem>();
        if (system == null) return;

        var info = new HandleInfo {
            system     = system,
            localPoint = transform.InverseTransformPoint(worldHitPoint)
        };


        var anchorGO = new GameObject($"CarryAnchor_{playerObj.OwnerClientId}");
        anchorGO.transform.parent   = system.transform;
        anchorGO.transform.position = system.GetHoldPointWorld();
        var anchorRb = anchorGO.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;


        var joint                          = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody               = anchorRb;
        joint.anchor          = info.localPoint;
        joint.connectedAnchor = Vector3.zero;
        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit { limit = 0f };

        var drive = new JointDrive {
            positionSpring = springForce,
            positionDamper = springDamper,
            maximumForce   = Mathf.Infinity
        };
        joint.xDrive = joint.yDrive = joint.zDrive = drive;
        joint.linearLimitSpring = new SoftJointLimitSpring {
            spring = springForce,
            damper = springDamper
        };

        info.joint           = joint;
        info.anchorTransform = anchorGO.transform;
        handles[playerObj.OwnerClientId] = info;
    }

    public void UnregisterHandle(ulong clientId)
    {
        if (!IsServer || !handles.TryGetValue(clientId, out var info))
            return;

        Destroy(info.joint);
        Destroy(info.anchorTransform.gameObject);
        handles.Remove(clientId);
    }

    private void FixedUpdate()
    {
        if (!IsServer || handles.Count == 0) return;

        var toUnregister = new List<ulong>();

        foreach (var kv in handles)
        {
            var clientId       = kv.Key;
            var info           = kv.Value;

            Vector3 attachPos  = transform.TransformPoint(info.localPoint);

            Vector3 holdPos    = info.system.GetHoldPointWorld();
            float dist         = Vector3.Distance(attachPos, holdPos);

            if (dist > breakDistance)
            {
                toUnregister.Add(clientId);
                continue;
            }


            info.anchorTransform.position = holdPos;
        }

        foreach (var clientId in toUnregister)
            UnregisterHandle(clientId);
    }
}
