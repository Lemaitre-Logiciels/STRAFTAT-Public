using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class ForceZone : StraftatTriggerZone {
    [Header("Applied continuously to the player while in the zone")]
    public Vector3 force = Vector3.zero;


    // Real jank since on stay runs in fixed update
    protected override void OnPlayerEnter(FirstPersonController player) { _players.Add(player); }
    protected override void OnPlayerExit(FirstPersonController player) { _players.Remove(player); }
    protected override void OnPhysicsPropEnter(PhysicsProp physicsProp) { _physicsProps.Add(physicsProp); }
    protected override void OnPhysicsPropExit(PhysicsProp physicsProp)  { _physicsProps.Remove(physicsProp); } 

    private readonly HashSet<FirstPersonController> _players = new();
    private readonly HashSet<PhysicsProp> _physicsProps = new();
    
    public void Update() {
        float clampedDeltaTime = Mathf.Clamp(Time.deltaTime, 0f, 0.2f); // rip if you are at 5 fps
        Vector3 frameRateFixedForce = force * clampedDeltaTime;
        
        foreach (FirstPersonController player in _players) {
            player.moveDirection += frameRateFixedForce;
        }

        foreach (PhysicsProp physicsProp in _physicsProps) {
            if (!physicsProp.IsOwner) { continue; }
            
            physicsProp.moveDirection += frameRateFixedForce;
        }
    }
}
