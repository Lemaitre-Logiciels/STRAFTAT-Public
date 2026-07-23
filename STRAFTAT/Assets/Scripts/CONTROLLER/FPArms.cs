using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class FPArms : MonoBehaviour
{
    [Header("Jump Animation")]
    [SerializeField] private Vector3 jumpOffset;
    [SerializeField] private Vector3 fallOffset;
    [SerializeField] private float jumpBobSpeed = 0.1f;
    [SerializeField] private float fallBobSpeed = 1f;

    [Header("Movement Animation")]
    [SerializeField] private Transform bobPosition;
    [SerializeField] private float bobScale = 0.3f;
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private float crouchBobScale = 0.7f;
    [SerializeField] private float crouchBobSpeed = 10f;
    [SerializeField] private float sprintBobScale = 0.1f;
    [SerializeField] private float sprintBobSpeed = 2f;
    [SerializeField] private float aimBobScale = 0.4f;
    [SerializeField] private float aimBobSpeed = 0.5f;
    [SerializeField] private float resetRecoverSpeed = 3f;
    private Vector3 restPos;
    private Quaternion restRot;
    private Vector3 targetIdlePos;
    private float IdleScaleTempTimer;
    private float tempBobScaleTimer;

    [Header("Idle Animation")]
    [SerializeField] private float idleSinSpeed = 0.25f;
    [SerializeField] private float idleScale = 0.7f;
    [SerializeField] private float idleLerpSpeed = 10f;
    [SerializeField] private float maxIdleScale = 0.3f;

    [Header("Members")]
    [SerializeField] private Animator animator;

    public PlayerControls playerControls;
    private InputAction move, jump, run, lookY, lookX, zoom, crouch;
    private Vector2 mouseInput;
    bool jumped;
    [HideInInspector] public bool heavy;
    [HideInInspector] public bool vertical;
    float landTimer;
    [SerializeField] private FirstPersonController player;

    private PauseManager pauseManager;

    void Awake()
    {
        playerControls = InputManager.inputActions;
        playerControls.Player.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        playerControls.Player.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

        pauseManager = PauseManager.Instance;
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        lookY = playerControls.Player.MouseY;
        lookY.Enable();

        lookX = playerControls.Player.MouseX;
        lookX.Enable();

        jump = playerControls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;
    }
    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        lookY.Disable();
        lookX.Disable();
        jump.performed -= Jump;
    }

    void Start()
    {
        restPos = transform.localPosition;
        restRot = transform.localRotation;
    }
    
    void Update()
    {
        landTimer -= Time.deltaTime;

        //Idle Bob

        if (!player.isGrounded && jumped && !player.isAiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, restPos + jumpOffset, jumpBobSpeed * Time.deltaTime);
        }
        else if (!player.isGrounded && !Physics.Raycast(player.transform.position, Vector3.down, 0.5f) && !player.isAiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, restPos + fallOffset, fallBobSpeed * Time.deltaTime);
        }
        else if (!player.isWalking && !player.isSprinting && !player.isSliding && player.isGrounded && player.playerSpeed < 0.3f)
        {
            IdleScaleTempTimer += Time.deltaTime;
            var scale = Mathf.Sin(idleSinSpeed * IdleScaleTempTimer) * idleScale;

            scale = Mathf.Clamp(scale, -maxIdleScale, maxIdleScale);

            targetIdlePos = new Vector3(restPos.x, restPos.y + scale, restPos.z);

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetIdlePos, idleLerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0,0,0), resetRecoverSpeed * Time.deltaTime);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, restPos, resetRecoverSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0,0,0), resetRecoverSpeed * Time.deltaTime);
        }

        //Headbob

        if (!player.isGrounded && jumped && !player.isAiming)
        {
            bobPosition.transform.localPosition = Vector3.Lerp(bobPosition.transform.localPosition, restPos, resetRecoverSpeed * Time.deltaTime);
            //transform.localPosition = Vector3.Lerp(transform.localPosition, restPos + jumpOffset, jumpBobSpeed * Time.deltaTime);
        }
        else if (!player.isGrounded && !Physics.Raycast(player.transform.position, Vector3.down, 0.5f) && !player.isAiming)
        {
            bobPosition.transform.localPosition = Vector3.Lerp(bobPosition.transform.localPosition, restPos, resetRecoverSpeed * Time.deltaTime);
            //transform.localPosition = Vector3.Lerp(transform.localPosition, restPos + fallOffset, fallBobSpeed * Time.deltaTime);
        }
        else if (move.ReadValue<Vector2>() != Vector2.zero && !player.isSliding && player.playerSpeed > 1 && player.isAiming && (pauseManager != null ? !pauseManager.pause && (pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) : 1==1))
        {
            tempBobScaleTimer += Time.deltaTime;
            IdleScaleTempTimer = 0;
            var scale = (2 / (3 - Mathf.Cos(2 * Time.time)));
            bobPosition.transform.localPosition = Vector3.Lerp(bobPosition.transform.localPosition, new Vector3(restPos.x, bobPosition.transform.localPosition.y, restPos.z), resetRecoverSpeed * Time.deltaTime);
            bobPosition.transform.localPosition = new Vector3(
                bobPosition.transform.localPosition.x,
                bobPosition.transform.localPosition.y + ((player.isCrouching || player.isLeaning  ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale) * aimBobScale) * (scale * Mathf.Sin(2 * Time.time * ((player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed : bobSpeed) * aimBobSpeed)) / 2* Time.deltaTime),
                bobPosition.transform.localPosition.z);

        }
        else if (move.ReadValue<Vector2>() != Vector2.zero && !player.isSliding && player.playerSpeed > 1 && !player.isAiming && (pauseManager != null ? !pauseManager.pause && (pauseManager.steamPlaying ? !pauseManager.chatting : 1 == 1) : 1==1))
        {
            if (heavy)
            {
                tempBobScaleTimer += Time.deltaTime;
                IdleScaleTempTimer = 0;
                var scale = (2 / (3 - Mathf.Cos(2 * tempBobScaleTimer)));
                bobPosition.transform.localPosition = new Vector3(
                    bobPosition.transform.localPosition.x + (player.isCrouching || player.isLeaning ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale) /2* (scale * Mathf.Sin(tempBobScaleTimer * (player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed : bobSpeed)) * Time.deltaTime),
                    bobPosition.transform.localPosition.y + ((player.isCrouching || player.isLeaning  ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale)+2) * (scale * Mathf.Sin(2 * tempBobScaleTimer * (player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed/3f : bobSpeed/3f)) / 2* Time.deltaTime),
                    bobPosition.transform.localPosition.z);
            }
            else if (vertical)
            {
                tempBobScaleTimer += Time.deltaTime * (player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed : bobSpeed)/6 ;
                IdleScaleTempTimer = 0;
                var scale = (2 / (3 - Mathf.Cos(2 * tempBobScaleTimer)));
                bobPosition.transform.localPosition = new Vector3(
                    bobPosition.transform.localPosition.x,
                    bobPosition.transform.localPosition.y + ((player.isCrouching || player.isLeaning  ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale/10f)+1.7f) * (scale * Mathf.Sin(4* tempBobScaleTimer) / 2* Time.deltaTime),
                    bobPosition.transform.localPosition.z);
            }
            else {
                tempBobScaleTimer += Time.deltaTime;
                IdleScaleTempTimer = 0;
                var scale = (2 / (3 - Mathf.Cos(2 * Time.time)));
                bobPosition.transform.localPosition = new Vector3(
                    bobPosition.transform.localPosition.x + (player.isCrouching || player.isLeaning ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale) * (scale * Mathf.Cos(Time.time * (player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed : bobSpeed)) * Time.deltaTime),
                    bobPosition.transform.localPosition.y + (player.isCrouching || player.isLeaning  ? crouchBobScale : player.isSprinting ? sprintBobScale : bobScale) * (scale * Mathf.Sin(2 * Time.time * (player.isCrouching || player.isLeaning  ? crouchBobSpeed : player.isSprinting ? sprintBobSpeed : bobSpeed)) / 2* Time.deltaTime),
                    bobPosition.transform.localPosition.z);
            }

        }
        else {
            bobPosition.transform.localPosition = Vector3.Lerp(bobPosition.transform.localPosition, restPos, resetRecoverSpeed * Time.deltaTime);
        }

        if ((landTimer < 0 || jumped) && player.isGrounded)
        {
            IdleScaleTempTimer = 0;
            transform.DOLocalMove(restPos, (jumped ? 0.2f : 0.3f)).SetEase(Ease.OutBack);
        }

        if (player.isGrounded && jumped)
        {
            jumped = false;
        }

        if (player.isGrounded)
            landTimer = 0.15f;

        //WeaponSway(restPos, transform);
        if (pauseManager != null ? !pauseManager.pause && !pauseManager.chatting : 1 == 1) TiltSway((player.wallSlideLean > 1 || player.wallSlideLean < -1 ? Quaternion.Euler(0, 0, player.wallSlideLean) :restRot), transform);

    }

    void Jump(InputAction.CallbackContext ctx)
    {
        if (Physics.Raycast(player.transform.position, Vector3.down, 1.3f))
            jumped = true;
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

    private float InputX;
    private float InputY;

    private void WeaponSway(Vector3 initialPosition, Transform member)
    {

        InputX = -Input.GetAxis("Mouse X");
        InputY = -Input.GetAxis("Mouse Y");

        float moveX = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
        float moveY = Mathf.Clamp(InputY * amount, -maxAmount, maxAmount);
        
        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        member.position = Vector3.Lerp(member.localPosition, initialPosition + finalPosition, Time.deltaTime * smoothAmount);
    }

    private void TiltSway(Quaternion initialRotation, Transform member)
    {
        float tiltY = Mathf.Clamp(-Input.GetAxis("Mouse X") * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = Mathf.Clamp(Input.GetAxis("Mouse Y") * rotationAmount, -maxRotationAmount, maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(rotationX ? tiltX : 0f, rotationY ? tiltY : 0f, rotationZ ? tiltY : 0f));

        member.localRotation = Quaternion.Slerp(member.localRotation, finalRotation * initialRotation, Time.deltaTime * smoothRotation);
    }
    
}
