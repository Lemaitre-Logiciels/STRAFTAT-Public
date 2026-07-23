using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerValues : NetworkBehaviour
{
    [SyncVar] public ClientInstance playerClient;
    PlayerSetup setup;
    public GameObject typingIndicator;

    [SerializeField] private AudioSource voiceChatSource;

    void Start()
    {
        setup = GetComponent<PlayerSetup>();
    }

    void Update(){

        if (!SteamLobby.Instance) return;

        if (!IsOwner) {
            //SyncedItemRaycast();
            if (voiceChatSource.mute != playerClient.voiceChatSource.mute)
                voiceChatSource.mute = playerClient.voiceChatSource.mute;
            
            typingIndicator.SetActive(playerClient.IsTyping);
        }

        if (!setup.hat) return;
        if (!setup.hat.activeSelf) setup.hat.SetActive(true);
    }

    [Space]
    [Header("Grab Sync Fix")]
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float syncFixTimer;
    [SerializeField] private float raycastLength;
    [SerializeField] private Transform head;
    public Interactable currentInteractable;

    void SyncedItemRaycast()
    {
        syncFixTimer -= Time.deltaTime;
        if (Physics.Raycast(head.position, head.forward, out RaycastHit hit, raycastLength, itemLayer))
        {
            if (hit.transform.gameObject.layer == 7) {
                if (hit.transform.GetComponent<Interactable>() != null) {
                    currentInteractable = hit.transform.GetComponent<Interactable>();
                    currentInteractable.canTake = false;
                }
            }
            else {
                if (currentInteractable != null) currentInteractable.canTake = false;
                currentInteractable = null;
            }
        }
        else {
            if (currentInteractable != null) currentInteractable.canTake = true;
            currentInteractable = null;
        }
    }
    
}
