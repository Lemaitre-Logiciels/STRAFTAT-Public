using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    [Tooltip("Radius of this spawn point.")]
    [SerializeField]
    private float _radius = 3f;
    public float Radius => _radius; // Read-only property to access the radius, kinda pointless but whatever

    [SerializeField]
    private bool snapToGround = true;

    private void Awake() { 
        if (snapToGround) { SnapToGround(); }
    }

    private void SnapToGround() {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f)) {
            transform.position = hit.point + Vector3.up * 0.2f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Vector3 playerSpawnPosition;
        Vector3 playerEyeLevel = Vector3.up * 2f;
        if (snapToGround && Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit, 10f)) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, hit.point);
            
            playerSpawnPosition = hit.point;
            playerEyeLevel += hit.point;
        }
        else {
            playerSpawnPosition = transform.position;
            playerEyeLevel += transform.position;
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerSpawnPosition, _radius);
        
        Gizmos.color = Color.green;
        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
        Gizmos.DrawLine(playerEyeLevel, playerEyeLevel + forward * 2f);
    }
#endif
}
