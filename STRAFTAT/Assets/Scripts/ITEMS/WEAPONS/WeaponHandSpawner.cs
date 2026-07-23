using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using FishNet.Object.Prediction;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class WeaponHandSpawner : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private GameObject objToSpawn;
    [SerializeField] private Transform previewObject;
    [SerializeField] private LayerMask landLayer;
    [SerializeField] private float maxInteractionDistance = 5;
    [SerializeField] private bool canPlaceMaxDistance;
    private float interactionDistance;
    private float interactionDistanceFront;
    [SerializeField] private bool proximityMine;
    [SerializeField] private bool claymore;
    [SerializeField] private bool apmine;
    float fireTimer;
    bool canPlace;
    Vector3 position;
    Quaternion calculatorRot;
    Quaternion rotation;

    private void Update()
    {
        WeaponUpdate();

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        //If not in hands
        if (gameObject.layer == 7) {
            if (previewObject.gameObject.activeSelf)
                previewObject.gameObject.SetActive(false);
            return;
        }

        HandlePlacement();

        //Shoot
        if (!onePressShoot)
        {
            if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand) 
                Fire();

            if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand) 
                Fire();
        }
        else
        {
            if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand) 
                isClicked = true;

            if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && inLeftHand) 
                isClicked = true;
        }

        if (isClicked && (onePressShoot || (!reloadWeapon && currentAmmo <=0)))
        {
            if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand) {
                Fire();
                isClicked = false;
            }

            if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand) {
                Fire();
                isClicked = false;
            }

            
        }

    }

    bool place;
    bool placeDown;

    private void HandlePlacement()
    {
        if (!IsOwner) return;
        
        //Raycastsize
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit cast, maxInteractionDistance, landLayer))
        {
            interactionDistance = cast.distance;
        }
        else{
            interactionDistance = maxInteractionDistance;
        }

        // Not Looking down raycast size
        //Raycastsize
        if (Physics.Raycast(rootObject.transform.position + Vector3.up, rootObject.transform.forward, out RaycastHit castFront, maxInteractionDistance, landLayer))
        {
            interactionDistanceFront = castFront.distance;
        }
        else{
            interactionDistanceFront = maxInteractionDistance;
        }

        place = Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit cast2, interactionDistance + 0.1f, landLayer) && (apmine ? Vector3.Angle(cast.normal, Vector3.up) >= 0 : claymore ? Vector3.Angle(cast.normal, Vector3.up) > 70 : Vector3.Angle(cast.normal, Vector3.up) < 65);
        placeDown = Physics.Raycast(rootObject.transform.position + rootObject.transform.forward * interactionDistanceFront + Vector3.up, -Vector3.up, out RaycastHit castFront2, 2, landLayer) && Vector3.Angle(castFront2.normal, Vector3.up) < 65;
        if (place)
        {
            previewObject.gameObject.layer = 0;

            position = cast2.point;
            previewObject.transform.up = cast.normal;

            previewObject.position = position;
            rotation = (claymore ? Quaternion.LookRotation(cast2.normal) : previewObject.rotation);

            if (!previewObject.gameObject.activeSelf)
                previewObject.gameObject.SetActive(true);

            canPlace = true;
        } 
        else {

            if (canPlaceMaxDistance && placeDown && proximityMine)
            {
                previewObject.gameObject.layer = 0;

                position = castFront2.point;
                previewObject.transform.up = castFront2.normal;

                previewObject.position = position;
                rotation = previewObject.rotation;

                if (!previewObject.gameObject.activeSelf)
                    previewObject.gameObject.SetActive(true);

                canPlace = true;
            }
            else if (previewObject.gameObject.activeSelf) {
                    previewObject.gameObject.SetActive(false);
                canPlace = false;
            }
        }
    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || behaviour.playerPickup.currentEnvironmentInteractable != null) return;

        if (!playerController.IsOwner || !playerController.canMove) return;

        if (fireTimer > 0) return;

        fireTimer = timeBetweenFire;    

        if (currentAmmo <=0) audio.PlayOneShot(nobulletClip);

        if (currentAmmo <= 0) return;
        if (!canPlace) return;

        //Online

        SpawnObject(objToSpawn, position, rotation);
        RemoveAmmo();

        //Local

        CameraAnimation();
        WeaponAnimation();
            
    }

    [ServerRpc (RunLocally = true)]
    void RemoveAmmo()
    {
        currentAmmo --;
    }

    [ServerRpc]
    public void SpawnObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        if (PauseManager.BetweenRounds) { return; }
        
        GameObject spawned = Instantiate(obj, position, rotation);

        ServerManager.Spawn(spawned, base.Owner);


        if (proximityMine || apmine)
        {
            spawned.GetComponent<ProximityMine>()._rootObject = rootObject;
            spawned.GetComponent<ProximityMine>().weapon = this;
        }
        else if (claymore)
        {
            spawned.GetComponent<Claymore>()._rootObject = rootObject;
            spawned.GetComponent<Claymore>().weapon = this;
        }

    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect()
    {
        audio.PlayOneShot(fireClip);
        
        if (Settings.Instance.reduceVFX) { return; }
    }
    
    private void LocalSound(int index)
    {
        if (index == 0)
            audio.PlayOneShot(headHitClip);
    }
    

}
