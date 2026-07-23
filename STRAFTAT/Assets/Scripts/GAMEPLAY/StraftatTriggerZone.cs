using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class StraftatTriggerZone : MonoBehaviour {
    private Collider Collider;

    protected virtual void Awake() {
        Collider = GetComponent<Collider>();
        if (Collider is MeshCollider meshCollider) { 
            meshCollider.convex = true; // Ensure MeshCollider is convex to allow triggers
        }
        Collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) { OnPlayerEnter(player); return; }
        
        PhysicsProp physicsProp = other.GetComponent<PhysicsProp>();
        if (physicsProp != null) { OnPhysicsPropEnter(physicsProp); return; }
    }
    private void OnTriggerExit(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) { OnPlayerExit(player); return; }
        
        PhysicsProp physicsProp = other.GetComponent<PhysicsProp>();
        if (physicsProp != null) { OnPhysicsPropExit(physicsProp); return; }
    }
    private void OnTriggerStay(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) { OnPlayerStay(player); return; }
        
        PhysicsProp physicsProp = other.GetComponent<PhysicsProp>();
        if (physicsProp != null) { OnPhysicsPropStay(physicsProp); return; }
    }

    protected virtual void OnPlayerEnter(FirstPersonController player) { }
    protected virtual void OnPlayerExit(FirstPersonController player) { }
    protected virtual void OnPlayerStay(FirstPersonController player) { }
    
    protected virtual void OnPhysicsPropEnter(PhysicsProp physicsProp) { }
    protected virtual void OnPhysicsPropExit(PhysicsProp physicsProp) { }
    protected virtual void OnPhysicsPropStay(PhysicsProp physicsProp) { }
}

