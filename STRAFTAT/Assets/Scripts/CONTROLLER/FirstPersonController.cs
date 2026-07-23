using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputSystem = UnityEngine.InputSystem;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Transporting;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using LambdaTheDev.NetworkAudioSync;
using FishNet.Component.Animating;
using HeathenEngineering.SteamworksIntegration;
using FishNet.Object.Synchronizing;
using HeathenEngineering.PhysKit;
using TMPro;

public class FirstPersonController : NetworkBehaviour
{
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool canMove = false;
    public bool isSprinting;
    public bool isWalking;
    public bool isCrouching;
    public bool isGrounded;
    public bool safeGrounded;
    public bool isSliding;
    public bool isLeaning;
    public bool isTouchingAnything;

    public PlayerControls playerControls;
    [HideInInspector] public InputSystem.InputAction move, jump, run, lookY, lookX, zoom, crouch, moveUp, leanLeft, leanRight, record;
    [HideInInspector] public InputSystem.InputAction fire1, fire2, reload;
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadMovement = true;
    [SerializeField] private bool WillSlideOnSlopes = true;
    [SerializeField] private bool useFootsteps = true;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float crouchSpeed = 5f;
    [SerializeField] private float slopeSpeed = 8f;
    [SerializeField] private float airControl = 1f;
    [SerializeField] private float airSpeed = 10f;
    [SerializeField] private float sprintAirSpeed = 14f;

    [SerializeField] private float globalAcceleration = 15f;
    [SerializeField] private float globalDeceleration = 15f;
    [SerializeField] private float walkAcceleration = 15f;
    [SerializeField] private float sprintAcceleration = 15f;
    [SerializeField] private float crouchAcceleration = 15f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float sprintAirAcceleration = 15f;
    public float movementFactor = 1, jumpFactor=1, wallJumpFactor=1;
    public int maxWallJumps=1;
    public Vector3 moveDirection;
    private Vector3 objectCollisionMoveDirection;
    public Vector3 moveAdded;
    public Vector3 groundNormal;
    public float playerSpeed, speedFactor = 1;
    public Vector2 currentInput, currentInputRaw;
    private float verticalInput, horizontalInput, verticalInputRaw, horizontalInputRaw;

    [Header("Look Parameters")]
    [SerializeField, Range(0.1f, 5)] public float lookSpeedX = 2.0f;
    [SerializeField, Range(0.1f, 5)] public float lookSpeedY = 2.0f;
    [SerializeField, Range(0.1f, 5)] public float lookSpeedAim = 0.8f;
    [SerializeField, Range(0.1f, 5)] public float lookSpeedAimNoScope = 2;
    public bool isScopeAiming;
    [SerializeField, Range(0, 90)] private float upperLookLimit = 90f;
    [SerializeField, Range(0, 90)] private float lowerLookLimit = 90f;
    [SerializeField] private Vector3 offset = new Vector3(0,-0.25f,0);
    public float rotationX = 0;
    private float rotationZ = 0;
    Vector2 mouseInput;

    [Space]
    public float killShockWaveStrength = 10;
    [SerializeField] private float saturationSpeed = 20;

    [Header("Lean Parameters")]
    [SerializeField] private Transform leanCamera;
    [SerializeField] private float leanSpeed = 8;
    [SerializeField] private float leanLimit = 30;
    [SerializeField] private float constrainSphereRadius = 0.5f;
    float currentRot;

    [Header("Jumping Parameters")]
    public float jumpForce = 8.0f;
    [SerializeField] private float jumpSlopeForce = 8.0f;
    [SerializeField] private float jumpSlopeDecel = 3.0f;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float crouchGravity = 40.0f;
    [SerializeField] private float jumpGravity = 20.0f;
    
    [HideInInspector] public float gravityMultiplier = 1f;
    
    [SerializeField] private float ladderDetectionLength = 0.5f;
    [SerializeField] private float ladderSpeed = 2f;
    [SerializeField] private float vaultMinimumAngle = 80f;
    [SerializeField] private LayerMask ladderLayer;
    public LayerMask landLayer;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip wallOutClip;
    [SerializeField] private AudioClip teleportClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip ceilingHitClip;
    [SerializeField] private AudioClip groundSlideClip;
    [SerializeField] private AudioClip wallSlideClip;
    [SerializeField] private GameObject jumpDust;
    [SerializeField] private GameObject landDust;
    private bool prejump;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float slideHeight = 0.8f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchingSpeed = 0.3f;
    [SerializeField] private Ease crouchingEase = Ease.Linear;

    [Header("Slide Parameters")]
    [SerializeField] private float walkSlideImpulsion = 2;
    [SerializeField] private float walkSlideDuration = 20;
    [SerializeField] private float sprintSlideImpulsion = 2;
    [SerializeField] private float sprintSlideDuration = 30;
    [SerializeField] private float slideResetTime = 1.5f;
    [SerializeField] private GameObject slideDust;
    [SerializeField] private GameObject legRight;
    [SerializeField] private GameObject legLeft;
    [SerializeField] private GameObject torso;
    bool walkSlide, sprintSlide;

    [Header("Head Movement")]
    [SerializeField] private Transform landBobPivot;
    [SerializeField] private float landRecoverSpeed;
    [SerializeField] private float landBobAmount;
    [SerializeField] private float landBobDuration;
    [SerializeField] private Ease landBobEase;
    
    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField] private float fallBobSpeed = 8f;
    [SerializeField] private float fallBobUpSpeed = 2f;
    [SerializeField] private float fallBobAmount = 0.025f;
    [SerializeField] private float fallShakeHeight = 2f;
    [SerializeField] private Vector3 fallShakeForce = new Vector3(-1f, 0.4f, -1f);
    [SerializeField] private float fallShakeDuration = 0.2f;
    [SerializeField] private int fallShakeVibrato = 2;
    [SerializeField] private float fallShakeElasticity = 1f;

    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private Vector3 highFallShakeForce = new Vector3(0, 0, -21);
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float highFallShakeDuration = 0.15f;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float highFallShakeRecoverTime = 1f;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float highFallShakeRecoverSpeed = 3f;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float highFallShakeHeight = 7f;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private AudioClip highFallAudioClip;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private GameObject highFallVfx;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float fallAudioLerpSpeed;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float fallAudioLerpOutSpeed;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float lensDistortionWhenFalling;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float lensDistortionLerpSpeed;
    [SerializeField] [BoxGroup("High fall parameters", centerLabel: true)] private float lensDistortionLerpOutSpeed;
    private ParticleSystem highFallVfxComponent;
    private float defaultYPos = 0;
    
    [Header("Camera Controller")]
    [SerializeField] private float zoomSpeed = 0.3f;
    [SerializeField] public float zoomFOV = 30f;
    [SerializeField] private Ease zoomEase = Ease.Linear;
    [SerializeField] private float runEaseSpeed = 0.3f;
    [SerializeField] private float runFOV = 90f;
    [SerializeField] private Ease slideEase = Ease.Linear;
    [SerializeField] private float slideEaseSpeed = 0.3f;
    [SerializeField] private float slideFOV = 80f;
    [SerializeField] private Ease runSlideEase = Ease.Linear;
    [SerializeField] private float runSlideEaseSpeed = 0.3f;
    [SerializeField] private float runSlideFOV = 94f;
    [SerializeField] private Ease runEase = Ease.Linear;
    [SerializeField] private float tiltAmount = 7f;
    [SerializeField] private float tiltSpeed = 7f;
    [SerializeField] private float slideTiltAmount = 15f;
    public bool camController = true;
    public bool isZooming;
    public bool isAiming;
    [HideInInspector] public float defaultFOV;
    public bool smoothCam = false;

    [Header("Camera mouse incidence")]
    [SerializeField] private float rotationAmount = 4f;
    [SerializeField] private float maxRotationAmount = 5f;
    [SerializeField] private float smoothRotation = 12f;

    [Header("Footsteps Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private float ladderStep = 0.75f;
    [SerializeField] private float walkStepVolume = 0.75f;
    [SerializeField] private AudioClip[] concreteClips = default;
    [SerializeField] private AudioClip[] dallesClips = default;
    [SerializeField] private AudioClip[] betonSolideClips = default;
    [SerializeField] private AudioClip[] betonPleinairClips = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] woodSecClips = default;
    [SerializeField] private AudioClip[] dirtClips = default;
    [SerializeField] private AudioClip[] sandClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    [SerializeField] private AudioClip[] graviersClips = default;
    [SerializeField] private AudioClip[] waterClips = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] grilleClips = default;
    [SerializeField] private AudioClip[] pipeClips = default;
    [SerializeField] private AudioClip[] matelasClips = default;
    [SerializeField] private AudioClip[] moquetteClips = default;
    private float footstepTimer = 0;
    private float ladderStepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    private float GetCurrentVolume => isCrouching ? 0 : isSprinting ? 1 : walkStepVolume;

    [Space]
    [SerializeField] private AudioClip[] chainClips = default;
    [SerializeField] private AudioClip[] ladderClips = default;

    [Space]
    [SerializeField] private AudioClip[] tauntClip;

    private Vector3 hitPointNormal;
    private Vector3 wallPointNormal;
    public bool IsSliding;
    public bool CanWallJump;
    private string currentwalltag;
    public int wallJumpsCount;
    bool hitDiagonalCeiling;
    [HideInInspector] public float wallSlideLean;

    void OnControllerColliderHit(ControllerColliderHit collision)
    {
        isTouchingAnything = true;
        
        if (characterController.isGrounded && collision.moveDirection.y < -0.3)
        {
            hitPointNormal = collision.normal;
            RaycastHit slopeHitGround;
            var downRaySlopes = Physics.Raycast(transform.position, -Vector3.up, out slopeHitGround, 0.5f, landLayer);

            if (Vector3.Angle(collision.normal, Vector3.up) > 65 && (downRaySlopes ? Vector3.Angle(slopeHitGround.normal, Vector3.up) > 65 : 1 == 1))
                IsSliding = true;
            else
                IsSliding = false;

        }
        else IsSliding = false;

        if (!characterController.isGrounded && wallJumpsCount < maxWallJumps && !onLadder && (collision.transform.gameObject.layer == 0 || collision.transform.gameObject.layer == 14) && !downRay && Vector3.Angle(collision.normal, Vector3.up) > 88 && Vector3.Angle(collision.normal, Vector3.up) < 100)
        {
            if (!CanWallJump) {
                ChangeSlideClipServer(1);
                if (!slideAudio.isPlaying) SlideAudioPlay();
                slideAudio.volume = 1 * audio.volume;
            }
            if (rightHeadCast) wallSlideLean = 10f;
            else if (leftHeadCast) wallSlideLean = -10f;

            wallPointNormal = collision.normal;
            currentwalltag = collision.transform.tag;
            CanWallJump = true; 
        }
        else CanWallJump = false;

        if (collision.moveDirection.y < -0.3)
        {

            if (collision.transform.tag == "ShatterableGlass" && fallDistance > 1)
            {
                BreakGlassServer(collision.point, collision.normal, collision.transform.gameObject);
            }
        }

        if (!characterController.isGrounded && collision.moveDirection.y > 0.2f && !hitDiagonalCeiling && moveDirection.y > 0)
        {
            if (Vector3.Angle(collision.normal, Vector3.up) > 91 || Vector3.Angle(collision.normal, Vector3.up) < 170) {
                hitDiagonalCeiling = true;
                audio.PlayOneShot(ceilingHitClip);
                if (Vector3.Angle(collision.normal, Vector3.up) > 130) AddVerticalForce(new Vector3(0, -1, 0), isSprinting ? -2f : -1f);

                BForce(new Vector3(collision.normal.x, 0, collision.normal.z), 5.5f, false, true, jumpSlopeDecel * 2, true);
                playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }
        }
        else if (moveDirection.y > 0 && allJumped)
        {
            if (Vector3.Angle(collision.normal, Vector3.up) < 170 || Vector3.Angle(collision.normal, Vector3.up) > 182) return;

            allJumped = false;
            AddVerticalForce(new Vector3(0, -1, 0), isSprinting ? -8f : -5f);
            audio.PlayOneShot(ceilingHitClip);
            playerCamera.transform.DOPunchRotation(new Vector3(-fallShakeForce.x, 0 , -fallShakeForce.z), fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            moveDirection.y = isSprinting ? -8f : -5f;
        }

        
    }

    [SerializeField] public Camera playerCamera;
    [SerializeField] public GameObject playerCameraHolder;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    public CharacterController characterController;
    private NetworkObject networkObject;
    public SlopeSlide slopeSlideScript;
    public CustomAddForce customForceScript;
    [HideInInspector] public PauseManager pauseManager;
    public PlayerPickup playerPickupScript;
    [HideInInspector] public PlayerSetup setupScript;
    public CameraShakeConstrains cameraScript;
    private Slope slopeScript;
    [HideInInspector] public PostProcessVolume volume;
    [HideInInspector] public LensDistortion lensDistortion;
    [HideInInspector] public ColorGrading colorGrading;
    [SerializeField] private AudioSource slideAudio;
    [SerializeField] private NetworkAudioSource networkSlideAudio;
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioSource fallAudio;

    [Header("Functionnal Variables")]
    public bool landBool;
    private float landTimer;
    private bool slideEnd;
    private bool jumpSlope;
    private bool onLadder;
    private float aftervaultjumpTimer;
    public float headbobTimer;
    private float slideTimer;
    private float setSpeedTimer;
    private float slideResetTimer;
    private float slideCancelTimer;
    private float slideTime;
    private float coyoteTimer;
    private float fallShakeOldPos;
    private float fallDistance;
    private bool crouchPress;
    [HideInInspector] public bool jumped;
    private bool allJumped;
    public bool onMovingPlatform;
    public bool groundSphereCast;
    private float tauntTimer;
    private const float TauntObserverRateLimit = 0.2f;
    private float nextTauntObserverPlayTime;
    [HideInInspector] public bool slopeSlideJumped;

    [Header("Raycasts")]
    public bool shortfrontRay;
    public bool vaulRayDown;
    public bool vaulRayUp;
    public bool vaultRayUpShort;
    private bool steepSlideForward;
    private bool steepSlideBackward;
    public bool downRay;
    private bool animDownRay;
    private bool slideDownRay;
    private bool upSphere;
    private bool ladderRay;

    [Header("Slope Parameters")]
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;

    bool flymode;

    private bool OnSlopeAir()
    {
        if (!safeGrounded) return false;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength))
            if (hit.normal != Vector3.up && Vector3.Angle(hit.normal, Vector3.up) > 10 && Vector3.Angle(hit.normal, Vector3.up) < 65)
                return true;
        return false;
    }

    private bool OnSlopeForSlide()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength))
            if (hit.normal != Vector3.up && Vector3.Angle(hit.normal, Vector3.up) > 10 && Vector3.Angle(hit.normal, Vector3.up) < 65)
                return true;
        return false;
    }

    private bool OnSlopeIce()
    {
        if (!safeGrounded) return false;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength * 10))
            if (hit.normal != Vector3.up && Vector3.Angle(hit.normal, Vector3.up) > 10 && Vector3.Angle(hit.normal, Vector3.up) < 65)
                return true;
        return false;
    }

    public static FirstPersonController instance;

    private VerletSpring[] physCables;

    private SetPlayerVolume playerVolume;

    private TextMeshProUGUI speedometer;

    void Awake()
    {
        instance = this;
        
        playerVolume = GetComponent<SetPlayerVolume>();

        physCables = GetComponentsInChildren<VerletSpring>();
        
        playerControls = InputManager.inputActions;

        networkObject = GetComponent<NetworkObject>();

        startOfRound = true;
        

        playerPickupScript = GetComponent<PlayerPickup>();

        slopeSlideScript = GetComponent<SlopeSlide>();
        slopeScript = GetComponent<Slope>();
        cameraScript = playerCamera.GetComponent<CameraShakeConstrains>();
        customForceScript = GetComponent<CustomAddForce>();
        setupScript = GetComponent<PlayerSetup>();

        characterController = GetComponent<CharacterController>();
        
        playerCameraHolder.transform.localPosition = new Vector3(0, playerCameraHolder.transform.localPosition.y + offset.y, 0);
        defaultYPos = playerCameraHolder.transform.localPosition.y + offset.y;
        defaultFOV = playerCamera.fieldOfView;

        if (GameObject.FindGameObjectWithTag("PauseManager").GetComponent<PauseManager>()!= null) 
            pauseManager = GameObject.FindGameObjectWithTag("PauseManager").GetComponent<PauseManager>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        highFallVfxComponent = highFallVfx.GetComponent<ParticleSystem>();
        volume = playerCamera.transform.GetComponent<PostProcessVolume>();

        if (volume.profile.TryGetSettings(out LensDistortion lens))
        {
            lensDistortion = lens;
        }

        if (volume.profile.TryGetSettings(out ColorGrading color))
        {
            colorGrading = color;
        }

        
        //Input
        playerControls.Player.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        playerControls.Player.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

        if (GameObject.Find("Speedometer") != null) speedometer = GameObject.Find("Speedometer").GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {

        move = playerControls.Player.Move;
        move.Enable();

        zoom = playerControls.Player.Zoom;
        zoom.Enable();
        zoom.started += SetZoom;
        zoom.canceled += SetZoom;
        zoom.performed += SetZoomToggle;

        lookY = playerControls.Player.MouseY;
        lookY.Enable();

        lookX = playerControls.Player.MouseX;
        lookX.Enable();

        jump = playerControls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        run = playerControls.Player.Run;
        run.Enable();

        leanLeft = playerControls.Player.LeanLeft;
        leanLeft.Enable();

        leanRight = playerControls.Player.LeanRight;
        leanRight.Enable();

        record = playerControls.Player.VoiceChat;
        record.Enable();

        moveUp = playerControls.Player.MoveUp;
        moveUp.Enable();

        crouch = playerControls.Player.Crouch;
        crouch.Enable();
        crouch.performed += Slide;
        crouch.started += SetCrouch;
        crouch.canceled += SetCrouch;
        crouch.canceled += SlideEnd;

        fire1 = playerControls.Player.FireHold;
        fire1.Enable();

        fire2 = playerControls.Player.RightClick;
        fire2.Enable();

        reload = playerControls.Player.Reload;
        reload.Enable();

        PauseManager.OnRoundStarted += roundStartEvent;
        PauseManager.OnBeforeSpawn += OnBeforeSpawn;
    }
    private void OnDisable()
    {
        move.Disable();
        moveUp.Disable();
        jump.Disable();
        jump.performed -= Jump;
        run.Disable();
        zoom.Disable();
        zoom.started -= SetZoom;
        zoom.canceled -= SetZoom;
        zoom.performed -= SetZoomToggle;
        lookY.Disable();
        lookX.Disable();
        crouch.Disable();
        crouch.performed -= Slide;
        crouch.started -= SetCrouch;
        crouch.canceled -= SetCrouch;
        crouch.canceled -= SlideEnd;
        leanLeft.Disable();
        leanRight.Disable();
        record.Disable();

        fire1.Disable();
        reload.Disable();
        fire2.Disable();
        
        PauseManager.OnRoundStarted -= roundStartEvent;
        PauseManager.OnBeforeSpawn -= OnBeforeSpawn;

        speedometer.text = "";
        
        vstreamRecorder.IsRecording = false;
    }

    private void OnBeforeSpawn() {
        canMove = false; 
        setupScript.wasMoving = false;
    }

    [ServerRpc (RunLocally = true)]
    public void CmdChangeRootMotion(bool istrue)
    {
        ChangeRootMotion(istrue);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ChangeRootMotion(bool istrue)
    {
        animator.applyRootMotion = istrue;
    }

    public bool sprintToggle;
    public bool aimToggle;
    public bool leanToggle;
    public bool crouchToggle;
    public bool reverseSprintBind;
    public bool invertX;
    public bool invertY;

    bool funcSprint;
    bool funcSprintTrigger;
    bool isLeaningLeft;
    bool isLeaningRight;
    bool funcLeanRightTrigger;
    bool funcLeanLeftTrigger;
    bool funcAim;
    bool funcInvertX;
    bool funcInvertY;

    float distToRunFov;
    float distToSlideFov;
    float distToRunSlideFov;

    void Start()
    {
        settings = Settings.Instance;

        distToRunFov = runFOV - defaultFOV;
        distToSlideFov = slideFOV - defaultFOV;
        distToRunSlideFov = runSlideFOV - defaultFOV;

        defaultFOV = settings.normalFovValue;
    }

    bool safeDeathFalling;
    float dmgZoneTimer;
    
    void Update()
    {
        isTouchingAnything = false;
        
        HandleTimers();
        VoiceChat();

        //Handle FOV settings
        defaultFOV = settings.normalFovValue;
        runFOV = defaultFOV + distToRunFov;
        slideFOV = defaultFOV + distToSlideFov;
        runSlideFOV = defaultFOV + distToRunSlideFov;

        //Safe Death Falling
        if (transform.position.y < -300 && !safeDeathFalling)
        {
            safeDeathFalling = true;
            Settings.Instance.IncreaseSuicidesAmount();
            GetComponent<PlayerHealth>().fellVoid = true;
            DespawnObject(gameObject);
        }

        if(Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftControl) && Application.isEditor)
        {
            Debug.Break();
        }

        if(Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftControl))
        {
            if ( (Application.isEditor || SceneMotor.Instance.testMap)) flymode = !flymode;
        }

        if (characterController.skinWidth != (isCrouching && !isSliding ? 0.07f : 0.2f)) {
            setupScript.ChangeSkinWidth((isCrouching && !isSliding ? 0.07f : 0.2f));
            #if UNITY_EDITOR
            Debug.Log("FirstPersonController: Skin width changed");
            #endif
        }
        characterController.skinWidth = (isCrouching && !isSliding ? 0.07f : 0.2f);
        
        colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, 0, saturationSpeed * Time.deltaTime);
        colorGrading.gamma.value = new Vector4(1,1,1,settings.brightness-1);

        dirForward = Vector3.Lerp(dirForward, transform.TransformDirection(Vector3.forward), 13 * Time.deltaTime);
        //dirForward = transform.TransformDirection(Vector3.forward);
        dirRight = Vector3.Lerp(dirRight, transform.TransformDirection(Vector3.right), 13 * Time.deltaTime);
        //dirRight = transform.TransformDirection(Vector3.right);

        if (pauseManager.nonSteamworksTransport) canMove = true;

        if (MapsManager.Instance != null) {
            if (pauseManager.inVictoryMenu || MapsManager.Instance.inExplorationMap) canMove = true;
        }

        if (SceneMotor.Instance != null) {
            if (SceneMotor.Instance.testMap) canMove = true;
        }

        if (canMove) startOfRound = false;
    

        if ((pauseManager != null ? !pauseManager.pause && (pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) : 1==1))
        {
            if (!pauseManager.otherPauseBools && (startOfRound ? true : canMove)) HandleMouseLook();
            HandleCameraController();
            if (canMove) {

                if (playerCamera == null) return;
                
                CalculateMovementInput();
                HandlePhysicsCasting();
                CheckForVault();
                HandleLadders();
                if (!flymode) HandleCameraLean();
                

                if (canCrouch) HandleCrouch();

                if (canUseHeadMovement) HandleHeadMovement();

                if (useFootsteps) HandleFootsteps();
            }
        }
        else 
        {
            currentInput = Vector2.zero;
        }


        HandleBForce();

        if (!flymode) HandleMovementInput();

        HandleAddingForce();

        if ((pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) && canMove) HandleAnimation();

        if ((pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1)) HandleTaunt();
        
        if (!flymode) ApplyFinalMovements();

        if (canMove) {

            if (flymode)
            {
                bool isAscending = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E);
                bool isDescending = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q);
                int vertical = (isAscending ? 1 : 0) + (isDescending ? -1 : 0);
                Vector3 motion =
                    move.ReadValue<Vector2>().y * transform.forward +
                    move.ReadValue<Vector2>().x * transform.right +
                    transform.up * (vertical);

                motion *= Time.deltaTime * 10 * (isSprinting ? 2 : 1);
                characterController.Move(motion);
            }

            if (!characterController.isGrounded && moveDirection.y < 0)
            {
                fallDistance -= moveDirection.y * Time.deltaTime;
            }

            if (moveDirection.y > 0) fallDistance = 0;


            if (characterController.isGrounded) {
                moveDirection.y = -1f;
                coyoteTimer = 0.15f;
                hitDiagonalCeiling = false;
                
            }

            if ((Physics.Raycast(transform.position, Vector3.down, 0.5f) && landTimer > 0 && !allJumped && moveDirection.y < 0) || characterController.isGrounded)
                safeGrounded = true;
            else
                safeGrounded = false;
                    

            //Set run
            if (!playerPickupScript.hasObjectInLeftHand)
                isAiming = isZooming;
            else isAiming = false;

            if (sprintToggle)
            {
                if (run.ReadValue<float>() > 0.1f && funcSprintTrigger)
                {
                    funcSprint = !funcSprint;
                    funcSprintTrigger = false;
                }

                if (run.ReadValue<float>() < 0.1f) funcSprintTrigger = true;
                
            }
            else funcSprint = (reverseSprintBind ? run.ReadValue<float>() < 0.1f : run.ReadValue<float>() > 0.1f);

            isSprinting = (!isLeaning && !isAiming && move.ReadValue<Vector2>() != Vector2.zero && !isCrouching ? funcSprint : false);
            isSlideSprinting = (!isLeaning && !isAiming && move.ReadValue<Vector2>() != Vector2.zero ? funcSprint : false);
            isWalking = (move.ReadValue<Vector2>() != Vector2.zero);
            isGrounded = characterController.isGrounded;

            if (leanToggle)
            {
                if (leanLeft.ReadValue<float>() > 0.1f && funcLeanLeftTrigger)
                {
                    isLeaningLeft = !isLeaningLeft;
                    isLeaningRight = false;
                    funcLeanLeftTrigger = false;
                }
                else if (leanRight.ReadValue<float>() > 0.1f && funcLeanRightTrigger)
                {
                    isLeaningLeft = false;
                    isLeaningRight = !isLeaningRight;
                    funcLeanRightTrigger = false;
                }

                if (leanLeft.ReadValue<float>() < 0.1f) funcLeanLeftTrigger = true;
                if (leanRight.ReadValue<float>() < 0.1f) funcLeanRightTrigger = true;
            }
            else{
                isLeaningLeft = leanLeft.ReadValue<float>() > 0.1f;
                isLeaningRight = leanRight.ReadValue<float>() > 0.1f;

                if (isLeaningLeft && isLeaningRight) {
                    isLeaningLeft = false;
                    isLeaningRight = false;
                }
            }

            
            isLeaning = (isLeaningLeft || isLeaningRight);

            if (isGrounded)
            {
                wallJumpsCount = 0;
                jumped = false;
                allJumped = false;
                slopeSlideJumped = false;

                settings.timeSpentOnGround += Time.deltaTime;
            }
            else settings.timeSpentInAir += Time.deltaTime;
        }

        if (Settings.Instance.showSpeedometer) speedometer.text = $"Speed : {(Mathf.RoundToInt((playerSpeed*100))*0.01f).ToString()}";

    }

    private void HandleTaunt()
    {
        tauntTimer -= Time.deltaTime;

        if (tauntTimer > 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            AboubiPlayServer(0);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            AboubiPlayServer(1);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            AboubiPlayServer(2);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            AboubiPlayServer(3);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            AboubiPlayServer(4);
            Settings.Instance.IncreaseTauntsAmount();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            AboubiPlayServer(5);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)) {
            AboubiPlayServer(6);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) {
            AboubiPlayServer(7);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            AboubiPlayServer(8);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            AboubiPlayServer(9);
            Settings.Instance.IncreaseTauntsAmount();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) tauntTimer = 0.4f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) tauntTimer = 0.3f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) tauntTimer = 0.3f;
        if (Input.GetKeyDown(KeyCode.Alpha4)) tauntTimer = 0.5f;
        if (Input.GetKeyDown(KeyCode.Alpha5)) tauntTimer = 0.7f;
        if (Input.GetKeyDown(KeyCode.Alpha6)) tauntTimer = 0.4f;
        if (Input.GetKeyDown(KeyCode.Alpha7)) tauntTimer = 0.7f;
        if (Input.GetKeyDown(KeyCode.Alpha8)) tauntTimer = 0.9f;
        if (Input.GetKeyDown(KeyCode.Alpha9)) tauntTimer = 1.0f;
        if (Input.GetKeyDown(KeyCode.Alpha0)) tauntTimer = 0.3f;
    }

    [ServerRpc (RunLocally = true)]
    private void AboubiPlayServer(int clip)
    {
        AboubiPlayObservers(clip);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void AboubiPlayObservers(int clip)
    {
        if (Time.unscaledTime < nextTauntObserverPlayTime) return;
        nextTauntObserverPlayTime = Time.unscaledTime + TauntObserverRateLimit;
        audio.PlayOneShot(tauntClip[clip]);
    }

    private void HandleAnimation()
    {
        if (!IsOwner) { return; }

        animator.SetBool("Crouch", isCrouching);
        animator.SetBool("Grounded", slideDownRay && moveDirection.y <= 0);
        animator.SetBool("Slide", isSliding || slopeSlideScript.isCrouchSlopeSliding || slideTimer > 0.1f);
        animator.SetFloat("crouchMove", move.ReadValue<Vector2>().magnitude);
        animator.SetFloat("MovementSpeed", (isSprinting ? 1 : isWalking ? 0.5f : 0));
        animator.SetFloat("Vertical", (isSliding ? -(rotationX/90 -1)-2 : -((rotationX) / 90)));

        animator.SetBool("LeanRight", isLeaningRight && !isSliding && isGrounded && Mathf.Abs(leanCamera.transform.localEulerAngles.z) < 339);
        animator.SetBool("LeanLeft", isLeaningLeft && !isSliding && isGrounded && Mathf.Abs(leanCamera.transform.localEulerAngles.z) > 19);

        if (torso.activeSelf && !isSliding && !slopeSlideScript.isCrouchSlopeSliding) torso.SetActive(false);
        if (legLeft.activeSelf && !isSliding && !slopeSlideScript.isCrouchSlopeSliding) legLeft.SetActive(false);
        if (legRight.activeSelf && !isSliding && !slopeSlideScript.isCrouchSlopeSliding) legRight.SetActive(false);
    }

    private IEnumerator ActiveTorso()
    {
        
        yield return new WaitForSeconds(0f);

        torso.SetActive(true);
        legLeft.SetActive(true);
        legRight.SetActive(true);
        
    }

    private float tempSetSpeed;

    private void CalculateMovementInput()
    {
        playerSpeed = Mathf.Round(characterController.velocity.magnitude * 100) * 0.01f;

        //Calculate Input
        if (move.ReadValue<Vector2>().x > 0 && baircontrol) horizontalInput = Mathf.Lerp(horizontalInput, 1, globalAcceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);
        else if (move.ReadValue<Vector2>().x < 0 && baircontrol) horizontalInput = Mathf.Lerp(horizontalInput, -1, globalAcceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);
        else horizontalInput = Mathf.Lerp(horizontalInput, 0, globalDeceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);

        if (move.ReadValue<Vector2>().y > 0 && baircontrol) verticalInput = Mathf.Lerp(verticalInput, 1, globalAcceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);
        else if (move.ReadValue<Vector2>().y < 0 && baircontrol) verticalInput = Mathf.Lerp(verticalInput, -1, globalAcceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);
        else verticalInput = Mathf.Lerp(verticalInput, 0, globalDeceleration * (!characterController.isGrounded ? airControl : 1) * Time.deltaTime);

        //Calculate Movement Speed (by order)
        if (slopeSlideScript.isCrouchSlopeSliding ? !slopeScript.uphill : 1 == 1)
        {
            if (!characterController.isGrounded && funcSprint && move.ReadValue<Vector2>().magnitude != 0 && !isLeaning)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, sprintAirSpeed, sprintAirAcceleration * Time.deltaTime) * 100) * 0.01f;

            else if (!characterController.isGrounded && move.ReadValue<Vector2>().magnitude != 0 && !isLeaning)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, airSpeed, airAcceleration * Time.deltaTime) * 100) * 0.01f;


            /*else if (slopeSlideScript.isSteepSlopeSliding && characterController.isGrounded && slopeScript.uphill)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, 2, airAcceleration * Time.deltaTime) * 100) * 0.01f;*/

            else if (isCrouching && move.ReadValue<Vector2>().magnitude != 0 && characterController.isGrounded && !isLeaning)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, crouchSpeed, crouchAcceleration * Time.deltaTime) * 100) * 0.01f;

            else if (isSprinting && move.ReadValue<Vector2>().magnitude != 0 && !isLeaning)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, sprintSpeed, sprintAcceleration * Time.deltaTime) * 100) * 0.01f;

            else if (move.ReadValue<Vector2>().magnitude != 0 && !isLeaning)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, walkSpeed, walkAcceleration * Time.deltaTime) * 100) * 0.01f;

            else
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, 1, globalDeceleration * Time.deltaTime) * 100) * 0.01f;
        }
        /*else if (slopeSlideScript.isSteepSlopeSliding && characterController.isGrounded && slopeScript.uphill)
                speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, 2, airAcceleration * Time.deltaTime) * 100) * 0.01f;*/
        else
            speedFactor = Mathf.Round(Mathf.Lerp(speedFactor, 1, globalDeceleration * Time.deltaTime) * 100) * 0.01f;

        setSpeedTimer -= Time.deltaTime;

        if (setSpeedTimer > 0) speedFactor = tempSetSpeed;
        

        //Set Movement Input
        //if (slopeSlideScript.onIce && safeGrounded && currentInputRaw != Vector2.zero) currentInput = new Vector2(0, 0.001f);
        //else 
        currentInput = new Vector2((Mathf.Round(verticalInput * 100) * 0.01f), (Mathf.Round(horizontalInput * 100) * 0.01f)) * speedFactor;
        currentInputRaw = (!characterController.isGrounded && isSprinting ? sprintAirSpeed : !characterController.isGrounded ? airSpeed : isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * move.ReadValue<Vector2>();

        //Change pitch of footsteps according to mass
        audio.pitch = Mathf.Lerp(0.5f, 1, movementFactor);
    }

    public void SetSpeed(float speed, float time)
    {
        tempSetSpeed = speed;
        setSpeedTimer = time;
    }

    Vector3 dirForward;
    Vector3 dirRight;

    private void HandleMovementInput()
    {
        
        if (canMove) currentInput = Vector2.ClampMagnitude(currentInput, (!characterController.isGrounded && isSprinting ? sprintAirSpeed : !characterController.isGrounded ? airSpeed : isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed)) * movementFactor;
        else currentInput = Vector2.zero;

        float  moveDirectionY = moveDirection.y;
        moveDirection = (dirForward * currentInput.x) + (dirRight * currentInput.y);

        moveDirection.y = moveDirectionY;
    }

    [SerializeField] private Transform legsPivot;
    float legsRotation;

    private void HandleMouseLook()
    {
        rotationX -= mouseInput.y * 0.1f * (isScopeAiming ? lookSpeedAim : isAiming ? lookSpeedAimNoScope : lookSpeedY) * (invertY ? -1 : 1) * (pauseManager.gamepad ? 14 : 1);
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCameraHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);

        //if (mouseInput.x != 0)
            //animator.SetFloat("Horizontal", mouseInput.x * 0.05f * lookSpeedX, 2, Time.deltaTime);

        transform.rotation *= Quaternion.Euler(0, mouseInput.x * 0.1f * (invertX ? -1 : 1) * (isScopeAiming ? lookSpeedAim : isAiming ? lookSpeedAimNoScope : lookSpeedX) * (pauseManager.gamepad ? 14 : 1), 0);

        /*legsRotation += mouseInput.x * 0.1f * (invertX ? -1 : 1) * (isScopeAiming ? lookSpeedAim :lookSpeedX);
        legsRotation = Mathf.Clamp(legsRotation, -45, 45);

        legsPivot.localRotation = Quaternion.Euler(0, legsRotation, 0);*/
    }

    private void HandleCameraLean()
    {

        if (isLeaningRight && !isSliding && isGrounded)
            CameraLean(Quaternion.Euler(0, 0, - leanLimit));
        else if (isLeaningLeft && !isSliding && isGrounded)
            CameraLean(Quaternion.Euler(0, 0, leanLimit));
        else 
            CameraLean(Quaternion.Euler(0, 0, 0));
        
    }

    bool camWall;
    bool camWallExit;
    Quaternion wallExitRot;

    private void CameraLean(Quaternion rot)
    {
        bool hasHitWallLeft = Physics.Raycast(transform.position + Vector3.up * 2, -transform.right, out RaycastHit wallLeft, 1.8f, landLayer);
        bool hasHitWallRight = Physics.Raycast(transform.position + Vector3.up * 2, transform.right, out RaycastHit wallRight, 1.8f, landLayer);

        if (isLeaningRight){
            if (hasHitWallRight)
                leanCamera.transform.localRotation = Quaternion.Slerp(leanCamera.transform.localRotation, Quaternion.Slerp(Quaternion.Euler(0,0,0), rot, Vector3.Distance(transform.position + Vector3.up * 2, wallRight.point) / 1.8f), (Vector3.Distance(playerCamera.transform.position, wallRight.point) < 0.8f ? 200 : leanSpeed) * Time.deltaTime);
            else
                leanCamera.transform.localRotation = Quaternion.Slerp(leanCamera.transform.localRotation, rot, leanSpeed * Time.deltaTime);
        }
        else if (isLeaningLeft){
            if (hasHitWallLeft)
                leanCamera.transform.localRotation = Quaternion.Slerp(leanCamera.transform.localRotation, Quaternion.Slerp(Quaternion.Euler(0,0,0), rot, Vector3.Distance(transform.position + Vector3.up * 2, wallLeft.point) / 1.8f), (Vector3.Distance(playerCamera.transform.position, wallLeft.point) < 0.8f ? 200 : leanSpeed) * Time.deltaTime);
            else
                leanCamera.transform.localRotation = Quaternion.Slerp(leanCamera.transform.localRotation, rot, leanSpeed * Time.deltaTime);
        }
        else leanCamera.transform.localRotation = Quaternion.Slerp(leanCamera.transform.localRotation, Quaternion.Euler(0,0,0), leanSpeed * Time.deltaTime);

            
    }


    public void Jump(InputSystem.InputAction.CallbackContext ctx)
    {
        if (!canJump || !canMove || slopeSlideScript.onSuperIce) return;

        if (CanWallJump) {
            wallJumpsCount ++;
            //SlideAudioStop();
            SlideFadeOut();
            WallJumpClip(currentwalltag);
            PlaySoundServer(22);
            BForce(wallPointNormal, (move.ReadValue<Vector2>() != Vector2.zero  ? jumpSlopeForce : 7) * 0.7f, false, true, jumpSlopeDecel, true);
            moveDirection.y = jumpForce * 0.8f * wallJumpFactor;
            playerCamera.transform.DOPunchRotation(-fallShakeForce * 1.5f, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            CanWallJump = false;
        }

        if ((pauseManager != null ? pauseManager.pause || (pauseManager.chatting && pauseManager.steamPlaying) : 1==1)) return;

        if (characterController.isGrounded) {
            jumpSlope = true;
            moveDirection.y = 0;
        }

        if (slopeSlideScript.isCrouchSlopeSliding) slopeSlideScript.slopeSlideMove = new Vector3(slopeSlideScript.slopeSlideMove.x, 0, slopeSlideScript.slopeSlideMove.z);

        jumpSlope = true;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit prejumpHit, 1.3f) && moveDirection.y < 0 && !characterController.isGrounded) prejump = true;

        if (characterController.isGrounded && !prejump) {
            allJumped = true;
            slopeSlideJumped = true;
            coyoteTimer = 0;
            moveDirection.y = jumpForce * jumpFactor;
            slideCancelTimer = 2;
            animator.Rebind();
            networkAnimator.SetTrigger("Jump");

            foreach (var obj in physCables)
            {
                obj.AddForce(0, -transform.forward * 5f);
            }
            
            if (IsSliding) {
                BForce(hitPointNormal, move.ReadValue<Vector2>() != Vector2.zero  ? jumpSlopeForce : 7, false, true, jumpSlopeDecel, true);
                playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }
            

            PlaySoundServer(10);
            //Instantiate(jumpDust, transform.position, Quaternion.identity);
        }
        else if (coyoteTimer > 0 && !characterController.isGrounded && moveDirection.y < 0)
        {
            allJumped = true;
            slopeSlideJumped = true;
            prejump = false;
            coyoteTimer = 0;
            moveDirection.y = jumpForce * jumpFactor;
            slideCancelTimer = 2;
            animator.Rebind();
            networkAnimator.SetTrigger("Jump");

            if (IsSliding) {
                BForce(hitPointNormal, move.ReadValue<Vector2>() != Vector2.zero  ? jumpSlopeForce : 7, false, true, jumpSlopeDecel, true);
                playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }

            PlaySoundServer(10);
            //Instantiate(jumpDust, transform.position, Quaternion.identity);
        }
        else if (aftervaultjumpTimer > 0)
        {
            allJumped = true;
            jumped = true;
            slopeSlideJumped = true;
            vaultActivate = true;
            prejump = false;
            aftervaultjumpTimer = 0;
            moveDirection.y = 0;
            moveDirection.y = jumpForce * jumpFactor;
            slideCancelTimer = 2;
            animator.Rebind();
            networkAnimator.SetTrigger("Jump");

            if (IsSliding) {
                BForce(hitPointNormal, move.ReadValue<Vector2>() != Vector2.zero  ? jumpSlopeForce : 7, false, true, jumpSlopeDecel, true);
                playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }

            PlaySoundServer(10);
            //Instantiate(jumpDust, transform.position, Quaternion.identity);
        }
    }

    public void SetZoom(InputSystem.InputAction.CallbackContext ctx)
    {
        if ((pauseManager != null ? !pauseManager.pause && (pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) : 1==1) && !aimToggle)
            isZooming = ctx.started;
    }

    public void SetZoomToggle(InputSystem.InputAction.CallbackContext ctx)
    {
        if ((pauseManager != null ? !pauseManager.pause && (pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) : 1==1) && aimToggle)
            isZooming = !isZooming;
    }

    public void SetCrouch(InputSystem.InputAction.CallbackContext ctx)
    {
        if (crouchToggle) return;
        isCrouching = ctx.started;
        crouchPress = ctx.started;
    }

    bool crouchExit;

    private void HandleCrouch()
    {
        if (Physics.Raycast(transform.position, Vector3.up, 2f, landLayer)) {
            crouchExit = true;
            isCrouching = true;
        }
        else if (crouchExit && !crouchPress) {
            isCrouching = false;
            crouchExit = false;
        }
        
        var desiredHeight = isSliding || slopeSlideScript.isCrouchSlopeSliding ? slideHeight : isCrouching ? crouchHeight : standHeight;

        if (characterController.height != desiredHeight)
        {
            AdjustHeight(desiredHeight);
        }

        HandleSlide();
    }

    private void HandleHeadMovement()
    {


        if (camController)
        {
            leanCamera.transform.localPosition = characterController.center + new Vector3(0, characterController.height/2 - 2.5f, 0); 
        }
        /*else if ((currentInputRaw != Vector2.zero) && camController && characterController.isGrounded)
        {
            headbobTimer += Time.deltaTime * (isSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, characterController.height + Mathf.Sin(headbobTimer) * (isSprinting ? sprintBobAmount : walkBobAmount), playerCamera.transform.localPosition.z);
        }*/

    }

    bool prejumpLand;
    bool activateFallAudio;
    public bool startOfRound;
    private void ApplyFinalMovements()
    {
        if (pauseManager.startRound || startOfRound) highFallVfxComponent.startColor = new Color(255, 255, 255,0);
        if (pauseManager.startRound || startOfRound) return;

        if (playerSpeed < 15) activateFallAudio = true;
        if (playerSpeed > 15 && activateFallAudio) 
        {
            activateFallAudio = false;
            fallAudio.Play();
        }

        if (playerSpeed > 15 && fallDistance > 4)
        {
            fallAudio.volume = Mathf.Lerp(fallAudio.volume, 1, fallAudioLerpSpeed * Time.deltaTime);
        }
        else 
        {
            fallAudio.volume = Mathf.Lerp(fallAudio.volume, 0, fallAudioLerpOutSpeed * Time.deltaTime);
        }

        highFallVfxComponent.startColor = new Color(255, 255, 255, (rotationX > 20 && fallAudio.volume > 0.05f ? Mathf.Lerp(highFallVfxComponent.startColor.a, 255, 50 * Time.deltaTime) : rotationX < 20 && fallAudio.volume > 0.05f ? Mathf.Lerp(highFallVfxComponent.startColor.a, 0, 50 * Time.deltaTime) : Mathf.Lerp(highFallVfxComponent.startColor.a, 0, 50 * Time.deltaTime)));
        lensDistortion.intensity.value = (rotationX > 20 && fallAudio.volume > 0.05f ? Mathf.Lerp(lensDistortion.intensity.value, lensDistortionWhenFalling, lensDistortionLerpSpeed * Time.deltaTime) : rotationX < 20 && fallAudio.volume > 0.05f ? Mathf.Lerp(lensDistortion.intensity.value, 0, lensDistortionLerpOutSpeed * Time.deltaTime) : Mathf.Lerp(lensDistortion.intensity.value, 0, lensDistortionLerpOutSpeed * Time.deltaTime));

        if (fallAudio.volume < 0.03f && playerSpeed < 15) fallAudio.Stop();

        if (prejump && characterController.isGrounded && moveDirection.y < 0) 
        {
            allJumped = true;
            slopeSlideJumped = true;
            moveDirection.y = jumpForce * jumpFactor;
            animator.Rebind();
            networkAnimator.SetTrigger("Jump");

            if (IsSliding) {
                BForce(hitPointNormal, move.ReadValue<Vector2>() != Vector2.zero  ? jumpSlopeForce : 7, false, true, jumpSlopeDecel, true);
                playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }

            PlaySoundServer(10);
            slideCancelTimer = 2;
            //Instantiate(jumpDust, transform.position, Quaternion.identity);
            prejump = false;
            prejumpLand = true;
        }

        if (slideEnd && characterController.isGrounded)
        {
            slideEnd = false;
            //forceFactor = 0;
            deceleration = 2.5f;
        }

        if (moveDirection.y < -3 && !characterController.isGrounded) landBool = true;

        //All that happens when landing

        if (landBool && characterController.isGrounded && !IsSliding)
        {
            landBool = false;
            if (!prejumpLand) 
            {
                PlaySoundServer(11);
                if (!isCrouching && landBobPivot.localPosition.y > -0.4f) landBobPivot.DOLocalMove(landBobPivot.localPosition - Vector3.up * landBobAmount, landBobDuration).SetEase(landBobEase);
            }
            foreach (var obj in physCables)
            {
                obj.AddForce(0, Vector3.up * 4.5f);
            }
            //Instantiate(landDust, transform.position, Quaternion.identity);

            

            var initRotation = playerCamera.transform.eulerAngles;

            if (fallDistance > fallShakeHeight && !vault)
            {
                if (fallDistance > highFallShakeHeight) {
                    playerCamera.transform.DOLocalRotate(playerCamera.transform.localEulerAngles - highFallShakeForce, highFallShakeDuration);
                    cameraScript.rotateBack = true;
                    cameraScript.baseSpeed = highFallShakeRecoverSpeed;
                    PlaySoundServer(21);
                    StartCoroutine(HighCameraShake());
                }
                else playerCamera.transform.DOPunchRotation(-fallShakeForce, fallShakeDuration, fallShakeVibrato, fallShakeElasticity);
            }

            footstepTimer = GetCurrentOffset;

            prejumpLand = false;

            fallDistance = 0;
        }

        //Recover from land bob
        if (landBobPivot.localPosition.y < -0.02f) landBobPivot.localPosition = Vector3.Lerp(landBobPivot.localPosition, Vector3.zero, landRecoverSpeed * Time.deltaTime * (isCrouching ? 7 : !isGrounded ? 5 : 1));

        if (characterController.isGrounded) landTimer = 0.3f;

        //Steep Slope Force Cancelation
        if (groundSphereCast && Vector3.Angle(Vector3.up, groundNormal) > 65 && !jumpSlope && isGrounded)
        {
            bforcefinal = Vector3.zero;
            forceAdded = Vector3.zero;
        }

        //Handle Gravity

        if (OnSlopeIce() && slopeSlideScript.onIce && !jumpSlope)
        {
            moveDirection.y -= (isCrouching ? crouchGravity * slopeForce * 2 : moveDirection.y > 0 ? jumpGravity : gravity * slopeForce) * gravityMultiplier * Time.deltaTime;
        }
        else if (OnSlopeAir() && currentInput.magnitude != 0 && !jumpSlope)
        {
            moveDirection.y -= (isCrouching ? crouchGravity * slopeForce * 2 : moveDirection.y > 0 ? jumpGravity : gravity * slopeForce) * gravityMultiplier * Time.deltaTime;
        }
        else if (!characterController.isGrounded && moveDirection.y > -40) {
            moveDirection.y -= (isCrouching ? crouchGravity : moveDirection.y > 0 ? jumpGravity : gravity) * gravityMultiplier * Time.deltaTime;
        }

        if (!pauseManager.startRound) characterController.Move((moveDirection + forceAdded + bforcefinal + moveAdded + slopeSlideScript.slopeSlideMove + slopeSlideScript.steepSlopeSlideMove + objectCollisionMoveDirection + customForceFinal) * Time.deltaTime);

        jumpSlope = false;
    }

    IEnumerator HighCameraShake()
    {
        yield return new WaitForSeconds(highFallShakeRecoverTime);

        cameraScript.rotateBack = true;
    }

    private void HandleCameraController()
    {
        var desiredFOV = isAiming ? zoomFOV : (isSliding && isSprinting) || slopeSlideScript.isCrouchSlopeSliding ? runSlideFOV : isSliding ? slideFOV : isWalking && funcSprint ? runFOV : defaultFOV;
        desiredFOV = Mathf.Round(desiredFOV * 100) * 0.01f;

        if (playerCamera.fieldOfView != desiredFOV)
        {
            ChangeFOV(desiredFOV, (sprintSlide ? runSlideEase : walkSlide ? slideEase : isSprinting ? runEase : zoomEase) , (sprintSlide ? runSlideEaseSpeed : walkSlide ? slideEaseSpeed : isSprinting ? runEaseSpeed : zoomSpeed));
        }
        //Camera Tilt
        var desiredTilt = (isSliding || slopeSlideScript.isCrouchSlopeSliding ? -slideTiltAmount : isSprinting && move.ReadValue<Vector2>().x > 0 ? -tiltAmount : isSprinting && move.ReadValue<Vector2>().x < 0 ? tiltAmount : 0) + wallSlideLean;
        if (camController) SideTilt(desiredTilt);
    }

    private void AdjustHeight(float height)
    {
        float center = height / 2;

        characterController.height = Mathf.Lerp(characterController.height, height, crouchingSpeed * Time.deltaTime);
        characterController.center = Vector3.Lerp(characterController.center, new Vector3(0, center, 0), crouchingSpeed * Time.deltaTime);
    }

    private void ChangeFOV(float fov, Ease easeType, float speed)
    {
        //playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomSpeed * Time.deltaTime );
        playerCamera.DOFieldOfView(fov, speed).SetEase(easeType);
    }

    private void SideTilt(float temptiltAmount)
    {
        //Mouse Movement Incidence
        float tiltY = Mathf.Clamp(-mouseInput.x * 0.1f * rotationAmount, -maxRotationAmount, maxRotationAmount);

        rotationZ = Mathf.Lerp(rotationZ, temptiltAmount, tiltSpeed * Time.deltaTime) + Mathf.Lerp(transform.localRotation.z, tiltY, Time.deltaTime * smoothRotation * 2.3f);
    }

    public void Slide(InputSystem.InputAction.CallbackContext ctx)
    {
        if (crouchToggle) {
            isCrouching = !isCrouching;
            if (!isCrouching) EndSlide();
        }

        if (move.ReadValue<Vector2>().y < 0) return;

        deceleration = 1;

        if (move.ReadValue<Vector2>() != Vector2.zero && slideResetTimer < 0){// && characterController.height > crouchHeight + 0.2f
            if (funcSprint) slideTimer = 0.8f;
            slideResetTimer = slideResetTime;
        }

    }

    public void SlideEnd(InputSystem.InputAction.CallbackContext ctx)
    {
        if (crouchToggle) return;
        //if (characterController.isGrounded) forceFactor = 0;
        isSliding = false;
        if (forceFactor > 1) slideEnd = true;
        isCrouching = false;
        crouchPress = false;
        slideTimer = 0;

        sprintSlide = false;
        walkSlide = false;
        if (characterController.isGrounded) slideCancelTimer = 2;

        if (IsOwner && !slopeSlideScript.isCrouchSlopeSliding) {
            legRight.SetActive(false);
            legLeft.SetActive(false);
            torso.SetActive(false);
        }
    }

    void EndSlide()
    {
        //if (characterController.isGrounded) forceFactor = 0;
        isSliding = false;
        if (forceFactor > 1) slideEnd = true;
        isCrouching = false;
        crouchPress = false;
        slideTimer = 0;

        sprintSlide = false;
        walkSlide = false;
        if (characterController.isGrounded) slideCancelTimer = 2;

        if (IsOwner && !slopeSlideScript.isCrouchSlopeSliding) {
            legRight.SetActive(false);
            legLeft.SetActive(false);
            torso.SetActive(false);
        }
    }
    bool isSlideSprinting;

    [ServerRpc (RunLocally = true)]
    void ChangeSlideClipServer(int index)
    {
        ChangeSlideClipObservers(index);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void ChangeSlideClipObservers(int index)
    {
        if (index == 0) slideAudio.clip = groundSlideClip;
        if (index == 1) slideAudio.clip = wallSlideClip;

    }

    private void HandleSlide()
    {
        if (!CanWallJump) SlideFadeOut();

        if (slopeSlideScript.isCrouchSlopeSliding) 
        {
            legRight.SetActive(true);
            legLeft.SetActive(true);
        }

        if (slopeSlideScript.slopeSlideTrigger && slopeSlideScript.isCrouchSlopeSliding)
        {
            slopeSlideScript.slopeSlideTrigger = false;
            StartCoroutine(ActiveTorso());
        }

        if (!isCrouching && move.ReadValue<Vector2>() == Vector2.zero) return;
        if (slideTimer > 0 && slideDownRay && moveDirection.y < 0 && isCrouching && isSlideSprinting)
        {
            ChangeSlideClipServer(0);
            isSliding = true;
            slideTimer = 0;
            slideCancelTimer = -1;
            slideAudio.volume = 1 * audio.volume;

            if (!slopeSlideScript.sprintSlideTrigger)
                SlideAudioPlay();
            StartCoroutine(ActiveTorso());
            
            if (isSprinting) {
                sprintSlide = true;
                walkSlide = false;
            }
            else{
                sprintSlide = false;
                walkSlide = true;
            }
            AddHorizontalForce((isSprinting ? sprintSlideImpulsion : walkSlideImpulsion) * transform.forward, (isSprinting ? sprintSlideDuration : walkSlideDuration));
        }

        if (forceFactor < 0.1f && isSliding)
        {
            if (pauseManager.gamepad)
                EndSlide();
        }

        if (forceFactor == 0 || !slideDownRay) {
            isSliding = false;
            sprintSlide = false;
            walkSlide = false;
            slideCancelTimer = 2;

            if (IsOwner && !slopeSlideScript.isCrouchSlopeSliding) {
                legRight.SetActive(false);
                legLeft.SetActive(false);
                torso.SetActive(false);
            }
        }

        slideTime -= Time.deltaTime;

        if (isSliding)
        {
            if (slideTime < 0)
            {
                slideTime = 0.2f;
                SpawnVFX(transform.position + transform.forward + transform.right, Quaternion.identity);
                SpawnVFX(transform.position + transform.forward - transform.right, Quaternion.identity);
            }
            
        }
    }
    
    private void HandleFootsteps()
    {
        if (!characterController.isGrounded || isSliding) return;
        if (currentInputRaw == Vector2.zero || playerSpeed < 1) return;
        if (isLeaning || isCrouching) return;

        footstepTimer -= Time.deltaTime * movementFactor;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
            {
                switch(hit.collider.tag)
                {
                    case "Footsteps/Concrete/Default":
                        PlaySoundServer(1);
                        break;
                    case "Footsteps/Concrete/Dalles":
                        PlaySoundServer(2);
                        break;
                    case "Footsteps/Concrete/Solide":
                        PlaySoundServer(3);
                        break;
                    case "Footsteps/Concrete/PleinAir":
                        PlaySoundServer(20);
                        break;
                    case "Footsteps/Wood/Creux":
                        PlaySoundServer(4);
                        break;
                    case "Footsteps/Dirt":
                        PlaySoundServer(5);
                        break;
                    case "Footsteps/Sand":
                        PlaySoundServer(6);
                        break;
                    case "Footsteps/Grass":
                        PlaySoundServer(7);
                        break;
                    case "Footsteps/Graviers":
                        PlaySoundServer(8);
                        break;
                    case "Footsteps/Water":
                        PlaySoundServer(9);
                        break;
                    case "Footsteps/Metal/Pipe2":
                        PlaySoundServer(12);
                        break;
                    case "Footsteps/Metal/Grille":
                        PlaySoundServer(19);
                        break;
                    case "Footsteps/Metal/Pipe":
                        PlaySoundServer(13);
                        break;
                    case "Footsteps/Matelas":
                        PlaySoundServer(14);
                        break;
                    case "Footsteps/Moquette":
                        PlaySoundServer(15);
                        break;
                    case "Footsteps/Wood/Sec":
                        PlaySoundServer(16);
                        break;
                    default:
                        PlaySoundServer(1);
                        break;
                }
            }
            
            footstepTimer = GetCurrentOffset;
        }
    }

    private Collider[] ladderColliders;
    private string groundTag;

    bool rightHeadCast;
    bool leftHeadCast;

    private void HandlePhysicsCasting()
    {
        shortfrontRay = (Physics.Raycast(transform.position - Vector3.up*characterController.skinWidth, transform.forward, 0.6f));

        rightHeadCast = (Physics.Raycast(transform.position, transform.right, 2f, landLayer));
        leftHeadCast = (Physics.Raycast(transform.position, -transform.right, 2f,landLayer));

        if (!CanWallJump) wallSlideLean = 0;

        var downRaySlopes = Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hitGround, 8f, landLayer);
        groundNormal = hitGround.normal;
        if (downRaySlopes) groundTag = hitGround.transform.gameObject.name;
        
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit vault, 1.5f))
        {
            if (Vector3.Angle(vault.normal, Vector3.up) > 88)
                vaulRayDown = true;
            else 
                vaulRayDown = false;
        }
        else vaulRayDown = false;

        groundSphereCast = Physics.CheckSphere(transform.position, 0.28f, landLayer);


        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit steepSlide1, 1.5f))
        {
            if (Vector3.Angle(steepSlide1.normal, Vector3.up) > 65 && IsSliding)
                steepSlideForward = true;
            else 
                steepSlideForward = false;
        }
        else steepSlideForward = false;

        if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit steepSlide2, 1.5f))
        {
            if (Vector3.Angle(steepSlide2.normal, Vector3.up) > 65 && IsSliding)
                steepSlideBackward = true;
            else 
                steepSlideBackward = false;
        }
        else steepSlideBackward = false;

        ladderColliders = (Physics.OverlapSphere(transform.position + new Vector3(0, ladderDetectionLength, 0), ladderDetectionLength, ladderLayer));

        ladderRay = ladderColliders.Length != 0;

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit vault2, 1.5f))
        {
            if (Vector3.Angle(vault2.normal, Vector3.up) > 88)
                vaulRayUp = true;
            else 
                vaulRayUp = false;
        }
        else vaulRayUp = false;

        vaultRayUpShort = (Physics.Raycast(transform.position + Vector3.up/2, transform.forward, 1.3f));

        downRay = (Physics.Raycast(transform.position, -transform.up, 0.3f));
        animDownRay = (Physics.Raycast(transform.position, -transform.up, 0.6f));
        slideDownRay = (Physics.Raycast(transform.position, -transform.up, 0.8f));

        upSphere = (Physics.Raycast(transform.position + new Vector3(0, characterController.height, 0), transform.up, 0.25f, landLayer));
    }

    private void HandleLadders()
    {
        HandleLadderSounds();

        if (ladderRay && !isCrouching)
        {
            if (move.ReadValue<Vector2>().magnitude > 0 || moveUp.ReadValue<float>() > 0.1f)
            {
                onLadder = true;
                moveDirection.y = ladderSpeed;
            }
            else
                onLadder = false;
        }
        else
            onLadder = false;

        if (isCrouching)
        {
            onLadder = false;
        }

        if (onLadder && Physics.Raycast(transform.position, transform.forward, 1f, ladderLayer) && move.ReadValue<Vector2>().y < 0)
        {
            BForce(-transform.forward, 7, false, true, 3, true);
        }

        if (onLadder && Physics.Raycast(transform.position, -transform.forward, 1f, ladderLayer) && move.ReadValue<Vector2>().y < 0)
        {
            BForce(transform.forward, 7, false, true, 3, true);
        }
    }

    private void HandleLadderSounds()
    {
        if (!onLadder) return;
        if (ladderColliders.Length == 0) return;

        ladderStepTimer -= Time.deltaTime;

        if (ladderStepTimer <= 0)
        {

            switch(ladderColliders[0].tag)
            {
                case "Ladder/Metal":
                    PlaySoundServer(17);
                    break;
                case "Ladder/Chain":
                    PlaySoundServer(18);
                    break;
                default:
                    PlaySoundServer(17);
                    break;
            }
        
            ladderStepTimer = ladderStep;
        }
    }

    //Custom addforce physics function
    public bool addingForce;
    public Vector3 forceAdded;
    public Vector3 force;
    public float forceFactor;
    public float initforceFactor;
    public float forceSpeed = 30;
    public float tempforceSpeed;
    [Range(0f, 100.0f)] public float deceleration = 1;

    public void AddForce(Vector3 tempforce, float tempforceFactor)
    {
        deceleration = 1;
        moveDirection = new Vector3(0,0,0);
        forceFactor = tempforceFactor;
        force = tempforce;
        moveDirection.y = force.y * forceFactor;
        tempforceSpeed = forceSpeed;
        initforceFactor = tempforceFactor;
    } 

    public void AddAccumulatedForce(Vector3 tempforce, float tempforceFactor)
    {
        deceleration = 1;
        forceFactor = tempforceFactor;
        force = tempforce;
        moveDirection.y = force.y * forceFactor;
        tempforceSpeed = forceSpeed;
        initforceFactor = tempforceFactor;
    }  

    public void AddHorizontalForce(Vector3 tempforce, float tempforceFactor)
    {
        deceleration = 1;
        moveDirection = new Vector3(0, moveDirection.y, 0);
        forceFactor = tempforceFactor;
        force = tempforce;
        tempforceSpeed = forceSpeed;
        initforceFactor = tempforceFactor;
    }
    
    public void AddVerticalForce(Vector3 tempforce, float tempforceFactor)
    {
        deceleration = 1;
        moveDirection = new Vector3(moveDirection.x, -1, moveDirection.z);
        forceFactor = tempforceFactor;
        force = tempforce;
        moveDirection.y = force.y * forceFactor;
        tempforceSpeed = forceSpeed;
        initforceFactor = tempforceFactor;
    }

    private void HandleAddingForce()
    {
        if (initforceFactor > 20f && tempforceSpeed < 10f) tempforceSpeed = 10f;
        else if (tempforceSpeed < 2f) tempforceSpeed = 2f;
        if (!characterController.isGrounded)
        {
            if (forceFactor < 0f) forceFactor = 0f;
            if (forceFactor > 0) tempforceSpeed -= Time.deltaTime * initforceFactor / 2;
        }
        else tempforceSpeed = forceSpeed;

        

        forceFactor -= Time.deltaTime * (tempforceSpeed * deceleration);

        if (forceFactor > 0)
        {
            addingForce = true;
            forceAdded = force * forceFactor;
        }
        else
        {
            addingForce = false;
            forceAdded = new Vector3(0,0,0);
            forceFactor = 0;
            force = new Vector3(0,0,0);
        }

        if (forceFactor <= 1) deceleration = 1;
    }

    //Custom physics v.3
    public void CustomAddForce(Vector3 dir, float force)
    {
        customForceScript.AddForce(dir, force);
    }

    // Physics engine rework
    public Vector3 customForceFinal;
    private Vector3 bforcefinal;
    private Vector3 bdirection;
    private float bfactor;
    private float bdecel;
    private bool bstop;
    private bool bgroundStop;
    private bool baircontrol = true;
    private float btimer;
    
    public void BForce(Vector3 dir, float factor, bool vertical, bool stopOnGround, float decel, bool aircontrol)
    {
        //moveDirection = Vector3.zero;
        bgroundStop = false;
        bstop = false;
        btimer = 0;

        bdirection = dir;
        bfactor = factor;
        bdecel = decel;
        bgroundStop = stopOnGround;
        baircontrol = aircontrol;

        if (vertical) moveDirection.y = dir.y * factor;
        
    }

    void HandleBForce()
    {
        if (bfactor > 0)
        {
            bforcefinal = new Vector3(bdirection.x, 0, bdirection.z) * bfactor;
            bfactor -= Time.deltaTime * bdecel;
        }
        if (bfactor < 0) bforcefinal = Vector3.zero;
        if (bfactor <= 0.3f)
        {
            baircontrol = true;
        }

        if (bstop)
        {
            bstop = false;
            bdirection = Vector3.zero;
            bfactor = 0;
            bforcefinal = Vector3.zero;
        }

        if (bgroundStop && characterController.isGrounded && btimer > 0.5f)
        {
            baircontrol = true;
            bdirection = Vector3.zero;
            bfactor = 0;
            bforcefinal = Vector3.zero;
            bgroundStop = false;
        }

        btimer += Time.deltaTime;
    }

    private Settings settings;

    private void HandleTimers()
    {
        aftervaultjumpTimer -= Time.deltaTime;
        slideTimer -= Time.deltaTime;
        slideResetTimer -= Time.deltaTime;
        slideCancelTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
        landTimer -= Time.deltaTime;
        dmgZoneTimer -= Time.deltaTime;

        settings.timeSpentInGame += Time.deltaTime;
    }

    public bool vault;
    private bool vaultActivate;

    private void CheckForVault()
    {
        aftervaultjumpTimer -= Time.deltaTime;

        if (isGrounded) { 
            vaultActivate = true;
        }

        if (safeGrounded || isCrouching) return;

        RaycastHit vaultHit;

        var rayFoots = Physics.Raycast(transform.position, transform.forward, out vaultHit, 1.4f, landLayer) && !isGrounded;
        var rayMiddle = Physics.Raycast(transform.position + Vector3.up * 1.2f, transform.forward, 1.5f, landLayer);
        var rayHead = Physics.Raycast(transform.position + Vector3.up * 1.8f, transform.forward, 2f, landLayer);
        var rayDown = Physics.Raycast(transform.position, -Vector3.up, 1f, landLayer);

        

        if (rayFoots && !rayMiddle && !rayHead && Vector3.Angle(vaultHit.normal, Vector3.up) > vaultMinimumAngle && Vector3.Angle(vaultHit.normal, Vector3.up) < 130 && vaultActivate && move.ReadValue<Vector2>().y > 0 && (moveDirection.y > 0 ? !rayDown : 1 == 1))
        {
            jumped = false;
            slopeSlideJumped = false;
            vaultActivate = false;
            aftervaultjumpTimer = 0.5f;
            playerCamera.transform.DOPunchRotation(new Vector3(6, 0, 3), 0.45f, 0, 3);
            vault = true;
            moveDirection.y = 9;
            BForce(transform.forward, 1.5f, false, true, 3, true);
            VaultClip(vaultHit.transform.tag);
            StartCoroutine(DeactivateVault());
        }
    }

    IEnumerator DeactivateVault()
    {
        yield return new WaitForSeconds(0.15f);
        cameraScript.rotateBack = true;
        if (downRay && !jumped) moveDirection.y = -5;
        else if (!jumped) moveDirection.y = 0;
        StartCoroutine(RotateBackAfterVault());
    }

    IEnumerator RotateBackAfterVault()
    {
        yield return new WaitForSeconds(0.3f);
        cameraScript.rotateBack = true;
        vault = false;
    }


    void OnGUI()
    {
        
        
        //GUI.Label(new Rect(10, 5, 300, 20), "press 'v' to switch map");
        //GUI.Label(new Rect(10, 20, 300, 20), "press 'f' to respawn");
    }

    private void OnDrawGizmos()
    
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position + new Vector3(0, characterController.height, 0), 0.15f);
    }

    private void SlideFadeOut()
    {
        if (slideCancelTimer > 0)
        {
            slideAudio.volume = Mathf.Lerp(slideAudio.volume, 0, 5 * Time.deltaTime);
        }

        if (slideAudio.volume < 0.03f)
        {
            SlideAudioStop();
        }
    }

    [ServerRpc]
    private void SlideAudioPlay()
    {
        networkSlideAudio.Play();
    }

    [ServerRpc]
    private void SlideAudioStop()
    {
        networkSlideAudio.Stop();
    }

    [ServerRpc (RunLocally = true)]
    private void PlaySoundServer(int clip)
    {
        PlaySoundObservers(clip);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void PlaySoundObservers(int clip)
    {
        if (clip == 0)
            audio.PlayOneShot(jumpClip);

        if (clip == 1) {
            audio.PlayOneShot(concreteClips[0]);
            Shuffle(concreteClips);
        }
        if (clip == 2) {
            audio.PlayOneShot(dallesClips[0]);
            Shuffle(dallesClips);
        }
        if (clip == 3) {
            audio.PlayOneShot(betonSolideClips[0]);
            Shuffle(betonSolideClips);
        }
        if (clip == 4) {
            audio.PlayOneShot(woodClips[0]);
            Shuffle(woodClips);
        }
        if (clip == 5) {
            audio.PlayOneShot(dirtClips[0]);
            Shuffle(dirtClips);
        }
        if (clip == 6) {
            audio.PlayOneShot(sandClips[0]);
            Shuffle(sandClips);
        }
        if (clip == 7) {
            audio.PlayOneShot(grassClips[0]);
            Shuffle(grassClips);
        }
        if (clip == 8) {
            audio.PlayOneShot(graviersClips[0]);
            Shuffle(graviersClips);
        }
        if (clip == 9) {
            audio.PlayOneShot(waterClips[0]);
            Shuffle(waterClips);
        }
        if (clip == 12) {
            audio.PlayOneShot(metalClips[0]);
            Shuffle(metalClips);
        }
        if (clip == 13) {
            audio.PlayOneShot(pipeClips[0]);
            Shuffle(pipeClips);
        }
        if (clip == 14) {
            audio.PlayOneShot(matelasClips[0]);
            Shuffle(matelasClips);
        }
        if (clip == 15) {
            audio.PlayOneShot(moquetteClips[0]);
            Shuffle(moquetteClips);
        }
        if (clip == 16) {
            audio.PlayOneShot(woodSecClips[0]);
            Shuffle(woodSecClips);
        }
        if (clip == 19) {
            audio.PlayOneShot(grilleClips[0]);
            Shuffle(grilleClips);
        }
        if (clip == 20) {
            audio.PlayOneShot(betonPleinairClips[0]);
            Shuffle(betonPleinairClips);
        }

        //Ladders
        if (clip == 17)
            audio.PlayOneShot(ladderClips[(int)Mathf.RoundToInt(Random.Range(0, ladderClips.Length ))]);
        if (clip == 18)
            audio.PlayOneShot(chainClips[(int)Mathf.RoundToInt(Random.Range(0, chainClips.Length))]);

        //Big fall
        if (clip == 21)
            audio.PlayOneShot(highFallAudioClip);


        if (clip == 10) JumpClip();
        if (clip == 11) JumpClip();
        if (clip == 22) audio.PlayOneShot(wallOutClip);
        if (clip == 23) audio.PlayOneShot(teleportClip);
        
    }

    //[ServerRpc (RunLocally = true)]
    void JumpClip()
    {
        JumpClipObservers();
    }

    //[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void JumpClipObservers()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            switch(hit.collider.tag)
            {
                case "Footsteps/Concrete/Default":
                    audio.PlayOneShot(concreteClips[(int)Mathf.RoundToInt(Random.Range(0, concreteClips.Length))]);
                    break;
                case "Footsteps/Concrete/Dalles":
                    audio.PlayOneShot(dallesClips[(int)Mathf.RoundToInt(Random.Range(0, dallesClips.Length))]);
                    break;
                case "Footsteps/Concrete/Solide":
                    audio.PlayOneShot(betonSolideClips[(int)Mathf.RoundToInt(Random.Range(0, betonSolideClips.Length))]);
                    break;
                case "Footsteps/Concrete/PleinAir":
                    audio.PlayOneShot(betonPleinairClips[(int)Mathf.RoundToInt(Random.Range(0, betonPleinairClips.Length))]);
                    break;
                case "Footsteps/Wood/Creux":
                    audio.PlayOneShot(woodClips[(int)Mathf.RoundToInt(Random.Range(0, woodClips.Length))]);
                    break;
                case "Footsteps/Wood/Sec":
                    audio.PlayOneShot(woodSecClips[(int)Mathf.RoundToInt(Random.Range(0, woodSecClips.Length))]);
                    break;
                case "Footsteps/Dirt":
                    audio.PlayOneShot(dirtClips[(int)Mathf.RoundToInt(Random.Range(0, dirtClips.Length))]);
                    break;
                case "Footsteps/Sand":
                    audio.PlayOneShot(sandClips[(int)Mathf.RoundToInt(Random.Range(0, sandClips.Length))]);
                    break;
                case "Footsteps/Grass":
                    audio.PlayOneShot(grassClips[(int)Mathf.RoundToInt(Random.Range(0, grassClips.Length))]);
                    break;
                case "Footsteps/Graviers":
                    audio.PlayOneShot(graviersClips[(int)Mathf.RoundToInt(Random.Range(0, graviersClips.Length))]);
                    break;
                case "Footsteps/Water":
                    audio.PlayOneShot(waterClips[(int)Mathf.RoundToInt(Random.Range(0, waterClips.Length))]);
                    break;
                case "Footsteps/Metal/Pipe2":
                    audio.PlayOneShot(metalClips[(int)Mathf.RoundToInt(Random.Range(0, metalClips.Length))]);
                    break;
                case "Footsteps/Metal/Grille":
                    audio.PlayOneShot(grilleClips[(int)Mathf.RoundToInt(Random.Range(0, grilleClips.Length))]);
                    break;
                case "Footsteps/Metal/Pipe":
                    audio.PlayOneShot(pipeClips[(int)Mathf.RoundToInt(Random.Range(0, pipeClips.Length))]);
                    break;
                case "Footsteps/Matelas":
                    audio.PlayOneShot(matelasClips[(int)Mathf.RoundToInt(Random.Range(0, matelasClips.Length))]);
                    break;
                case "Footsteps/Moquette":
                    audio.PlayOneShot(moquetteClips[(int)Mathf.RoundToInt(Random.Range(0, moquetteClips.Length))]);
                    break;
                default:
                    audio.PlayOneShot(concreteClips[(int)Mathf.RoundToInt(Random.Range(0, concreteClips.Length))]);
                    break;
            }
        }
        else
            audio.PlayOneShot(jumpClip);
    }

    [ServerRpc (RunLocally = true)]
    void WallJumpClip(string walltag)
    {
        JumpClipObservers(walltag);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void JumpClipObservers(string walltag)
    {
        switch(walltag)
        {
            case "Footsteps/Concrete/Default":
                audio.PlayOneShot(concreteClips[(int)Mathf.RoundToInt(Random.Range(0, concreteClips.Length))]);
                break;
            case "Footsteps/Concrete/Dalles":
                audio.PlayOneShot(dallesClips[(int)Mathf.RoundToInt(Random.Range(0, dallesClips.Length))]);
                break;
            case "Footsteps/Concrete/Solide":
                audio.PlayOneShot(betonSolideClips[(int)Mathf.RoundToInt(Random.Range(0, betonSolideClips.Length))]);
                break;
            case "Footsteps/Concrete/PleinAir":
                audio.PlayOneShot(betonPleinairClips[(int)Mathf.RoundToInt(Random.Range(0, betonPleinairClips.Length))]);
                break;
            case "Footsteps/Wood/Creux":
                audio.PlayOneShot(woodClips[(int)Mathf.RoundToInt(Random.Range(0, woodClips.Length))]);
                break;
            case "Footsteps/Wood/Sec":
                audio.PlayOneShot(woodSecClips[(int)Mathf.RoundToInt(Random.Range(0, woodSecClips.Length))]);
                break;
            case "Footsteps/Dirt":
                audio.PlayOneShot(dirtClips[(int)Mathf.RoundToInt(Random.Range(0, dirtClips.Length))]);
                break;
            case "Footsteps/Sand":
                audio.PlayOneShot(sandClips[(int)Mathf.RoundToInt(Random.Range(0, sandClips.Length))]);
                break;
            case "Footsteps/Grass":
                audio.PlayOneShot(grassClips[(int)Mathf.RoundToInt(Random.Range(0, grassClips.Length))]);
                break;
            case "Footsteps/Graviers":
                audio.PlayOneShot(graviersClips[(int)Mathf.RoundToInt(Random.Range(0, graviersClips.Length))]);
                break;
            case "Footsteps/Water":
                audio.PlayOneShot(waterClips[(int)Mathf.RoundToInt(Random.Range(0, waterClips.Length))]);
                break;
            case "Footsteps/Metal/Pipe2":
                audio.PlayOneShot(metalClips[(int)Mathf.RoundToInt(Random.Range(0, metalClips.Length))]);
                break;
            case "Footsteps/Metal/Grille":
                audio.PlayOneShot(grilleClips[(int)Mathf.RoundToInt(Random.Range(0, grilleClips.Length))]);
                break;
            case "Footsteps/Metal/Pipe":
                audio.PlayOneShot(pipeClips[(int)Mathf.RoundToInt(Random.Range(0, pipeClips.Length))]);
                break;
            case "Footsteps/Matelas":
                audio.PlayOneShot(matelasClips[(int)Mathf.RoundToInt(Random.Range(0, matelasClips.Length))]);
                break;
            case "Footsteps/Moquette":
                audio.PlayOneShot(moquetteClips[(int)Mathf.RoundToInt(Random.Range(0, moquetteClips.Length))]);
                break;
            default:
                audio.PlayOneShot(concreteClips[(int)Mathf.RoundToInt(Random.Range(0, concreteClips.Length))]);
                break;
        }
    }

    [ServerRpc (RunLocally = true)]
    void VaultClip(string surface)
    {
        VaultClipObservers(surface);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void VaultClipObservers(string surface)
    {
        switch(surface)
        {
            case "Footsteps/Concrete/Default":
                audio.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                break;
            case "Footsteps/Concrete/Dalles":
                audio.PlayOneShot(dallesClips[Random.Range(0, dallesClips.Length - 1)]);
                break;
            case "Footsteps/Concrete/Solide":
                audio.PlayOneShot(betonSolideClips[Random.Range(0, betonSolideClips.Length - 1)]);
                break;
            case "Footsteps/Concrete/PleinAir":
                audio.PlayOneShot(betonPleinairClips[Random.Range(0, betonPleinairClips.Length - 1)]);
                break;
            case "Footsteps/Wood/Creux":
                audio.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                break;
            case "Footsteps/Wood/Sec":
                audio.PlayOneShot(woodSecClips[Random.Range(0, woodSecClips.Length - 1)]);
                break;
            case "Footsteps/Dirt":
                audio.PlayOneShot(dirtClips[Random.Range(0, dirtClips.Length - 1)]);
                break;
            case "Footsteps/Sand":
                audio.PlayOneShot(sandClips[Random.Range(0, sandClips.Length - 1)]);
                break;
            case "Footsteps/Grass":
                audio.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                break;
            case "Footsteps/Graviers":
                audio.PlayOneShot(graviersClips[Random.Range(0, graviersClips.Length - 1)]);
                break;
            case "Footsteps/Water":
                audio.PlayOneShot(waterClips[Random.Range(0, waterClips.Length - 1)]);
                break;
            case "Footsteps/Metal/Pipe2":
                audio.PlayOneShot(metalClips[Random.Range(0, metalClips.Length - 1)]);
                break;
            case "Footsteps/Metal/Grille":
                audio.PlayOneShot(grilleClips[Random.Range(0, grilleClips.Length - 1)]);
                break;
            case "Footsteps/Metal/Pipe":
                audio.PlayOneShot(pipeClips[Random.Range(0, pipeClips.Length - 1)]);
                break;
            case "Footsteps/Matelas":
                audio.PlayOneShot(matelasClips[Random.Range(0, matelasClips.Length - 1)]);
                break;
            case "Footsteps/Moquette":
                audio.PlayOneShot(moquetteClips[Random.Range(0, moquetteClips.Length - 1)]);
                break;
            default:
                audio.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                break;
        }
        
    }

    [ServerRpc (RunLocally = true)]
    void SpawnVFX(Vector3 position, Quaternion rotation)
    {
        SpawnVFXObservers(position, rotation);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void SpawnVFXObservers(Vector3 position, Quaternion rotation)
    {
        Instantiate(slideDust, position, rotation);
    }

    private bool roundHasStarted = false;
    private void roundStartEvent() { roundHasStarted = true; }
    
    private void OnTriggerEnter(Collider col) {
        if (!roundHasStarted) { return; }
        
        if (col.CompareTag("Teleport"))
        {
            Teleporter teleporter = col.GetComponent<Teleporter>();
            Transform tp = teleporter.teleportPoint;
            Teleport(tp.position, teleporter.anglesDifference, true, tp, teleporter.propulsionPower, teleporter.propulsionDecel, teleporter.dontTranslateRotation);
        }

        if (col.CompareTag("Killz"))
        {
            Settings.Instance.IncreaseSuicidesAmount();
            GetComponent<PlayerHealth>().fellVoid = true;
            DespawnObject(gameObject);
        }

        /*if (col.CompareTag("MovingObject"))
        {
            entered = true;
            onMovingPlatform = true;
            networkObject.SetParent(col.transform.GetComponentInParent<NetworkBehaviour>());
            objectCollisionMoveDirection = transform.forward * 0.01f;
            CmdChangeRootMotion(false);
        }*/
    }

    private bool entered;

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("MovingObject") )
        {
            if (!onMovingPlatform && isGrounded) {
                //entered = false;
                networkObject.SetParent(col.transform.GetComponentInParent<NetworkBehaviour>());
                objectCollisionMoveDirection = transform.forward * 0.01f;
                onMovingPlatform = true;
                CmdChangeRootMotion(false);
            }
        }

        if (col.CompareTag("DamageZone") && isTouchingAnything) {
            var dmgZone = col.GetComponentInParent<DamageZone>();
            if (dmgZone != null && dmgZoneTimer < 0)
            {
                if (GetComponent<PlayerHealth>().health - dmgZone.damageAmount <= 0) {
                    Settings.Instance.IncreaseSuicidesAmount();
                    GetComponent<PlayerHealth>().suicide = true;
                    DespawnObject(gameObject);
                }
                GetComponent<PlayerHealth>().RemoveHealth(dmgZone.damageAmount);
                dmgZoneTimer = dmgZone.damageInterval; 
            }
        }
    }

    [SerializeField] private float movingPlatformEjectForce = 2;

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("MovingObject") && onMovingPlatform)
        {
            networkObject.UnsetParent();
            if (col.transform.GetComponentInParent<MovingPlatformParent>().doesEject) CustomAddForce(col.transform.GetComponentInParent<MovingPlatformParent>().movingVector, movingPlatformEjectForce);
            objectCollisionMoveDirection = Vector3.zero;
            onMovingPlatform = false;
            CmdChangeRootMotion(true);
        }
    }

    public void SetNetworkParent(bool set, Transform t)
    {
        if (!set) networkObject.UnsetParent();
        else networkObject.SetParent(t.GetComponent<NetworkBehaviour>());

    }

    [ServerRpc]
    public void DespawnObject(GameObject obj)
    {
        transform.DOKill();
        
        GetComponent<PlayerHealth>().health = -8;
        GetComponent<PlayerHealth>().isKilled = true;
        ExplodeOnDeath();
    }

    [ObserversRpc]
    private void ExplodeOnDeath()
    {
        GetComponent<PlayerHealth>().Explode(false, false, "", -transform.forward, 30, transform.position + Vector3.up * 2 + Vector3.right);
    }

    public void Teleport(Vector3 position, float angle, bool boost, Transform cac, float power, float decel, bool dontTranslateRotation)
    {
        
        transform.position = position;
        if (!dontTranslateRotation) transform.eulerAngles -= new Vector3(0, angle -180, 0);

        if (boost) BForce(cac.forward, power, true, false, decel, true);

        if (!dontTranslateRotation) {
            dirForward = transform.TransformDirection(Vector3.forward);
            dirRight = transform.TransformDirection(Vector3.right);
        }

        PlaySoundServer(23);
        

    }

    public void KillShockWave()
    {
        Settings.Instance.IncreaseKillsAmount();
        lensDistortion.intensity.value = killShockWaveStrength;
        colorGrading.saturation.value = -100;
    }

    [ServerRpc (RunLocally = true)]
    public void BreakGlassServer(Vector3 hitPoint, Vector3 direction, GameObject obj)
    {
        BreakGlassObservers(hitPoint, direction, obj);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void BreakGlassObservers(Vector3 hitPoint, Vector3 direction, GameObject obj)
    {
        if (obj.GetComponent<ShatterableGlass>() != null) obj.GetComponent<ShatterableGlass>().Shatter3D(hitPoint, direction);
    }

    public void Shuffle(AudioClip[] texts)
    {
        AudioClip firstText = texts[0];
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)

        int r = Random.Range(1, texts.Length);
        texts[0] = texts[r];
        texts[r] = firstText;
        
    }

    [SerializeField] private VoiceStream vstream;
    [SerializeField] private VoiceRecorder vstreamRecorder;
    
    [ServerRpc]
    public void PlayVoiceChat(byte[] data)
    {
        PlayVoiceChatObservers(data);
    }

    [ObserversRpc]
    public void PlayVoiceChatObservers(byte[] data)
    {
        if (!IsOwner) vstream.PlayVoiceData(data);
    }

    void VoiceChat() {
        if (pauseManager.chatting) { return; }
        
        if (!Settings.Instance.enableVoiceChat) {
            vstreamRecorder.IsRecording = false;
            pauseManager.isRecording = false;
            return;
        }

        switch (Settings.Instance.voiceChatMode) {
            case VoiceChatMode.ToggleMute: {
                if (!record.WasPressedThisFrame()) { return; }
                vstreamRecorder.IsRecording = !vstreamRecorder.IsRecording;
                pauseManager.isRecording = vstreamRecorder.IsRecording;
                break;
            }
            case VoiceChatMode.OpenMic: {
                vstreamRecorder.IsRecording = true;
                pauseManager.isRecording = true;
                break;
            }
            case VoiceChatMode.PushToTalk: {
                if (record.ReadValue<float>() > 0.1f) {
                    vstreamRecorder.IsRecording = true;
                    pauseManager.isRecording = true;
                } else {
                    vstreamRecorder.IsRecording = false;
                    pauseManager.isRecording = false;
                }
                break;
            }
        }

        if (IsOwner) { // I think this is pointless but scope is so so so so so convuluted in this project so I don't klnowdaawuhgdawhjdiowhdaui
            if (ClientInstance.Instance.IsTalking != vstreamRecorder.IsRecording) {
                ClientInstance.Instance.SetTalking(vstreamRecorder.IsRecording);
            }
        }
    }

    
    // again did not know a good place to put this, and I needed to be able to access it from other scripts
    public MatchPoitnsHUD matchPoitnsHUD;
}
