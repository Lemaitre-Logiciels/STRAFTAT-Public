using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class WallJumpZone : StraftatTriggerZone {
    [SerializeField]
    private bool resetAllWallJumps = true;
    
    #if UNITY_EDITOR
    private bool NotResetAllWallJumps => !resetAllWallJumps; // Dumb thing so ShowIf can work in the way I want it to
    [ShowIf("NotResetAllWallJumps")]
    #endif
    [SerializeField]
    private int wallJumpsToAdd = 1;
    
    protected override void OnPlayerEnter(FirstPersonController player) {
        if (resetAllWallJumps) { player.wallJumpsCount = 0; } 
        else { player.wallJumpsCount -= wallJumpsToAdd; }
    }
}
