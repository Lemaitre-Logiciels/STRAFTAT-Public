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

public class Taser : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float chargeTime;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private float stunTime;
    [SerializeField] private Vector3 boxdimensions;

    [Space]
    [SerializeField] private MeshRenderer light;
    [SerializeField] private Material chargingMat;
    [SerializeField] private Material readyMat;
    [SerializeField] private AudioSource chargingAudio;
    [SerializeField] private GameObject readyVfx;
    float fireTimer;

    private void Update()
    {
        WeaponUpdate();
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        //If not in hands
        if (gameObject.layer == 7) return;


        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && currentAmmo > 0) 
            Fire();

        if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && currentAmmo > 0)
            Fire();

        if (fireTimer < 0 && fireTimer > -1)
        {
            fireTimer = -2;
            chargingAudio.Stop();
            Material[] tempMats = new Material[1];
            tempMats[0] = readyMat;
            light.materials = tempMats;
            SetVfxActive(true);
        }
    


    }

    [ServerRpc (RunLocally = true)]
    void SetVfxActive(bool active)
    {
        SetVfxActiveObservers(active);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void SetVfxActiveObservers(bool active)
    {
        readyVfx.SetActive(active);
    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (fireTimer > 0) {
            return;
        }

        if (!reloadWeapon && currentAmmo <=0) 
        {
            audio.PlayOneShot(nobulletClip);
            noAmmoClicks ++;
        }
        chargingAudio.Play();
        Material[] tempMats = new Material[1];
        tempMats[0] = chargingMat;
        light.materials = tempMats;
        SetVfxActive(false);
        ShootServer(damage, cam.transform.position, cam.transform.forward);
        fireTimer = timeBetweenFire;
    }

    private PlayerHealth enemyHealth;

    private void ShootServer(float damageToGive, Vector3 position, Vector3 direction)
    {
        enemyHealth = null; 
        ShootServerEffect();

        Collider[] phColl = Physics.OverlapBox(position + direction * (boxdimensions.z/2), boxdimensions, Quaternion.LookRotation(direction), playerLayer);

        Transform hit = null;

        foreach (var ph in phColl)
        {
            if (ph.GetComponentInParent<PlayerHealth>() != null) {
                enemyHealth = ph.GetComponentInParent<PlayerHealth>();
                hit = ph.transform;
                break;
            } 
        }

        if (enemyHealth != null)
        {
            Settings.Instance.taserShots ++; 
            if (enemyHealth.gameObject == transform.root.gameObject) 
                return;

            TaserEnemy(enemyHealth);
                
        }

    }

    [ServerRpc]
    void TaserEnemy(PlayerHealth enemyHealth)
    {
        enemyHealth.controller.canMove = false;

        TaserEnemyTarget(enemyHealth.playerValues.playerClient.transform.GetComponent<NetworkObject>().Owner, enemyHealth);

    }

    [TargetRpc]
    void TaserEnemyTarget(NetworkConnection conn, PlayerHealth enemyHealth)
    {
        
        enemyHealth.StartCoroutine(enemyHealth.UnfreezePlayer(stunTime));
    }

    [ServerRpc (RunLocally = true)]
    private void ShootServerEffect()
    {
        ShootObserversEffect();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect()
    {
        audio.PlayOneShot(fireClip);
     
        if (Settings.Instance.reduceVFX) { return; }
        
        var flash = Instantiate(muzzleFlash, muzzleFlashPoint.position, shootPoint.rotation, (behaviour.vfxAttachedOnGun ? transform : null));
        flash.GetComponent<ParticleSystem>().Play();
    }

    [ServerRpc (RunLocally = true)]
    private void SpawnBulletTrailServer(Vector3 hitPoint)
    {
        SpawnBulletTrail(hitPoint);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void SpawnBulletTrail(Vector3 hitPoint)
    {
        GameObject bulletTrailEffect = Instantiate(bulletTrailLocal.gameObject, shootPoint.position, Quaternion.identity);

        LineRenderer lineRenderer = bulletTrailEffect.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, shootPoint.position);
        lineRenderer.SetPosition(1, hitPoint);

        Destroy(bulletTrailEffect, 0.4f);
    }

    [ServerRpc (RunLocally = true)]
    private void SpawnVFXServer(int index, Vector3 hitPoint, Vector3 hitNormal)
    {
        SpawnVFX(index, hitPoint, hitNormal);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void SpawnVFX(int index, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (Settings.Instance.reduceVFX) { return; }

        if (index == 0)
            Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal));
        if (index == 1)
            Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal));
        if (index == 2)
            Instantiate(bodyImpact, hitPoint, Quaternion.identity);
    }
    

}
