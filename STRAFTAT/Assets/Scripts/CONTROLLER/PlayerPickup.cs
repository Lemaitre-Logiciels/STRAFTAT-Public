using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class PlayerPickup : NetworkBehaviour
{
    private PlayerControls playerControls;
    private FirstPersonController playerController;
    private InputAction interact, drop, change;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    [SerializeField] private LayerMask environmentInteractionLayer = default;
    [SerializeField] private LayerMask bodyInteractionLayer = default;
    [SerializeField] private LayerMask ragdollInteractionLayer = default;
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float maxSphereDistance = 5;
    [SerializeField] private float maxInteractionDistance = 5;
    [SerializeField] private float currentHitDistance = default;
    [SerializeField] private LayerMask landLayer = default;
    [SerializeField] private Animator animator;
    [SerializeField] public Animator globalAnimator;
    public Transform fpArms;
    public Interactable currentInteractable;
    public InteractEnvironment currentEnvironmentInteractable;
    public InteractEnvironment heldEnvironmentInteractable;
    public GameObject currentObject, currentHitObject, currentEnvironmentObject;
    private bool interactionSphere;
    [HideInInspector] public PlayerValues playerValues;

    public Transform[] pickupPositionRightHand;
    public Transform[] pickupPositionBothHand;
    public Transform[] pickupPositionLeftHand;
    public Transform[] aimPositionBothHand;
    public Transform[] aimPositionRightHand;
    public Transform[] pickupPositionOnline;

    /// <summary>
    /// we have this as an extra buffer layer so we don't set
    /// RightHandIK.data.target directly
    /// **we don't want to set the target directly as if the object is ever destroyed it will crash the game!!!**
    /// </summary>
    public Transform RightHandIKTarget;

    /// <summary>
    /// we have this as an extra buffer layer so we don't set
    /// LeftHandIK.data.target directly
    /// **we don't want to set the target directly as if the object is ever destroyed it will crash the game!!!**
    /// </summary>
    public Transform LeftHandIKTarget;
    
    private Transform TargetForRightIK;
    public void SetRightIKTarget(Transform transform) {
        TargetForRightIK = transform;
    }
    private Transform TargetForLeftIK;
    public void SetLeftIKTarget(Transform transform) {
        TargetForLeftIK = transform;
    }
    
    public void UpdateIKPoistion() {
        if (TargetForRightIK) { RightHandIKTarget.SetPositionAndRotation(TargetForRightIK.position, TargetForRightIK.rotation); }
        else { RightHandIKTarget.SetPositionAndRotation(RightIdlePosition.position, RightIdlePosition.rotation); }
        
        if (TargetForLeftIK) { LeftHandIKTarget.SetPositionAndRotation(TargetForLeftIK.position, TargetForLeftIK.rotation); }
        else { LeftHandIKTarget.SetPositionAndRotation(LeftIdlePosition.position, LeftIdlePosition.rotation); }
    }
    
    [SerializeField] private Transform LeftIdlePosition;
    [SerializeField] private Transform RightIdlePosition;
    [SerializeField] private RigBuilder RigBuilder;

    [Space]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip switchWeaponsClip;

    [Space]

    Camera cam;
    [SerializeField] private GameObject graphics;
    [SerializeField] private GameObject RightHandPositions;
    [SerializeField] private GameObject BothHandPositions;
    [SerializeField] private GameObject LeftHandPositions;
    [SerializeField] private GameObject AimBothHandPositions;
    [SerializeField] private GameObject AimRightHandPositions;
    [SerializeField] private GameObject OnlinePositions;
    GameObject camHolder;
    public CameraShakeConstrains camAnimScript;
    
    [SyncVar] public bool hasObjectInHand;
    [SyncVar] public GameObject objInHand;
    private Weapon weaponInHand;
    private ItemBehaviour behaviourInHand;

    [SyncVar] public bool hasObjectInLeftHand;
    [SyncVar] public GameObject objInLeftHand;
    private Weapon weaponInLeftHand;
    private ItemBehaviour behaviourInLeftHand;

    private PauseManager pauseManager;

    public override void OnStartClient()
    {
        base.OnStartClient();

        cam = GetComponent<FirstPersonController>().playerCamera;
        camHolder = GetComponent<FirstPersonController>().playerCameraHolder;
        playerController = GetComponent<FirstPersonController>();

        pickupPositionRightHand = new Transform[RightHandPositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < pickupPositionRightHand.Length; i++)
        {
            pickupPositionRightHand[i] = RightHandPositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        pickupPositionLeftHand = new Transform[LeftHandPositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < pickupPositionLeftHand.Length; i++)
        {
            pickupPositionLeftHand[i] = LeftHandPositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        pickupPositionBothHand = new Transform[BothHandPositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < pickupPositionBothHand.Length; i++)
        {
            pickupPositionBothHand[i] = BothHandPositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        aimPositionBothHand = new Transform[AimBothHandPositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < aimPositionBothHand.Length; i++)
        {
            aimPositionBothHand[i] = AimBothHandPositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        aimPositionRightHand = new Transform[AimRightHandPositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < aimPositionRightHand.Length; i++)
        {
            aimPositionRightHand[i] = AimRightHandPositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        pickupPositionOnline = new Transform[OnlinePositions.GetComponentsInChildren<ItemPosition>().Length];

        for (int i=0; i < pickupPositionOnline.Length; i++)
        {
            pickupPositionOnline[i] = OnlinePositions.GetComponentsInChildren<ItemPosition>()[i].transform;
        }

        
        
    }

    private void Awake()
    {
        playerControls = InputManager.inputActions;
        playerValues = GetComponent<PlayerValues>();
        pauseManager = PauseManager.Instance;
    }
    private void OnEnable()
    {
        interact = playerControls.Player.Interact;
        interact.Enable();
        interact.performed += HandleInteraction;

        drop = playerControls.Player.Drop;
        drop.Enable();
        drop.performed += HandleDrop;

        change = playerControls.Player.ChangeWeapon;
        change.Enable();
        change.performed += HandleDrop;
    }
    private void OnDisable()
    {
        interact.Disable();
        drop.Disable();
        change.Disable();

        change.performed -= HandleDrop;
        drop.performed -= HandleDrop;
        interact.performed -= HandleInteraction;
    }

    float dropTimer;
    float interactTimer;

    private void Update() {
        UpdateIKPoistion();
        
        if (!base.IsOwner) return;

        RightHandFix();
        LeftHandFix();

        dropTimer -= Time.deltaTime;
        interactTimer -= Time.deltaTime;

        if (!hasObjectInHand) {
            playerController.movementFactor = 1;
            playerController.jumpFactor = 1;
            playerController.maxWallJumps = 1;
            playerController.wallJumpFactor = 1;
        }

        if (cam != null) {
            HandleInteractionCheck();
            HandleInteractEnvironment();
            HandleAboubiGrab();

            if (objInHand == null && objInLeftHand == null) 
            {
                animator.SetBool("TwoHanded", false);
                animator.SetBool("DoubleHanded", false);
                animator.SetBool("RightHanded", false);

                globalAnimator.SetBool("TwoHanded", false);
                globalAnimator.SetBool("DoubleSingle", false);
                globalAnimator.SetBool("SingleHanded", false);
                globalAnimator.SetBool("LeftHanded", false);
                return;
            }

            if (objInLeftHand == null && objInHand != null) {
                if (weaponInHand.requireBothHands ){
                    if (behaviourInHand.rightHandAnim == "") {
                    animator.SetBool("TwoHanded", true);
                    animator.SetBool("DoubleHanded", false);
                    animator.SetBool("RightHanded", false);
                    }

                    globalAnimator.SetBool("TwoHanded", true);
                    globalAnimator.SetBool("DoubleSingle", false);
                    globalAnimator.SetBool("SingleHanded", false);
                    globalAnimator.SetBool("LeftHanded", false);
                }
                else if (!weaponInHand.requireBothHands){
                    if ( behaviourInHand.rightHandAnim == "") {
                    animator.SetBool("TwoHanded", false);
                    animator.SetBool("DoubleHanded", false);
                    animator.SetBool("RightHanded", true);
                    }

                    globalAnimator.SetBool("TwoHanded", false);
                    globalAnimator.SetBool("DoubleSingle", false);
                    globalAnimator.SetBool("SingleHanded", true);
                    globalAnimator.SetBool("LeftHanded", false);
                }
            }
            else if (objInLeftHand != null && objInHand != null) {
                if (behaviourInLeftHand.leftHandAnim == "" && behaviourInHand.rightHandAnim == ""){
                    animator.SetBool("TwoHanded", false);
                    animator.SetBool("DoubleHanded", true);
                    animator.SetBool("RightHanded", false);
                }

                    globalAnimator.SetBool("TwoHanded", false);
                    globalAnimator.SetBool("DoubleSingle", true);
                    globalAnimator.SetBool("SingleHanded", false);
                    globalAnimator.SetBool("LeftHanded", false);
            }
            else if (objInLeftHand != null && objInHand == null) {
                if (behaviourInLeftHand.leftHandAnim == ""){
                    animator.SetBool("TwoHanded", false);
                    animator.SetBool("DoubleHanded", false);
                    animator.SetBool("RightHanded", true);
                }

                    globalAnimator.SetBool("TwoHanded", false);
                    globalAnimator.SetBool("DoubleSingle", false);
                    globalAnimator.SetBool("SingleHanded", false);
                    globalAnimator.SetBool("LeftHanded", true);
            }
            else if (!weaponInHand.requireBothHands){

                if (behaviourInHand.rightHandAnim == "") {
                animator.SetBool("TwoHanded", false);
                animator.SetBool("DoubleHanded", false);
                animator.SetBool("RightHanded", true);
                }

                globalAnimator.SetBool("TwoHanded", false);
                globalAnimator.SetBool("DoubleSingle", false);
                globalAnimator.SetBool("SingleHanded", true);
                globalAnimator.SetBool("LeftHanded", false);
            }
            
        }

        
    }

    void RightHandPickup()
    {        
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            if (!hasObjectInHand && hit.transform.gameObject.layer == 7)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;

                if (objInHand.GetComponent<Weapon>().requireBothHands) 
                {
                    SetObjectInHandServer(objInHand, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                    SetLeftIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
                }
                else 
                {
                    SoundManager.Instance.PlaySound(pickupClip);
                    SetObjectInHandServer(objInHand, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                }
                RigBuilder.Build();
            }
            else if (hasObjectInHand)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                RightHandDrop();
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;

                if (objInHand.GetComponent<Weapon>().requireBothHands) 
                {
                    SetObjectInHandServer(objInHand, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                    SetLeftIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
                }
                else 
                {
                    SetObjectInHandServer(objInHand, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                }
                RigBuilder.Build();

            }
        }
        else if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out RaycastHit spherehit, currentHitDistance, interactionLayer))
        {
            if (!hasObjectInHand && spherehit.transform.gameObject.layer == 7)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                objInHand = spherehit.transform.gameObject;
                hasObjectInHand = true;

                if (objInHand.GetComponent<Weapon>().requireBothHands) 
                {
                    SetObjectInHandServer(objInHand, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                    SetLeftIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
                }
                else 
                {
                    SetObjectInHandServer(objInHand, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                }
                RigBuilder.Build();

                
            }
            else if (hasObjectInHand)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                RightHandDrop();
                objInHand = spherehit.transform.gameObject;
                hasObjectInHand = true;

                if (objInHand.GetComponent<Weapon>().requireBothHands) 
                {
                    SetObjectInHandServer(objInHand, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionBothHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                    SetLeftIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
                }
                else 
                {
                    SetObjectInHandServer(objInHand, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[objInHand.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                    SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
                }
                RigBuilder.Build();

            }
        }
        else if (hasObjectInHand)
        {
            RightHandDrop();
        }
    }

    void LeftHandPickup()
    {        
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            if (!hasObjectInLeftHand && hit.transform.gameObject.layer == 7)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                SetObjectInHandServer(hit.transform.gameObject, pickupPositionLeftHand[hit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[hit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);
                objInLeftHand = hit.transform.gameObject;
                hasObjectInLeftHand = true;

                SetLeftIKTarget(hit.transform.GetComponent<ItemBehaviour>().gripLeft);
                RigBuilder.Build();

            }
            else if (hasObjectInLeftHand)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                LeftHandDrop();
                SetObjectInHandServer(hit.transform.gameObject, pickupPositionLeftHand[hit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[hit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);
                objInLeftHand = hit.transform.gameObject;
                hasObjectInLeftHand = true;

                SetLeftIKTarget(hit.transform.GetComponent<ItemBehaviour>().gripLeft);
                RigBuilder.Build();

            }
        }
        else if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out RaycastHit spherehit, currentHitDistance, interactionLayer))
        {
            if (!hasObjectInLeftHand && spherehit.transform.gameObject.layer == 7)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                SetObjectInHandServer(spherehit.transform.gameObject, pickupPositionLeftHand[spherehit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[spherehit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);
                objInLeftHand = spherehit.transform.gameObject;
                hasObjectInLeftHand = true;

                SetLeftIKTarget(spherehit.transform.GetComponent<ItemBehaviour>().gripLeft);
                RigBuilder.Build();

            }
            else if (hasObjectInLeftHand)
            {
                SoundManager.Instance.PlaySound(pickupClip);
                LeftHandDrop();
                SetObjectInHandServer(spherehit.transform.gameObject, pickupPositionLeftHand[spherehit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[spherehit.transform.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);
                objInLeftHand = spherehit.transform.gameObject;
                hasObjectInLeftHand = true;

                SetLeftIKTarget(spherehit.transform.GetComponent<ItemBehaviour>().gripLeft);
                RigBuilder.Build();

            }
        }
        else if (hasObjectInLeftHand)
        {
            LeftHandDrop();
        }
    }

    public void RightHandDrop()
    {
        if (!hasObjectInHand || currentEnvironmentInteractable != null)
            return;

        SoundManager.Instance.PlaySound(dropClip);
        objInHand.GetComponent<ItemBehaviour>().StickOnGround();
        pauseManager.ChangeAmmoText("---", "", true);
        DropObjectServer(objInHand, true);
        hasObjectInHand = false;
        if (objInHand.GetComponent<Weapon>().requireBothHands) {
            SetLeftIKTarget(LeftIdlePosition);
        }
        SetRightIKTarget(RightIdlePosition);
        objInHand = null;
        RigBuilder.Build();

        if (hasObjectInLeftHand && currentInteractable == null) SwitchWeapons();
    }

    public void LeftHandDrop()
    {
        if (!hasObjectInLeftHand || currentEnvironmentInteractable != null)
            return;

        SoundManager.Instance.PlaySound(dropClip);
        objInLeftHand.GetComponent<ItemBehaviour>().StickOnGround();
        pauseManager.ChangeAmmoText("---", "", false);
        DropObjectServer(objInLeftHand, false);
        hasObjectInLeftHand = false;
        SetLeftIKTarget(LeftIdlePosition);
        objInLeftHand = null;
        RigBuilder.Build();
        
    }

    public void RightHandFix()
    {
        if (!hasObjectInHand) return;

        if (objInHand == null) {
            hasObjectInHand = false;
            SetRightIKTarget(RightIdlePosition);
            return;
        }

        if (objInHand.layer == 7 || objInHand.layer == 9) {
            RightHandDrop();
            SoundManager.Instance.PlaySound(dropClip);
            pauseManager.ChangeAmmoText("---", "", true);
            hasObjectInHand = false;
            if (objInLeftHand == null) {
                SetLeftIKTarget(LeftIdlePosition);
            }
            SetRightIKTarget(RightIdlePosition);
            objInHand = null;
            RigBuilder.Build();
        }
    }

    public void LeftHandFix()
    {
        if (!hasObjectInLeftHand) return;

        if (objInLeftHand == null) {
            hasObjectInLeftHand = false;
            SetLeftIKTarget(LeftIdlePosition);
            return;
        }

        if (objInLeftHand.layer == 7 || objInLeftHand.layer == 9) {
            LeftHandDrop();
            SoundManager.Instance.PlaySound(dropClip);
            pauseManager.ChangeAmmoText("---", "", false);
            hasObjectInLeftHand = false;
            SetLeftIKTarget(LeftIdlePosition);
            objInLeftHand = null;
            RigBuilder.Build();
        }
    }

    public void HandsReconstruct()
    {
        SetLeftIKTarget(LeftIdlePosition);
        SetRightIKTarget(RightIdlePosition);

        if (objInHand != null && objInHand.GetComponent<Weapon>().requireBothHands) 
        {
            SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
            SetLeftIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
        }
        else if (objInHand != null)
        {
            SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripRight);
        }

        if (objInLeftHand != null) {
            SetRightIKTarget(objInHand.GetComponent<ItemBehaviour>().gripLeft);
        }

        RigBuilder.Build();
    }

    private IEnumerator BuildOnDrop()
    {
        yield return new WaitForSeconds(0.1f);
        
    }

    [ServerRpc]
    private void GiveOwnerToObj(GameObject obj){
        obj.transform.GetComponent<NetworkObject>().GiveOwnership(base.Owner);
    }

    private void HandleInteraction(InputAction.CallbackContext ctx)
    {
        if (pauseManager.pause || pauseManager.chatting || pauseManager.startRound || interactTimer > 0 || !playerController.canMove) return;

        interactTimer = 0.1f;

        bool canInteract = true;

        PlayerGrab();

        if (currentInteractable != null)
            if (currentInteractable.TryGetComponent(out Weapon weapon))
                if (weapon.currentAmmo <= 0 || weapon.cantTakeSafeBool)
                    canInteract = false;
                else canInteract = true;

        if (!canInteract) return;

        if (currentInteractable)
            if (!currentInteractable.canTake) return;

        if (currentInteractable != null && interactionSphere) {
            currentInteractable.OnInteract();
        }

        if (currentEnvironmentInteractable != null) {
            currentEnvironmentInteractable.GetComponent<NetworkObject>().RemoveOwnership();
            GiveOwnerToObj(currentEnvironmentInteractable.gameObject);
            currentEnvironmentInteractable.OnInteract(transform);
        }

        if (hasObjectInHand)
            if (objInHand.GetComponent<DualLauncher>() != null) 
                if (objInHand.GetComponent<DualLauncher>().grenadeOpen)
                    return; 

        if (hasObjectInLeftHand)
            if (objInLeftHand.GetComponent<DualLauncher>() != null) 
                if (objInLeftHand.GetComponent<DualLauncher>().grenadeOpen)
                    return; 

        if (cam != null && dropTimer < 0)
        {
            dropTimer = 0.1f;
            if (currentInteractable != null && interactionSphere && !hasObjectInHand)
            {
                RightHandPickup();
                
                if (hasObjectInLeftHand)
                {
                    if (currentInteractable.GetComponent<Weapon>().requireBothHands)
                    {
                        LeftHandDrop();
                    }
                }
                
            }
            else if (currentInteractable != null && interactionSphere && hasObjectInHand)
            {
                if (objInHand.GetComponent<Weapon>().requireBothHands)
                {
                    RightHandPickup();
                    if (currentInteractable.TryGetComponent(out Weapon weapon3)) {
                    }
                }
                else if (currentInteractable.TryGetComponent(out Weapon weapon) && hasObjectInHand && hasObjectInLeftHand)
                {
                    if (weapon.requireBothHands)
                    {
                        RightHandDrop();
                        LeftHandDrop();

                        RightHandPickup();
                    }
                    else if (objInHand.GetComponent<Weapon>().currentAmmo <= 0)
                    {
                        RightHandPickup();
                    }
                    else {
                        LeftHandPickup();
                    }
                }
                else if (currentInteractable.TryGetComponent(out Weapon weapon2))
                {
                    
                    if (weapon2.requireBothHands)
                    {
                        RightHandPickup();
                    }
                    else if (objInHand.GetComponent<Weapon>().currentAmmo <= 0)
                    {
                        RightHandPickup();
                    }
                    else if (hasObjectInLeftHand)
                    {
                        if (objInLeftHand.GetComponent<Weapon>().currentAmmo <= 0)
                        {
                            LeftHandPickup();
                        }
                    }
                    else {
                        LeftHandPickup();
                    }
                }
                else {
                    LeftHandPickup();
                }
            }
            else if (currentInteractable == null && hasObjectInLeftHand)
            {
                if (hasObjectInHand) 
                {
                    if (objInLeftHand.GetComponent<Weapon>().currentAmmo <= 0)
                    {
                        LeftHandPickup();
                    }
                    else if (objInHand.GetComponent<Weapon>().currentAmmo <= 0)
                    {
                        RightHandPickup();
                    }
                    else {
                        LeftHandPickup();
                    }
                }
                else {
                    LeftHandPickup();
                }
            }
            else if (currentInteractable == null && hasObjectInHand)
            {
                RightHandPickup();
            }
            
        }
    }

    private void HandleDrop(InputAction.CallbackContext ctx)
    {
        SwitchWeapons();

    }

    private void SwitchWeapons()
    {
        if (pauseManager.pause || pauseManager.chatting) return;

        if (cam != null)
        {
            if (dropTimer > 0) return;

            if (hasObjectInHand && hasObjectInLeftHand)
            {
                dropTimer = 0.1f;
                if (objInHand.GetComponent<Weapon>().requireBothHands) return;

                SoundManager.Instance.PlaySound(switchWeaponsClip);
                
                var obj1 = objInHand;
                var obj2 = objInLeftHand;

                DropObjectServer(objInHand, true);
                DropObjectServer(objInLeftHand, false);

                SetObjectInHandServer(obj1, pickupPositionLeftHand[obj1.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[obj1.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);
                SetObjectInHandServer(obj2, pickupPositionRightHand[obj2.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[obj2.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                SetRightIKTarget(obj2.GetComponent<ItemBehaviour>().gripRight);
                SetLeftIKTarget(obj1.GetComponent<ItemBehaviour>().gripLeft);

                objInHand = obj2;
                objInLeftHand = obj1;

                RigBuilder.Build();
            }
            else if (hasObjectInLeftHand)
            {
                dropTimer = 0.1f;

                SoundManager.Instance.PlaySound(switchWeaponsClip);

                pauseManager.ChangeAmmoText("---", "", false);
                
                var obj = objInLeftHand;

                DropObjectServer(objInLeftHand, false);

                SetObjectInHandServer(obj, pickupPositionRightHand[obj.GetComponent<ItemBehaviour>().camChildIndex].position, pickupPositionRightHand[obj.GetComponent<ItemBehaviour>().camChildIndex].rotation, cam.gameObject, true);

                SetRightIKTarget(obj.GetComponent<ItemBehaviour>().gripRight);
                SetLeftIKTarget(LeftIdlePosition);

                objInHand = obj;
                objInLeftHand = null;
                hasObjectInLeftHand = false;
                hasObjectInHand = true;

                RigBuilder.Build();
            }
            else if (hasObjectInHand)
            {
                dropTimer = 0.1f;

                if (weaponInHand.requireBothHands) return;

                SoundManager.Instance.PlaySound(switchWeaponsClip);

                pauseManager.ChangeAmmoText("---", "", true);
                
                var obj = objInHand;

                DropObjectServer(objInHand, true);

                SetObjectInHandServer(obj, pickupPositionLeftHand[obj.GetComponent<ItemBehaviour>().camChildIndexLeftHand].position, pickupPositionLeftHand[obj.GetComponent<ItemBehaviour>().camChildIndexLeftHand].rotation, cam.gameObject, false);

                SetRightIKTarget(RightIdlePosition);
                SetLeftIKTarget(obj.GetComponent<ItemBehaviour>().gripLeft);

                objInHand = null;
                objInLeftHand = obj;
                hasObjectInLeftHand = true;
                hasObjectInHand = false;

                RigBuilder.Build();
            }
        }
    }

    [ServerRpc (RunLocally = true)]
    void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject player, bool rightHand)
    {
        SetObjectInHandObserver(obj, position, rotation, player, rightHand);
        obj.GetComponent<NetworkObject>().GiveOwnership(base.Owner);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player, bool rightHand)
    {
        if (!IsOwner)
        {
            obj.transform.position = pickupPositionOnline[rightHand ? 0 : 1].position;
            obj.transform.rotation = pickupPositionOnline[rightHand ? 0 : 1].rotation;
        }


        if (IsOwner) obj.transform.localScale = new Vector3(2,2,2);
        if (!IsOwner) {
            obj.transform.localScale = new Vector3(1,1,1);
            obj.GetComponent<ItemBehaviour>().KillAnimation();
        }

        if (IsOwner) {
            PauseManager.Instance.MoveAmmoDisplay(true, rightHand);
            if (rightHand) {
                weaponInHand = obj.GetComponent<Weapon>();
                behaviourInHand = obj.GetComponent<ItemBehaviour>();
            }
            else if (!rightHand) {
                weaponInLeftHand = obj.GetComponent<Weapon>();
                behaviourInLeftHand = obj.GetComponent<ItemBehaviour>();
            }
        }

        obj.transform.parent = (obj.GetComponent<Weapon>().requireBothHands ? pickupPositionBothHand[obj.GetComponent<ItemBehaviour>().camChildIndex].transform : rightHand ? pickupPositionRightHand[obj.GetComponent<ItemBehaviour>().camChildIndex].transform : pickupPositionLeftHand[obj.GetComponent<ItemBehaviour>().camChildIndexLeftHand].transform);
        obj.GetComponent<ItemBehaviour>().playerPickup = this;

        if (obj.GetComponent<ItemBehaviour>().rightHandAnim != ""){
            animator.SetBool("TwoHanded", false);
            animator.SetBool("DoubleHanded", false);
            animator.SetBool("RightHanded", false);
            animator.SetBool(rightHand ? obj.GetComponent<ItemBehaviour>().rightHandAnim : obj.GetComponent<ItemBehaviour>().leftHandAnim, true);
        }

        
        obj.GetComponent<ItemBehaviour>().playerController = GetComponent<FirstPersonController>();
        obj.GetComponent<ItemBehaviour>().rootObject = gameObject;
        obj.GetComponent<ItemBehaviour>().OnGrab(IsOwner, rightHand);
        obj.GetComponent<ItemBehaviour>().lastPlayerHolder = gameObject;
        obj.GetComponent<ItemBehaviour>().KillTweens();
        obj.GetComponent<Weapon>().camAnimScript = camAnimScript;
        obj.GetComponent<Weapon>().heldOnce = true;
        obj.GetComponent<Weapon>().playerValues = playerValues;
        obj.GetComponent<ItemBehaviour>().isTaken = true;
        obj.GetComponent<ItemBehaviour>().cam = cam;
        obj.GetComponent<Weapon>().inRightHand = rightHand;
        obj.GetComponent<Weapon>().inLeftHand = !rightHand;
        obj.GetComponent<Weapon>().fpArms = fpArms;
        obj.GetComponent<ItemBehaviour>().SetLayer();
        obj.layer = 8;

        if (obj.GetComponent<ItemBehaviour>().aimWeapon && IsOwner)
            playerController.zoomFOV = obj.GetComponent<ItemBehaviour>().aimFOV + (!Crosshair.Instance.canScopeAim ? (playerController.defaultFOV - 68) : 0);

        
    }

    [ServerRpc (RunLocally = true)]// (RequireOwnership = false)]
    void DropObjectServer(GameObject obj, bool rightHand)
    {
        DropObjectObserver(obj, rightHand);
        //obj.GetComponent<NetworkObject>().RemoveOwnership();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void DropObjectObserver(GameObject obj, bool rightHand)
    {
        if (!IsOwner) obj.GetComponent<ItemBehaviour>().StickOnGroundObservers();

        obj.transform.DOKill();

        if (IsOwner) {
            PauseManager.Instance.MoveAmmoDisplay(false, rightHand);
            playerController.isScopeAiming = false;
            if (rightHand) {
                weaponInHand = null;
                behaviourInHand = null;
            }
            else if (!rightHand) {
                weaponInLeftHand = null;
                behaviourInLeftHand = null;
            }
        }

        if (obj.GetComponent<ItemBehaviour>().rightHandAnim != ""){
            animator.SetBool(rightHand ? obj.GetComponent<ItemBehaviour>().rightHandAnim : obj.GetComponent<ItemBehaviour>().leftHandAnim, false);
        }
        camAnimScript.rotateBack = true;

        obj.GetComponent<ItemBehaviour>().playerPickup = null;
        obj.GetComponent<ItemBehaviour>().playerController = null;
        obj.GetComponent<ItemBehaviour>().rootObject = null;
        obj.GetComponent<ItemBehaviour>().OnDrop(cam);
        obj.GetComponent<Weapon>().camAnimScript = null;
        obj.GetComponent<ItemBehaviour>().cam = null;

        obj.transform.parent = null;
        obj.transform.localScale = new Vector3(2,2,2);
        obj.GetComponent<ItemBehaviour>().UnsetLayer();
        obj.layer = 7;
        RigBuilder.Build();
    }

    private void HandleInteractEnvironment()
    {


        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit frontCast, interactionDistance, environmentInteractionLayer) && currentInteractable == null)
        {
            frontCast.collider.TryGetComponent(out currentEnvironmentInteractable);

            if (!currentEnvironmentObject)
                currentEnvironmentInteractable.OnFocus();

            currentEnvironmentObject = frontCast.collider.gameObject;
        }
        else if (currentEnvironmentInteractable)
        {
            currentEnvironmentInteractable.OnLoseFocus();
            currentEnvironmentInteractable = null;
            currentEnvironmentObject = null;
        }

        if (currentEnvironmentInteractable != null && objInHand == null && Input.GetMouseButtonDown(0)) {
            currentEnvironmentInteractable.GetComponent<NetworkObject>().RemoveOwnership();
            GiveOwnerToObj(currentEnvironmentInteractable.gameObject);
            currentEnvironmentInteractable.OnInteract(transform);
        }

        if (currentEnvironmentInteractable == null)
        {
            PauseManager.Instance.interactPopup.gameObject.SetActive(false);
        }

    }

    private void HandleInteractionCheck()
    {
        //Spherecast size
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit sphereHit2, maxSphereDistance, landLayer))
        {
            currentHitObject = sphereHit2.transform.gameObject;
            currentHitDistance = sphereHit2.distance + sphereRadius;
        }
        else{
            currentHitDistance = maxSphereDistance;
            currentHitObject = null;
        }

        //Raycastsize
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit cast, maxInteractionDistance, landLayer))
        {
            if (cast.transform.gameObject.layer == 7) currentHitObject = cast.transform.gameObject;
            interactionDistance = cast.distance;
        }
        else{
            interactionDistance = maxInteractionDistance;
        }


        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit frontCast3, interactionDistance, interactionLayer))
        {
            interactionSphere = true;

            var lastInteractable = (currentInteractable ? currentInteractable : null);

            frontCast3.collider.TryGetComponent(out currentInteractable);

            if (lastInteractable)
                if (lastInteractable != currentInteractable)
                    lastInteractable.OnLoseFocus();

            currentObject = frontCast3.collider.gameObject;

            if (currentInteractable)
                currentInteractable.OnFocus();
                
        }
        else if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out RaycastHit spherehit2, currentHitDistance, interactionLayer))
        {
            interactionSphere = true;

            var lastInteractable = (currentInteractable ? currentInteractable : null);

            spherehit2.collider.TryGetComponent(out currentInteractable);

            if (lastInteractable)
                if (lastInteractable != currentInteractable)
                    lastInteractable.OnLoseFocus();

            currentObject = spherehit2.collider.gameObject;

            if (currentInteractable)
                currentInteractable.OnFocus();
                
        }
        else if (currentInteractable)
        {
            interactionSphere = false;
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
            currentObject = null;
        }

        if (currentInteractable == null)
        {
            PauseManager.Instance.grabPopup.gameObject.SetActive(false);
            interactionSphere = false;
        }

    }

    void OnDestroy()
    {
        PauseManager.Instance.grabPopup.gameObject.SetActive(false);
        
        PauseManager.Instance.interactPopup.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (cam == null)
            return;
        
        Gizmos.color = Color.red;
        Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * currentHitDistance);
        Gizmos.DrawWireSphere(cam.transform.position + cam.transform.forward * currentHitDistance, sphereRadius);
        Debug.DrawRay(cam.transform.position, cam.transform.forward * interactionDistance, Color.green);
    }

    private PlayerHealth enemyBody;
    private Transform ragdoll;
    Transform hips;
    [SerializeField] private Transform camPoint;

    private void PlayerGrab()
    {
        if (ragdoll) {
            ragdoll.SetParent(null);
            var bodies = ragdoll.GetComponentsInChildren<Rigidbody>();
            foreach(var body in bodies)
            {
                body.isKinematic = false;
                body.AddExplosionForce(150, cam.transform.position - cam.transform.forward, 0, 1f, ForceMode.Impulse);
            }
            hips = null;
            ragdoll = null;
            
        }
        else {

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionDistance, ragdollInteractionLayer))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                {
                    RightHandDrop();
                    LeftHandDrop();
                    ragdoll = hit.transform.root;
                    ragdoll.SetParent(cam.transform);
                    var bodies = ragdoll.GetComponentsInChildren<Rigidbody>();
                    foreach(var body in bodies)
                    {
                        if (body.gameObject.name == "Hips") {
                            hips = body.transform;
                            body.isKinematic = true;
                        }
                    }
                    
                }
            }
        }

        if (enemyBody) {
            SetEnemyParent(false, camPoint, enemyBody);
            BumpPlayerServer(cam.transform.forward, 70, enemyBody);
            
        }
        else {

            if (!playerController.canMove) return;

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionDistance, bodyInteractionLayer))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Body"))
                {
                    enemyBody = hit.transform.GetComponentInParent<PlayerHealth>();
                    if (!enemyBody.controller.canMove) 
                    SetEnemyParent(true, transform, enemyBody);
                    else enemyBody = null;
                }
            }
        }
        
    }

    Transform FindRecursive(string name, Transform root)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        Transform wantedObject = null;
        foreach (var child in children)
        {
            if (child.gameObject.name == name) 
            {
                wantedObject = child;
                break;
            }
        }
        
        return wantedObject;
    }

    private void HandleAboubiGrab()
    {
        if (hips) hips.position = cam.transform.position + cam.transform.forward * 1.5f;
    }


    [ServerRpc]
    private void BumpPlayerServer(Vector3 direction, float force, PlayerHealth ph)
    {
        ph.bounceDirection = direction;
        ph.bounceForce = force;
        ph.shouldBounce = true;
    }

    [ServerRpc]
    private void SetEnemyParent(bool set, Transform t, PlayerHealth ph)
    {
        if (!set) {
            ph.GetComponent<NetworkObject>().UnsetParent();
            ph.controller.canMove = true;
        }
        else ph.GetComponent<NetworkObject>().SetParent(t.GetComponent<NetworkBehaviour>());
        SetEnemyParentObservers(set);

    }

    [ObserversRpc]
    private void SetEnemyParentObservers(bool set)
    {
        if (!set) enemyBody = null;

    }

}
