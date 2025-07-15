using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Features.Player;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class CarryableObject : NetworkBehaviour
{
    [Header("Вес и сила")]
    public float weight         = 30f;
    public float playerStrength = 30f;
    public int   maxHandles     = 4;

    [Header("Пружина захвата")]
    public float springForce    = 500f;
    public float springDamper   = 50f;

    [Header("Slack & Break")]
    public float slackDistance  = 0.5f;
    public float breakDistance  = 2f;

    private Rigidbody rb;

    class HandleInfo
    {
        public ObjectCarrySystem system;
        public Vector3           localPoint;
        public ConfigurableJoint joint;
        public Transform         anchorTransform;
    }

    private Dictionary<ulong, HandleInfo> handles = new();

    public int HandleCount => handles.Count;
    public int RequiredHandles =>
        Mathf.Clamp(Mathf.CeilToInt(weight / playerStrength), 1, maxHandles);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void RegisterHandle(NetworkObject playerObj, Vector3 worldHitPoint)
    {
        if (!IsServer || handles.ContainsKey(playerObj.OwnerClientId)) return;
        var system = playerObj.GetComponent<ObjectCarrySystem>();
        if (system == null) return;

        var info         = new HandleInfo();
        info.system      = system;
        info.localPoint  = transform.InverseTransformPoint(worldHitPoint);

        var anchorGO                = new GameObject($"CarryAnchor_{playerObj.OwnerClientId}");
        anchorGO.transform.parent   = system.transform;
        anchorGO.transform.position = system.GetHoldPointWorld();
        var anchorRb = anchorGO.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        var joint                          = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody               = anchorRb;
        joint.anchor                      = info.localPoint;
        joint.connectedAnchor             = Vector3.zero;
        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit { limit = slackDistance };

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
        if (!IsServer || !handles.TryGetValue(clientId, out var info)) return;
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
            ulong clientId = kv.Key;
            var info      = kv.Value;

            Vector3 attachPos = transform.TransformPoint(info.localPoint);
            Vector3 holdPos   = info.system.GetHoldPointWorld();
            float   dist      = Vector3.Distance(attachPos, holdPos);

            if (dist > breakDistance) { toUnregister.Add(clientId); continue; }
            if (handles.Count >= RequiredHandles &&
                Vector3.Distance(info.anchorTransform.position, holdPos) > slackDistance)
            {
                info.anchorTransform.position = holdPos;
            }
        }
        foreach (var cid in toUnregister) UnregisterHandle(cid);
    }
}
