using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FishNet;
using FishNet.Object;
using LambdaTheDev.NetworkAudioSync;

public class SlopeSlide : NetworkBehaviour
{
    public Vector3 slopeSlideMove;
    public Vector3 steepSlopeSlideMove;
    public bool isCrouchSlopeSliding;
    public bool isSteepSlopeSliding;
    public bool sliding;
    public bool slopeSlideTrigger;
    public bool slopeSlideTrigger2;
    public bool onIce;
    /// <summary>
    /// This prevents the player from jumping while sliding on super ice lol.
    /// </summary>
    public bool onSuperIce;
    public bool onSand;
    public bool onConcrete;
    public bool sprintSlideTrigger;

    private FirstPersonController controller;
    private CharacterController characterController;
    private Slope slopeScript;

    [SerializeField] private NetworkAudioSource networkAudio;
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioClip slideOutClip;

    private bool downRay;
    RaycastHit slopeHit;
    RaycastHit slopeHit1;
    RaycastHit slopeHit2;
    RaycastHit slopeHit3;
    RaycastHit slopeHit4;

    private bool localSlopeSlideTrigger;
    private bool localSlopeSlideStopTrigger;
    private float slideTimer;
    private float slideCancelTimer;
    private float slideFadeInTimer;
    private float initSoundVolume;
    private bool crouchOutTrigger;
    private bool sprintInTrigger;
    private bool sprintLateTrigger;

    public float speedFactor = 0.1f;
    public float walkSpeedFactor = 0.1f;
    public float currentSpeed = 1;
    public float currentAcceleration = 1;
    public float currentDeceleration = 1;
    

    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float concreteSpeed = 1;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float iceSpeed = 2;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float minWalkIceSlideSpeed = 2;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float maxWalkIceSlideSpeed = 4;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float sandSpeed = 1;

    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float concreteFadeOutTime = 2;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float iceFadeOutTime = 5;
    [SerializeField] [BoxGroup("Slope speed by type", centerLabel: true)] private float sandFadeOutTime = 1.5f;

    [SerializeField] [BoxGroup("Slope acceleration by type", centerLabel: true)] private float concreteAcceleration = 1;
    [SerializeField] [BoxGroup("Slope acceleration by type", centerLabel: true)] private float iceAcceleration = 1;
    [SerializeField] [BoxGroup("Slope acceleration by type", centerLabel: true)] private float sandAcceleration = 1;

    [SerializeField] [BoxGroup("Slope deceleration by type", centerLabel: true)] private float concreteDeceleration;
    [SerializeField] [BoxGroup("Slope deceleration by type", centerLabel: true)] private float iceDeceleration;
    [SerializeField] [BoxGroup("Slope deceleration by type", centerLabel: true)] private float sandDeceleration;

    [SerializeField] [BoxGroup("Slide properties", centerLabel: true)] private float firstClamp = 0;
    [SerializeField] [BoxGroup("Slide properties", centerLabel: true)] private float secondClamp = 50;
    [SerializeField] [BoxGroup("Slide properties", centerLabel: true)] private float minSlopeAngle = 13;

    [SerializeField] [BoxGroup("Slide properties", centerLabel: true)] private float walkFirstClamp = 10;
    [SerializeField] [BoxGroup("Slide properties", centerLabel: true)] private float walkSecondClamp = 50;

    [SerializeField] [BoxGroup("Steep Slope Slide", centerLabel: true)] private float minimumSteepness = 65;
    [SerializeField] [BoxGroup("Steep Slope Slide", centerLabel: true)] private float steepSlopeSlideSpeed = 6;
    [SerializeField] [BoxGroup("Steep Slope Slide", centerLabel: true)] private float steepSlopeSlideAcceleration = 4;
    [SerializeField] [BoxGroup("Steep Slope Slide", centerLabel: true)] private float steepSlopeSlideDeceleration = 1;

    void Awake()
    {
        controller = GetComponent<FirstPersonController>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isSprinting && (controller.isCrouching || controller.isSliding)) {
            sprintSlideTrigger = true;
            sprintLateTrigger = true;

        }

        downRay = Physics.Raycast(transform.position, -Vector3.up, out slopeHit, 8f, controller.landLayer);
        /*var downRayFront = Physics.Raycast(transform.position + transform.forward*0.4f, -Vector3.up, out slopeHit1, 8f, controller.landLayer);
        var downRayRight = Physics.Raycast(transform.position + transform.right*0.4f, -Vector3.up, out slopeHit2, 8f, controller.landLayer);
        var downRayLeft = Physics.Raycast(transform.position - transform.forward*0.4f, -Vector3.up, out slopeHit3, 8f, controller.landLayer);
        var downRayBack = Physics.Raycast(transform.position - transform.right*0.4f, -Vector3.up, out slopeHit4, 8f, controller.landLayer);*/
        isCrouchSlopeSliding = controller.safeGrounded && downRay && controller.isCrouching && Vector3.Angle(slopeHit.normal, Vector3.up) > minSlopeAngle && (onIce ? true : !controller.isSprinting && controller.isSliding ? true : sprintSlideTrigger || slopeSlideTrigger2);
        //isSteepSlopeSliding = controller.isGrounded && downRay && (Vector3.Angle(slopeHit1.normal, Vector3.up) > minimumSteepness || Vector3.Angle(slopeHit2.normal, Vector3.up) > minimumSteepness || Vector3.Angle(slopeHit3.normal, Vector3.up) > minimumSteepness || Vector3.Angle(slopeHit4.normal, Vector3.up) > minimumSteepness) && !isCrouchSlopeSliding;

        speedFactor = Mathf.Clamp(Vector3.Angle(slopeHit.normal, Vector3.up), firstClamp, secondClamp) / (secondClamp - firstClamp);
        walkSpeedFactor = Mathf.Clamp(Vector3.Angle(slopeHit.normal, Vector3.up), walkFirstClamp, walkSecondClamp) / (secondClamp - firstClamp);

        // Crouch Slope Sliding
        if (isCrouchSlopeSliding)
        {
            slopeSlideMove = Vector3.Lerp(slopeSlideMove, new Vector3(slopeHit.normal.x, -slopeHit.normal.y, slopeHit.normal.z) * speedFactor * currentSpeed, currentAcceleration * Time.deltaTime);
            localSlopeSlideStopTrigger = true;
        }
        else {
            slopeSlideMove = Vector3.Lerp(new Vector3(slopeSlideMove.x, 0, slopeSlideMove.z), Vector3.zero, currentDeceleration * Time.deltaTime);
            sprintSlideTrigger = false;
            slopeSlideTrigger2 = false;
            localSlopeSlideTrigger = true;
            slopeSlideTrigger = true;
        }

        //Ice and steep slope sliding (when not crouching)
        if (onIce && controller.safeGrounded)
        {
            steepSlopeSlideMove = Vector3.Lerp(steepSlopeSlideMove, new Vector3(slopeHit.normal.x, 0, slopeHit.normal.z) * Mathf.Lerp(minWalkIceSlideSpeed, maxWalkIceSlideSpeed, walkSpeedFactor), currentAcceleration * Time.deltaTime);
        }
        /*else if ((controller.groundSphereCast && Vector3.Angle(Vector3.up, controller.groundNormal) > 65) || controller.IsSliding)
        {
            steepSlopeSlideMove = Vector3.Lerp(steepSlopeSlideMove, new Vector3(slopeHit.normal.x, 0, slopeHit.normal.z) * steepSlopeSlideSpeed * (controller.isSprinting ? 2.1f : controller.isWalking ? 1.8f : 1), steepSlopeSlideAcceleration * Time.deltaTime);
        }*/
        else steepSlopeSlideMove = Vector3.Lerp(new Vector3(steepSlopeSlideMove.x, 0, steepSlopeSlideMove.z), Vector3.zero, steepSlopeSlideDeceleration * Time.deltaTime);

        if (!controller.isCrouching && !onIce) slopeSlideMove = Vector3.zero;

        // Steep Slope Sliding
        /*if (isSteepSlopeSliding)
        {
            steepSlopeSlideMove = Vector3.Lerp(steepSlopeSlideMove, new Vector3(slopeHit.normal.x, -slopeHit.normal.y, slopeHit.normal.z) * steepSlopeSlideSpeed, steepSlopeSlideAcceleration * Time.deltaTime);
            localSlopeSlideStopTrigger = true;
        }
        else {
            steepSlopeSlideMove = Vector3.Lerp(new Vector3(steepSlopeSlideMove.x, 0, steepSlopeSlideMove.z), Vector3.zero, steepSlopeSlideAcceleration * Time.deltaTime);
            sprintSlideTrigger = false;
            slopeSlideTrigger2 = false;
            localSlopeSlideTrigger = true;
            slopeSlideTrigger = true;
        }*/

        if (localSlopeSlideTrigger && sprintSlideTrigger && isCrouchSlopeSliding && sprintLateTrigger)
        {
            sprintLateTrigger = false;
            sprintInTrigger = true;
            localSlopeSlideTrigger = false;
            slideCancelTimer = 2.1f;

            audio.volume = 1;
            initSoundVolume = audio.volume;
            SlideAudioPlay();
            stopped = true;
        }
        else if (localSlopeSlideTrigger && isCrouchSlopeSliding && sprintLateTrigger)
        {
            slopeSlideTrigger2 = true;
            sprintLateTrigger = false;
            sprintInTrigger = true;
            localSlopeSlideTrigger = false;
            slideCancelTimer = 2.1f;
            audio.volume = 1;
            initSoundVolume = audio.volume;
            stopped = true;
        }

        if (controller.isCrouching) crouchOutTrigger = true;

        if (crouchOutTrigger && !controller.isCrouching && audio.volume > 0.1f && sprintInTrigger)
        {
            //StopServer();
            initSoundVolume = audio.volume;
            sprintInTrigger = false;
            crouchOutTrigger = false;
            slideCancelTimer = 0;
        }
        SlideFadeOut(!controller.isCrouching ? 0.2f : onIce ? iceFadeOutTime : onConcrete ? concreteFadeOutTime : sandFadeOutTime);

        //if (Vector3.Angle(slopeHit.normal, Vector3.up) > 30) SlideFadeIn();

        if (localSlopeSlideStopTrigger && !isCrouchSlopeSliding)
        {
            initSoundVolume = audio.volume;
            localSlopeSlideStopTrigger = false;
            slideCancelTimer = 0;
        }

        if (downRay)
        {
            switch (slopeHit.transform.tag)
            {
                case "Footsteps/Ice" :
                    onIce = true;
                    onSuperIce = false;
                    onSand = false;
                    onConcrete = false;
                    currentSpeed = iceSpeed;
                    currentAcceleration = iceAcceleration;
                    currentDeceleration = iceDeceleration;
                    break;
                case "Footsteps/SuperIce" :
                    onIce = true;
                    onSuperIce = true;
                    onSand = false;
                    onConcrete = false;
                    currentSpeed = iceSpeed;
                    currentAcceleration = iceAcceleration;
                    currentDeceleration = iceDeceleration;
                    break;
                case "Footsteps/Sand" :
                    onIce = false;
                    onSuperIce = false;
                    onSand = true;
                    onConcrete = false;
                    currentSpeed = sandSpeed;
                    currentAcceleration = sandAcceleration;
                    currentDeceleration = sandDeceleration;
                    break;
                default :
                    onIce = false;
                    onSuperIce = false;
                    onSand = false;
                    onConcrete = true;
                    currentSpeed = concreteSpeed;
                    currentAcceleration = concreteAcceleration;
                    currentDeceleration = concreteDeceleration;
                    break;
            }
        }
    }

    bool stopped;

    private void SlideFadeOut(float _duration)
    {
        slideCancelTimer += Time.deltaTime;
        if (slideCancelTimer <= _duration)
        {
            slideFadeInTimer = 0;
            audio.volume = Mathf.Lerp(initSoundVolume, 0, slideCancelTimer / _duration);
        }

        if ((audio.volume < 0.05f || (slideCancelTimer > 1 && !controller.isCrouching)) && stopped)
        {
            stopped = false;
            SlideAudioStop();
        }
    }

    [ServerRpc]
    private void SlideAudioPlay()
    {
        networkAudio.Play();
    }

    [ServerRpc]
    private void SlideAudioStop()
    {
        networkAudio.Stop();
    }

    void OnDisable()
    {
        SlideAudioStop();
    }


}
