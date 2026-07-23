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

public class BumpGun : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float launchForce = 12;
    [SerializeField] private float playerKnockback = 2;
    [SerializeField] private BumpBullet _projectile;
    private const float MAX_PASSED_TIME = 0.3f;
    float fireTimer;

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
            if (fire1.ReadValue<float>() > 0.1f && inRightHand) 
                Fire();

            if (fire2.ReadValue<float>() > 0.1f && inLeftHand) 
                Fire();
        }
        else
        {
            if (fire1.ReadValue<float>() < 0.1f && inRightHand) 
                isClicked = true;

            if (fire2.ReadValue<float>() < 0.1f && inLeftHand) 
                isClicked = true;
        }

        if (isClicked && onePressShoot)
        {
            if (fire1.ReadValue<float>() > 0.1f && inRightHand) {
                Fire();
                isClicked = false;
            }

            if (fire2.ReadValue<float>() > 0.1f && inLeftHand) {
                Fire();
                isClicked = false;
            }
        }

    }

    private void Fire()
    {

        if (!playerController.IsOwner || !playerController.canMove) return;

        if (fireTimer > 0) return;

        fireTimer = timeBetweenFire;    

        if (currentAmmo <=0) audio.PlayOneShot(nobulletClip);

        if (currentAmmo <= 0) return;

        currentAmmo --;

        Vector3 position = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        //Online

        SpawnProjectile(position, direction, 0f);
        ServerFire(position, direction, base.TimeManager.Tick);
        playerController.BForce(-cam.transform.forward, playerKnockback, true, false, 4, true);

        //Local

        CameraAnimation();
        WeaponAnimation();
            
    }

    private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime)
    {
        BumpBullet pp = Instantiate(_projectile, position, Quaternion.identity);
        pp.Initialize(direction, launchForce, passedTime, rootObject, gameObject);
    }

    [ServerRpc (RunLocally = true)]
    private void ServerFire(Vector3 position, Vector3 direction, uint tick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);

        passedTime = Mathf.Min(MAX_PASSED_TIME / 2f, passedTime);

        ShootObserversEffect();

        if (!IsServer)
            SpawnProjectile(position, direction, passedTime);

        ObserversFire(position, direction, tick);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ObserversFire(Vector3 position, Vector3 direction, uint tick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);

        passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

        SpawnProjectile(position, direction, passedTime);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect()
    {
        
        audio.PlayOneShot(fireClip);

        if (Settings.Instance.reduceVFX) { return; }
        
        var flash = Instantiate(muzzleFlash, muzzleFlashPoint.position, shootPoint.rotation);
        flash.GetComponent<ParticleSystem>().Play();
    }
    
    private void LocalSound(int index)
    {
        if (index == 0)
            audio.PlayOneShot(headHitClip);
    }
    

}
