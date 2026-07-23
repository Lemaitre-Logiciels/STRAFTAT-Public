using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Door : InteractEnvironment
{
    [SyncVar] public bool isOpen;
    [SyncVar] public bool previousIsOpen = true;
    private bool trigger;
    bool firstSfx = true;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private float maxRotation = 105;
    [SerializeField] private float doorOpeningTime = 0.33f;
    [SerializeField] private string closeDoor = "close door";
    private AudioSource audio;
    private Tween tween;
    [SyncVar] public Transform localplayer;

    private void Start()
    {
        if (GetComponent<AudioSource>() != null) audio = GetComponent<AudioSource>();

        if (audio != null)
        {
            audio.maxDistance = 34;
            audio.spatialBlend = 1f;
        }
    }

    public override void OnFocus()
    {
        PauseManager.Instance.interactPopup.gameObject.SetActive(true);
        PauseManager.Instance.interactPopup.text = (isOpen ? closeDoor.ToLower() : popupText.ToLower()) + " [" + PauseManager.Instance.InteractPromptLetter.ToLower() + "]";
    }

    public override void OnInteract(Transform player)
    {
        if (timerdoor > 0) return;
        CmdInteract(player);
    }

    public override void OnLoseFocus()
    {
        
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    void CmdInteract(Transform player)
    {
        
        TriggerDoor();
        previousIsOpen = isOpen;
        isOpen = !isOpen;
        localplayer = player;
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void TriggerDoor()
    {
        if (timerdoor > 0) return;
        trigger = true;
    }

    float timerdoor;

    void Update()
    {
        if (audio != null && source != null) {
            if (audio.volume != source.volume)
                audio.volume = source.volume;
        }

        timerdoor -= Time.deltaTime;

        if (trigger)
        {
            trigger = false;
            tween.Kill();

            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = localplayer.position - transform.position;

            float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

            if (isOpen) {
                tween = transform.DOLocalRotate(new Vector3(0, (dot > 0 ? maxRotation : -maxRotation), 0), doorOpeningTime).SetAutoKill(true);
                
                if (timerdoor < 0) {
                    if (audio != null) {
                        audio.PlayOneShot(openClip);
                    }
                    firstSfx = false;
                    timerdoor = 0.1f;
                }
            }
            else {
                tween = transform.DOLocalRotate(new Vector3(0, 0, 0), doorOpeningTime).SetAutoKill(true);
                if (timerdoor < 0) {
                    if (audio != null) {
                        audio.PlayOneShot(openClip);
                    }
                    firstSfx = false;
                    timerdoor = 0.1f;
                }
            }
        }
        
    }

    private AudioSource source;
    // Start is called before the first frame update
    void Awake()
    {
        if (SoundManager.Instance._effectsSource != null) source = SoundManager.Instance._effectsSource;
    }


}
