using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet;
using DG.Tweening;
using TMPro;

public class ItemBehaviour : Interactable
{
    public string weaponName;
    public bool heavy;
    public bool vertical;
    public int camChildIndex;
    public int camChildIndexLeftHand;
    public int aimIndex;
    public bool aimWeapon = true;
    public float aimFOV = 45;

    [Space]
    public bool vfxAttachedOnGun;
    public ParticleSystem smokeTrail;
    [Space]

    public string rightHandAnim;
    public string leftHandAnim;
    public string outofhandsAnim;
    [SerializeField] private Sprite standCrosshair;
    [SerializeField] private Sprite sprintCrosshair;
    [SerializeField] private bool instantAimLens = false;
    [SerializeField] private Sprite aimCrosshair;
    [SerializeField]  float ejectForce = 9.5f;
    [SerializeField] private float torqueForce = 18;
    [SerializeField] private float gravityAdded = 2f;
    [SerializeField] private AudioClip hitSurfaceClip;
    public Transform gripRight;
    public Transform gripLeft;
    private Vector3 finalPos;

    [SerializeField] private AudioClip grabClip;
    public GameObject depopVFX;

    [SerializeField] private LayerMask groundLayer;
    
    public Camera cam;

    public bool isTaken;

    private Tween groundMov;
    private Tween tween1;
    private Tween tween2;
    private Tween tween3;
    private Tween tween4;


    public GameObject rootObject;
    public GameObject lastPlayerHolder;
    public Transform[] fpArms = new Transform[2];
    public PlayerPickup playerPickup;
    public FirstPersonController playerController;
    private Weapon weaponScript;
    private MeshRenderer[] hoveredObjectRenderer = new MeshRenderer[2];
    private List<Material> hoveredObjectMat = new List<Material>();

    private AudioSource audio;
    private Rigidbody tempRb;
    public bool dispenserStart;

    [Header ("AimStrafe Lean")]
    [SerializeField] private Transform aimStrafePivot;
    [SerializeField] private float maxPivot = 3;
    [SerializeField] private float aimStrafeLeanSpeed = 10;


    public override void OnFocus()
    {
        PauseManager.Instance.grabPopup.gameObject.SetActive(true);
        PauseManager.Instance.grabPopup.text = weaponName.ToLower() + " [" + PauseManager.Instance.InteractPromptLetter.ToLower() + "]";

        foreach (var mat in hoveredObjectMat)
        {
            mat.SetFloat("_OutlineWidth", 0.007f);
        }
    }


    public override void OnInteract()
    {
        dispenserStart = false;
        transform.DOKill();
    }


    public override void OnLoseFocus()
    {
        PauseManager.Instance.grabPopup.gameObject.SetActive(false);
        foreach (var mat in hoveredObjectMat)
        {
            mat.SetFloat("_OutlineWidth", 0f);
        }
    }

    public void OnGrab(bool owner, bool rightHand)
    {
        if (GetComponent<Rigidbody>() != null)
            Destroy(GetComponent<Rigidbody>());

        if (!owner || !rightHand) return;

        #if UNITY_EDITOR
        Debug.Log($"ItemBehaviour: OnGrab called on {transform.name}", transform);
        #endif
        Crosshair.Instance.standCrosshair = standCrosshair;
        Crosshair.Instance.sprintCrosshair = sprintCrosshair;
        Crosshair.Instance.instantAimLens = instantAimLens;
        if (aimWeapon && aimCrosshair != null) {
            Crosshair.Instance.aimCrosshair = aimCrosshair;
            Crosshair.Instance.canScopeAim = true;
        }
        else Crosshair.Instance.canScopeAim = false;

    }

    public void OnDrop(Camera tempCam)
    {
        weaponScript.isClicked = false;
        dispenserStart = false;
        gameObject.AddComponent<Rigidbody>();
        tempRb = GetComponent<Rigidbody>();
        tempRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tempRb.drag = 0;
        tempRb.interpolation = RigidbodyInterpolation.Interpolate;
        tempRb.AddForce(tempCam.transform.forward * ejectForce, ForceMode.Impulse);
        var randomInt = Random.Range(-1, 1);
        tempRb.AddTorque(tempCam.transform.forward * torqueForce + transform.right * torqueForce, ForceMode.Impulse);
    }

    [ObserversRpc (RunLocally = true)]
    public void DispenserDrop(Vector3 dir)
    {
        dispenserStart = true;
        gameObject.AddComponent<Rigidbody>();
        tempRb = GetComponent<Rigidbody>();
        tempRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tempRb.drag = 0;
        tempRb.interpolation = RigidbodyInterpolation.Interpolate;
        tempRb.AddForce(dir * ejectForce/2, ForceMode.Impulse);
        tempRb.AddTorque(dir * torqueForce + transform.right * torqueForce, ForceMode.Impulse);
        var randomInt = Random.Range(-1, 1);
    }

    void Start()
    {
        maxPivot = -maxPivot;
        weaponScript = GetComponent<Weapon>();
        transform.localScale = new Vector3(2,2,2);
        audio = GetComponent<AudioSource>();
        initialLocalPosition = transform.localPosition;

        hoveredObjectRenderer = GetComponentsInChildren<MeshRenderer>();
        hoveredObjectMat.Clear();
        for (int i=0; i < hoveredObjectRenderer.Length; i++)
        {
            foreach (var mat in hoveredObjectRenderer[i].materials)
            {
                hoveredObjectMat.Add(mat);
            }
        }

        col = GetComponent<Collider>();

            gripRight = GetComponentsInChildren<Grip>()[0].transform;
            gripLeft = GetComponentsInChildren<Grip>()[1].transform;

        if (!dispenserStart && gameObject.name!="Pig Held Item") groundMov = transform.DOLocalMove(transform.localPosition + transform.parent.up/2, 1.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (GetComponentInChildren<AimStrafePivot>() != null) aimStrafePivot = GetComponentInChildren<AimStrafePivot>().transform;

        
    }

    private void Update()
    { 
        if (gameObject.name=="Pig Held Item") return;
        if (rootObject == null) col.enabled = true;
        else col.enabled = false;

        if (dispenserStart) transform.DOKill();

        if (rootObject == null) {
            if (tempRb == null && !dispenserStart) transform.Rotate(0, 20 * Time.deltaTime, 0);
            return;
        }
        else if (rootObject.layer == 3) {
            transform.position = (weaponScript.inRightHand ? playerPickup.pickupPositionOnline[0].position : playerPickup.pickupPositionOnline[1].position);
            transform.rotation = (weaponScript.inRightHand ? playerPickup.pickupPositionOnline[0].rotation : playerPickup.pickupPositionOnline[1].rotation);
            if (aimStrafePivot) aimStrafePivot.localRotation = Quaternion.Slerp(aimStrafePivot.localRotation, Quaternion.Euler(aimStrafePivot.localRotation.x, aimStrafePivot.localRotation.y, 0), aimStrafeLeanSpeed * Time.deltaTime);
            //transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, )
            return;
        }
        else if (weaponScript.inRightHand)
        {
            CalculateMovementInput();

            if (aimCrosshair != null && playerController.isAiming && !weaponScript.isReloading) {
                playerController.isScopeAiming = true;

                if (aimWeapon && aimCrosshair != null) {
                    Crosshair.Instance.canScopeAim = true;
                }
                else Crosshair.Instance.canScopeAim = false;
            }
            else {
                playerController.isScopeAiming = false;

                if (aimWeapon && aimCrosshair != null) {
                    Crosshair.Instance.canScopeAim = false;
                }
            }

            if (playerController.isAiming && aimWeapon && !weaponScript.isReloading)
            {
                //WeaponTransform(weaponScript.IsOwner ? (weaponScript.requireBothHands ? playerPickup.aimPositionBothHand[aimIndex]  : playerPickup.aimPositionRightHand[aimIndex]) : playerPickup.pickupPositionOnline[0]);
                WeaponTransform(weaponScript.requireBothHands ? playerPickup.aimPositionBothHand[aimIndex] : playerPickup.aimPositionRightHand[aimIndex]);
                if (aimStrafePivot) aimStrafePivot.localRotation = Quaternion.Slerp(aimStrafePivot.localRotation, Quaternion.Euler(aimStrafePivot.localRotation.x, aimStrafePivot.localRotation.y, (currentInputRaw.x != 0 ? aimStrafePivot.localRotation.z + (currentInputRaw.x * maxPivot) : 0)), aimStrafeLeanSpeed * Time.deltaTime);

            }
            else if (weaponScript.requireBothHands)
            {
                //WeaponTransform(weaponScript.IsOwner ? playerPickup.pickupPositionBothHand[camChildIndex] : playerPickup.pickupPositionOnline[0]);
                finalPos = playerPickup.pickupPositionBothHand[camChildIndex].position;
                WeaponTransform(playerPickup.pickupPositionBothHand[camChildIndex]);
            }
            else
            {
                //WeaponTransform(weaponScript.IsOwner ? playerPickup.pickupPositionRightHand[camChildIndex] : playerPickup.pickupPositionOnline[0]);
                finalPos = playerPickup.pickupPositionRightHand[camChildIndex].position;
                WeaponTransform(playerPickup.pickupPositionRightHand[camChildIndex]);
                if (aimStrafePivot) aimStrafePivot.localRotation = Quaternion.Slerp(aimStrafePivot.localRotation, Quaternion.Euler(aimStrafePivot.localRotation.x, aimStrafePivot.localRotation.y, 0), aimStrafeLeanSpeed * Time.deltaTime);
            }

            

        }
        else if (weaponScript.inLeftHand)
        {
            CalculateMovementInput();
            //WeaponTransform(weaponScript.IsOwner ? playerPickup.pickupPositionLeftHand[camChildIndex] : playerPickup.pickupPositionOnline[1]);
            
            WeaponTransform(playerPickup.pickupPositionLeftHand[camChildIndexLeftHand]);

            finalPos = playerPickup.pickupPositionLeftHand[camChildIndexLeftHand].position;
        }

        


    }

    public void InstantComeBackOnFire()
    {
        if (weaponScript.inRightHand)
        {
            if (playerController.isAiming && aimWeapon)
            {
                transform.position = weaponScript.requireBothHands ? playerPickup.aimPositionBothHand[aimIndex].position : playerPickup.aimPositionRightHand[aimIndex].position;
                transform.localRotation = weaponScript.requireBothHands ? playerPickup.aimPositionBothHand[aimIndex].localRotation : playerPickup.aimPositionRightHand[aimIndex].localRotation;
                Physics.SyncTransforms();
            }
            else if (weaponScript.requireBothHands)
            {
                transform.position = playerPickup.pickupPositionBothHand[camChildIndex].position;
                transform.localRotation = playerPickup.pickupPositionBothHand[camChildIndex].localRotation;
                Physics.SyncTransforms();
            }
            else
            {
                transform.position = playerPickup.pickupPositionRightHand[camChildIndex].position;
                transform.localRotation = playerPickup.pickupPositionRightHand[camChildIndex].localRotation;
                Physics.SyncTransforms();
            }
        }
        else if (weaponScript.inLeftHand)
        {
            transform.position = playerPickup.pickupPositionLeftHand[camChildIndexLeftHand].position;
            transform.localRotation = playerPickup.pickupPositionLeftHand[camChildIndexLeftHand].localRotation;
            Physics.SyncTransforms();
        }
    }

    void FixedUpdate()
    {
        //if (tempRb != null) tempRb.AddForce(gravityAdded * Vector3.down, ForceMode.Acceleration);
    }

    public void KillAnimation()
    {
        if (transform != null)
            transform.DOKill();
    }


    private void WeaponTransform(Transform pos)
    {
        TiltSway(pos.localRotation);
        WeaponSway(pos.position);
    }

    [Header("Position")]
    public float amount = 0.02f;
    public float maxAmount = 0.06f;
    public float smoothAmount = 6f;


    [Header("Rotation")]
    public float rotationAmount = 4f;
    public float maxRotationAmount = 5f;
    public float smoothRotation = 12f;


    [Space]
    public bool rotationX = true;
    public bool rotationY = true;
    public bool rotationZ = true;


    [Header("Headbob")]
    [SerializeField] public float walkBobSpeed = 14f;
    [SerializeField] public float walkBobAmount = 0.02f;
    [SerializeField] public float sprintBobSpeed = 18f;
    [SerializeField] public float sprintBobAmount = 0.05f;
    [SerializeField] public float crouchBobSpeed = 8f;
    [SerializeField] public float crouchBobAmount = 0.008f;


    private float InputX;
    private float InputY;


    private Vector2 currentInputRaw;
    private float verticalInputRaw;
    private float horizontalInputRaw;


    private float movTimer;
    private Collider col;
    bool alreadyPlayed;

    [Header("Wall Clip Fix")]
    [SerializeField] private float distance = 1;
    [SerializeField] private float radius = 0.125f;
    [SerializeField] private LayerMask clippingLayerMask;
    [SerializeField] private AnimationCurve offsetCurve;
    Vector3 initialLocalPosition;


    private void CalculateMovementInput()
    {
        if (playerController != null) 
            currentInputRaw = playerController.currentInputRaw;

        alreadyPlayed = false;


    }


    private void ItemMovement(Vector3 initialPosition)
    {
        if (currentInputRaw != Vector2.zero) movTimer += Time.deltaTime * ((Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) ? crouchBobSpeed : Input.GetKey(KeyCode.LeftShift) ? sprintBobSpeed : walkBobSpeed);
        else movTimer = 0;
        
        var defaultYPos = initialPosition.y;
        if (currentInputRaw != Vector2.zero) {
            transform.position = new Vector3(initialPosition.x, defaultYPos + Mathf.Sin(movTimer) * ((Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) ? crouchBobAmount : Input.GetKey(KeyCode.LeftShift) ? sprintBobAmount : walkBobAmount), initialPosition.z);
        }
        else {
            transform.position = initialPosition;
        }
    }


    private void WeaponSway(Vector3 initialPosition)
    {
        if (Physics.SphereCast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), radius, out RaycastHit hit, distance, clippingLayerMask))
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition - cam.transform.forward * offsetCurve.Evaluate(hit.distance / distance), Time.deltaTime * smoothAmount);
        }
        else {


        InputX = -Input.GetAxis("Mouse X");
        InputY = -Input.GetAxis("Mouse Y");


        float moveX = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
        float moveY = Mathf.Clamp(InputY * amount, -maxAmount, maxAmount);
        
        Vector3 finalPosition = initialPosition + new Vector3(moveX, moveY, 0);


        transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * smoothAmount);
        //transform.position = initialPosition;
        //transform.localPosition = localPos;
        }
        
    }


    private void TiltSway(Quaternion initialRotation)
    {
        float tiltY = -Mathf.Clamp(Input.GetAxis("Mouse X") * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = -Mathf.Clamp(-Input.GetAxis("Mouse Y") * rotationAmount, -maxRotationAmount, maxRotationAmount);


        Quaternion finalRotation = initialRotation * Quaternion.Euler(new Vector3(rotationX ? tiltX : 0f, rotationY ? tiltY : 0f, rotationZ ? tiltY : 0f));


        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothRotation);
    }




    public void StickOnGround()
    {
        if (rootObject == null) return;


        Collider[] hitColliders = Physics.OverlapSphere(finalPos, 0.7f, groundLayer);
        Collider[] hitCollidersFront = Physics.OverlapSphere(rootObject.transform.position + Vector3.up + rootObject.transform.forward, 0.7f, groundLayer);


        if (hitColliders.Length != 0 || hitCollidersFront.Length != 0)
        {


            if (Physics.Raycast(rootObject.transform.position, -Vector3.up, out RaycastHit playerHit, Mathf.Infinity, groundLayer)){
                tween1 = transform.DOMove(playerHit.point + new Vector3(0, 0.5f, 0), 0.5f).SetEase(Ease.OutQuart);
                tween2 = transform.DORotate(new Vector3(playerHit.normal.x, rootObject.transform.eulerAngles.y, playerHit.normal.z), 0.5f);
                //transform.position = playerHit.point + new Vector3(0, 0.5f, 0);
                //transform.rotation = Quaternion.LookRotation(playerHit.normal);
            }
                
        }
        else {
            Quaternion.Euler(transform.rotation.x, transform.eulerAngles.y - 90, transform.rotation.z);
            transform.position = cam.transform.position + cam.transform.forward;
        }
        /*else if (Physics.Raycast(rootObject.transform.position, -Vector3.up, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            tween3 = transform.DORotate(new Vector3(hit.normal.x, rootObject.transform.eulerAngles.y, hit.normal.z), 0.5f);
            tween4 = transform.DOMove(hit.point + new Vector3(0, 0.5f, 0) + rootObject.transform.forward, 0.5f).SetEase(Ease.OutQuart);
            //transform.position = hit.point + new Vector3(0, 0.5f, 0);
            //transform.rotation = Quaternion.LookRotation(hit.normal);
        }*/
        
    }

    public void StickOnGroundObservers()
    {
        if (rootObject == null) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(finalPos, 0.7f, groundLayer);
        Collider[] hitCollidersFront = Physics.OverlapSphere(rootObject.transform.position + Vector3.up + rootObject.transform.forward, 0.7f, groundLayer);

        if (hitColliders.Length != 0 || hitCollidersFront.Length != 0)
        {

            if (Physics.Raycast(rootObject.transform.position, -Vector3.up, out RaycastHit playerHit, Mathf.Infinity, groundLayer)){
                transform.position = playerHit.point + new Vector3(0, 0.5f, 0);
                transform.rotation = Quaternion.Euler(playerHit.normal.x, rootObject.transform.eulerAngles.y, playerHit.normal.z);
            }
                
        }
        else {
            Quaternion.Euler(transform.rotation.x, transform.eulerAngles.y - 90, transform.rotation.z);
            transform.position = cam.transform.position + cam.transform.forward;
        }
        /*else if (Physics.Raycast(rootObject.transform.position, -Vector3.up, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            transform.position = hit.point + new Vector3(0, 0.5f, 0);
            transform.rotation = Quaternion.Euler(hit.normal.x, rootObject.transform.eulerAngles.y, hit.normal.z);
        }
        return;*/
    }

    public void GroundMovement()
    {
        /*if (tween1 != null)
            if (tween1.IsPlaying())
                groundMov = transform.DOMove(transform.position + Vector3.up/2, 1.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        else if (tween3 != null)
            if (tween3.IsPlaying())
                groundMov = transform.DOMove(transform.position + Vector3.up/2, 1.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);*/
    }


    void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            if (child.tag == "vfx") return;
            child.gameObject.layer = layer;
        }
    }

    public void KillTweens()
    {
#if UNITY_EDITOR
        Debug.Log($"Item Behaviour: Killing Tweens on {transform.name}", transform);
#endif
        if (transform != null) {
            transform.DOPause();
            transform.DOKill();
        }
    }


    public void SetLayer()
    {
        SetLayerAllChildren(transform, 8);
    }


    public void UnsetLayer()
    {
        SetLayerAllChildren(transform, 0);
    }


    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (tempRb && !alreadyPlayed)
        {
            alreadyPlayed = true;

            if (tempRb.velocity.magnitude > 0.5f && audio != null)
            {
                audio.spatialBlend = 1f;
                audio.PlayOneShot(hitSurfaceClip, Mathf.Clamp(tempRb.velocity.magnitude, 1.3f, 5)/5);
            }
            
        }
    }
}