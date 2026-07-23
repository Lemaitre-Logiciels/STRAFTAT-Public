using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;


public class FlashLight : Weapon
{
    [SerializeField] private float reloadTime;
    [SerializeField] private AudioClip reloadClip;
    float fireTimer;
    bool touched;
    Vector3 spread;
    [SerializeField] private GameObject light;
    bool isOn;

    private void Update()
    {
        WeaponUpdate();

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        //If not in hands
        if (gameObject.layer == 7) return;

        //Shoot
        if (!onePressShoot)
        {
            if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && (!reloadWeapon ? currentAmmo >0 : 1 == 1)) 
                Fire();

            if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && (!reloadWeapon ? currentAmmo >0 : 1 == 1))
                Fire();
    
            if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && akAnim && currentAmmo > 0) 
                camAnimScript.rotateBack = false;
            else if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && akAnim && currentAmmo > 0) 
                camAnimScript.rotateBack = false;
            else 
                camAnimScript.rotateBack = true;

            if (!reloadWeapon && currentAmmo <=0)
            {
                if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand) 
                    isClicked = true;

                if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && inLeftHand) 
                    isClicked = true;
            }
        }
        else
        {
            if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand) 
                isClicked = true;

            if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && inLeftHand) 
                isClicked = true;

            camAnimScript.rotateBack = true;
        }

        if (isClicked)
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

        if (shot && timeBetweenFire < 0.2f && onePressShoot) Fire();
            
        

    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (fireTimer > 0) {
            shot = true;
            return;
        }
        shot = false;

        fireTimer = timeBetweenFire;    

        if (!reloadWeapon && currentAmmo <=0) 
        {
            audio.PlayOneShot(nobulletClip);
            noAmmoClicks ++;
        }

        if (!reloadWeapon && currentAmmo <= 0) return;

        if (reloadWeapon && isReloading) return;

        
        Power();
    }

    [ServerRpc (RunLocally = true)]
    private void Power()
    {
        PowerObservers();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void PowerObservers()
    {
        audio.PlayOneShot(fireClip);

        light.SetActive(!light.activeSelf);
    }
}
