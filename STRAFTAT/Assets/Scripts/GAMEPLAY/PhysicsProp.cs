using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Component.Transforming;
using FishNet.Connection;
using UnityEngine.InputSystem;

public class PhysicsProp : InteractEnvironment
{
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool grabbed;

    private CharacterController physics;
    [SerializeField] private LayerMask defaultMask = default;
    [SerializeField] private float launchForce = 25;
    [SerializeField] private float distanceFromPlayer = 2.5f;
    [SerializeField] private float damage = 1;
    [SerializeField] private AudioClip[] hitClips;
    [SerializeField] private GameObject bloodImpact;
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public FirstPersonController playerThatHasGrabbedBarrel;
    Transform cam;

    [Space]
    [SerializeField] public float gravity = 20;

    public Vector3 moveDirection;
    private AudioSource audio;
    private BoxCollider col;

    bool canHit;
    bool canThrow;
    float throwTimer;
    
    private void Start() {
        audio = GetComponent<AudioSource>();
        physics = GetComponent<CharacterController>();
        col = GetComponent<BoxCollider>();
    }

    public override void OnFocus() {
        PauseManager.Instance.interactPopup.gameObject.SetActive(true);
        // you can't even see this text if the prop is grabbed...
        PauseManager.Instance.interactPopup.text = $"{(grabbed ? "drop" : "grab")} {popupText.ToLower()} [{PauseManager.Instance.InteractPromptLetter}]";
    }

    PlayerPickup LocalPickup;
    public override void OnInteract(Transform player) {
        FirstPersonController playerController = player.GetComponent<FirstPersonController>();
        if (grabbed && playerThatHasGrabbedBarrel != playerController) return;

        LocalPickup = playerController.playerPickupScript;
        if (!LocalPickup.heldEnvironmentInteractable) { LocalPickup.heldEnvironmentInteractable = this; }
        else {
            if (LocalPickup.heldEnvironmentInteractable != this) { return; }
        }
        
        shouldPlaySfx2 = true;
        
        //Launch code
        if (grabbed && playerThatHasGrabbedBarrel == playerController) { AddForce(cam.forward, launchForce); }

        throwTimer = 0.3f;
        
        CmdInteract(playerController);

        cam = player.GetComponentInChildren<Camera>().transform;
    }

    public override void OnLoseFocus()
    {
        
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    void CmdInteract(FirstPersonController player)
    {
        SetGrabbed(!grabbed);
        SetPlayer(grabbed ? player : null);
    }

    [ServerRpc(RunLocally = true, RequireOwnership = false)]
    void SetGrabbed(bool newGrabbed) => grabbed = newGrabbed;
    
    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    void SetPlayer(FirstPersonController player) { playerThatHasGrabbedBarrel = player; }

    [ServerRpc (RunLocally = true)]
    void PlaySfx()
    {
        PlaySfxObservers();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void PlaySfxObservers()
    {
        audio.PlayOneShot(hitClips[(int)Mathf.RoundToInt(Random.Range(0, hitClips.Length))]);
    }
    
    bool shouldPlaySfx;
    bool shouldPlaySfx2;

    void Update()
    {
        throwTimer -= Time.deltaTime;
        canThrow = (throwTimer < 0);

        if (col.isTrigger != grabbed) col.isTrigger = grabbed;
        if (physics.enabled == grabbed) physics.enabled = !grabbed;

        FirstPersonController localPlayer = ClientInstance.Instance.PlayerSpawner.player;
        if (localPlayer && localPlayer != playerThatHasGrabbedBarrel && localPlayer.playerPickupScript.heldEnvironmentInteractable == this) {
            localPlayer.playerPickupScript.heldEnvironmentInteractable = null;
            LocalPickup = null;
        }
        
        if (!IsOwner) { return; }
        
        if (grabbed)
        {
            // Handle dead players and stuff
            if (playerThatHasGrabbedBarrel) {
                PlayerHealth playerHealth = playerThatHasGrabbedBarrel.GetComponent<PlayerHealth>();
                if (playerHealth && playerHealth.isKilled) {
                    SetGrabbed(false);
                    SetPlayer(null);
                }
            } else {
                SetGrabbed(false);
            }
            
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, distanceFromPlayer, defaultMask))
            {
                
                if (shouldPlaySfx)
                {
                    shouldPlaySfx = false;
                    PlaySfx();
                }
                transform.position = hit.point - cam.forward * (col.size.z/2);
            }
            else {
                transform.position = cam.position + cam.forward * distanceFromPlayer;
                shouldPlaySfx = true;
            }

            moveDirection = Vector3.zero;
            customForceFinal = Vector3.zero;
            canHit = true;
            
            if (canThrow && (Input.GetMouseButton(0) || InputManager.inputActions.Player.Interact.WasPerformedThisFrame())) {
                OnInteract(playerThatHasGrabbedBarrel.transform);
                throwTimer = 0.3f;
            }
        }
        else 
        {
            if (!physics.isGrounded)
                moveDirection.y -= gravity * Time.deltaTime;
            physics.Move((moveDirection + customForceFinal) * Time.deltaTime);

            if (impact.magnitude > 0.2F) customForceFinal = impact;
            else customForceFinal = Vector3.zero;

            if (physics.isGrounded) impact.y = 0;

            // consumes the impact energy each cycle:
            impact = Vector3.Lerp(impact, Vector3.zero, (!physics.isGrounded ? airdeceleration : deceleration) *Time.deltaTime);
            
            if (LocalPickup && LocalPickup.heldEnvironmentInteractable == this) {
                LocalPickup.heldEnvironmentInteractable = null;
                LocalPickup = null;
            }
        }
        
    }

    void OnControllerColliderHit(ControllerColliderHit collision)
    {

        customForceFinal = Vector3.zero;

        if (collision.transform.GetComponentInParent<PlayerHealth>() && canHit && impact.magnitude > 2.5f) 
        {
            canHit = false;
            Instantiate(bloodImpact, collision.point, Quaternion.LookRotation(collision.normal));

            if ((collision.transform.GetComponentInParent<PlayerHealth>().health - damage) <= 0) {
                Settings.Instance.propKills ++;
                KillShockWave();
                SendKillLog(collision.transform.GetComponentInParent<PlayerHealth>());
            }

            collision.transform.GetComponentInParent<PlayerHealth>().RemoveHealth(damage);
            BumpPlayerServer(impact, 120, collision.transform.GetComponentInParent<PlayerHealth>());
        }

        if (shouldPlaySfx2)
        {
            shouldPlaySfx2 = false;
            PlaySfx();
        }

    }
    
    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        if (sendKillLog) return;
        sendKillLog = true;

        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was killed with a <b><color=white>{popupText.ToLower()}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
    }

    bool increaseKillAmount;
    public void KillShockWave()
    {
        if (!increaseKillAmount){
            Settings.Instance.IncreaseKillsAmount();
            increaseKillAmount = true;
        }
        FirstPersonController localLocalPlayer = ClientInstance.Instance.PlayerSpawner.player;
        if (!localLocalPlayer) { return; }
        localLocalPlayer.lensDistortion.intensity.value = localLocalPlayer.killShockWaveStrength;
        localLocalPlayer.colorGrading.saturation.value = -100;
    }

    [ServerRpc]
    private void BumpPlayerServer(Vector3 direction, float force, PlayerHealth ph)
    {
        BumpPlayer(ph.playerValues.playerClient.transform.GetComponent<NetworkObject>().Owner, ph, force, direction);
    }

    [TargetRpc]
    void BumpPlayer(NetworkConnection conn, PlayerHealth enemyHealth, float force, Vector3 direction)
    {
        enemyHealth.bounceDirection = direction;
        enemyHealth.bounceForce = force;
        enemyHealth.shouldBounce = true;
        enemyHealth.shouldDropWeapon = true;
    }

    [SerializeField] float mass = 1; // defines the character mass
    Vector3 impact = Vector3.zero;
    Vector3 customForceFinal;

    [SerializeField] public float airdeceleration = 3f;
    [SerializeField] public float deceleration = 5;
    // call this function to add an impact force:
    public void AddForce(Vector3 dir, float force){
        dir.Normalize();
        impact += dir.normalized * force / mass;
    }
}
