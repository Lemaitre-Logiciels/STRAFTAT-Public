using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class ImpulseZone : StraftatTriggerZone {
    [Header("Applied once when a player enters the trigger")]
    public Vector3 force = Vector3.zero;
    protected override void OnPlayerEnter(FirstPersonController player) { player.moveDirection += force; }

    protected override void OnPhysicsPropEnter(PhysicsProp physicsProp) {
        if (!physicsProp.IsOwner) { return; }
        physicsProp.moveDirection += force;
    }
}
