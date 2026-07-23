using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class PhysicsPropDecelerationZone : StraftatTriggerZone {
    [SerializeField]
    private float decelerationMultiplier = 1f;

    protected override void Awake() {
        base.Awake();
        if (decelerationMultiplier == 0) {
            Debug.LogWarning("Deceleration multiplier is zero, setting to 0.01 to avoid division by zero errors.");
            decelerationMultiplier = 0.01f;
        }
    }

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
        physicsProp.airdeceleration *= decelerationMultiplier;
        physicsProp.deceleration *= decelerationMultiplier;
        
    }
    protected override void OnPhysicsPropExit(PhysicsProp physicsProp) {
        _physicsProps.Remove(physicsProp);
        physicsProp.airdeceleration /= decelerationMultiplier;
        physicsProp.deceleration /= decelerationMultiplier;
    }
}
