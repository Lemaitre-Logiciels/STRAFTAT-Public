using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class GravityZone : StraftatTriggerZone {
    [SerializeField]
    private float gravityMultiplier = 1f;

    protected override void Awake() {
        base.Awake();
        if (gravityMultiplier == 0) {
            Debug.LogWarning("Gravity multiplier is zero, setting to 0.01 to avoid division by zero errors.");
            gravityMultiplier = 0.01f;
        }
    }

    protected override void OnPlayerEnter(FirstPersonController player) { player.gravityMultiplier *= gravityMultiplier; }
    protected override void OnPlayerExit(FirstPersonController player) { player.gravityMultiplier /= gravityMultiplier; }

    private readonly HashSet<PhysicsProp> _physicsProps = new HashSet<PhysicsProp>();
    private readonly HashSet<PhysicsProp> _lastUpdatePhysicsProps = new HashSet<PhysicsProp>();
    public void FixedUpdate() {
        foreach (PhysicsProp physicsProp in _physicsProps) {
            if (!_lastUpdatePhysicsProps.Contains(physicsProp)) { OnPhysicsPropExit(physicsProp); }
        }
        _lastUpdatePhysicsProps.Clear();
    }
    protected override void OnPhysicsPropStay(PhysicsProp physicsProp) { _lastUpdatePhysicsProps.Add(physicsProp); }
    
    protected override void OnPhysicsPropEnter(PhysicsProp physicsProp) {
        if (!_physicsProps.Add(physicsProp)) { return; }
        physicsProp.gravity *= gravityMultiplier;
    }
    protected override void OnPhysicsPropExit(PhysicsProp physicsProp) {
        _physicsProps.Remove(physicsProp);
        physicsProp.gravity /= gravityMultiplier;
    }
}
