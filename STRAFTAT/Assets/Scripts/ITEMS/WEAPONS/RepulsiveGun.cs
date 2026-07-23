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

public class RepulsiveGun : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float reloadTime;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private float repulseForce = 3;
    [SerializeField] private float playerKnockback = 0;
    [SerializeField] private Vector3 boxdimensions;
    float fireTimer;
    bool touched;
    Vector3 spread;

    void Start()
    {
        chargedBullets = ammoCharge;
        currentAmmo -= ammoCharge;
    }

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

        if (reloadWeapon && chargedBullets <= 0 && currentAmmo > 0) StartCoroutine(Reload());
        if (reloadWeapon && chargedBullets <= 0 && currentAmmo <= 0) audio.PlayOneShot(nobulletClip);
        if (reloadWeapon && chargedBullets <= 0) return;

        ShootServer(damage, cam.transform.position, cam.transform.forward);

        if (revolverShake) CameraRevolverAnimation();
        else CameraAnimation();
        WeaponAnimation();

        if (playerKnockback != 0) playerController.AddForce(-cam.transform.forward, playerKnockback);
            

            
    }

    bool hitOK;

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

            if (enemyHealth.gameObject == transform.root.gameObject) 
                return;

            BumpPlayerServer(direction + Vector3.up * 2, repulseForce, enemyHealth);
            hitOK = true;
                
            
        }
    }

    [ServerRpc]
    private void BumpPlayerServer(Vector3 direction, float force, PlayerHealth ph)
    {
        BumpPlayer(ph.playerValues.playerClient.transform.GetComponent<NetworkObject>().Owner, ph, force, direction);
    }

    [TargetRpc]
    void BumpPlayer(NetworkConnection conn, PlayerHealth enemyHealth, float force, Vector3 direction)
    {
        enemyHealth.bounceDirection = direction;
        enemyHealth.bounceForce = force;
        enemyHealth.shouldBounce = true;
    }

    [ServerRpc (RunLocally = true)]
    private void RemoveAmmo()
    {
        if (reloadWeapon) chargedBullets --;
        else currentAmmo --;
    }

    [ServerRpc (RunLocally = true)]
    private void GiveDamage(float damageToGive, PlayerHealth enemyHealth, string name)
    {
        enemyHealth.killer = rootObject.transform;
        enemyHealth.health -= damageToGive;

        enemyHealth.KillCam();
        HitFeeback(enemyHealth);
        enemyHealth.Dismemberment(name);
    }

    [ServerRpc]
    void KillServer(PlayerHealth enemyHealth)
    {
        enemyHealth.isShot = true;
        
        enemyHealth.health = -8;

        GameManager.Instance.PlayerDied(enemyHealth.playerValues.playerClient.PlayerId);

        if (rootObject != null)
            enemyHealth.killer = rootObject.transform;

        KillObserver(enemyHealth.playerValues.playerClient.transform.GetComponent<NetworkObject>().Owner, enemyHealth.playerValues.playerClient, enemyHealth);

    }

    [TargetRpc]
    void KillObserver(NetworkConnection conn, ClientInstance client, PlayerHealth enemyHealth)
    {
        enemyHealth.shouldDropWeapon = true;
        enemyHealth.isDeadFromTargetRpc = true;

        if (rootObject != null)
            GameObject.Find("Main Camera").GetComponent<KillCam>().enemy = rootObject.transform;
    }

    [ObserversRpc]
    void HitFeeback(PlayerHealth enemyHealth)
    {
        enemyHealth.HitFeeback();
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
        
        var flash = Instantiate(muzzleFlash, muzzleFlashPoint.position, shootPoint.rotation);
        var children = flash.transform.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            if (child.GetComponent<Light>() == null && child.tag != "vfx")
                child.gameObject.layer = 8;

            if (child.GetComponent<Light>() != null) child.GetComponent<Light>().intensity = lightIntensity;
        }
        foreach (var fx in flash.GetComponentsInChildren<ParticleSystem>())
        {
            fx.Play();
        }
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
    private void SpawnVFXServer(int index, Vector3 hitPoint, Vector3 hitNormal, string surface, Transform parent)
    {
        SpawnVFX(index, hitPoint, hitNormal, surface, parent);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void SpawnVFX(int index, Vector3 hitPoint, Vector3 hitNormal, string surface, Transform parent)
    {
        if (Settings.Instance.reduceVFX) { return; }

        switch(surface)
             {
                 case "Footsteps/Concrete/Default":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/Dalles":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/Solide":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/PleinAir":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Wood/Creux":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(woodHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Dirt":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(dirtHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Sand":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(sandHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Grass":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(dirtHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Graviers":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Water":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(waterHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Pipe2":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(metalHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Grille":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(tauleHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Pipe":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(tauleHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Matelas":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(softbodyHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Moquette":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(softbodyHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Wood/Sec":
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(woodHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                case "NoSound":
                    break;
                case "Mine":
                    break;
                case "Grenade":
                    break;
                case "Hat":
                    break;
                 default:
                    if (index == 0)
                        Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
             }
        
        if (index == 2)
            Instantiate(bodyImpact, hitPoint, Quaternion.identity, parent);
    }
    
    private void LocalSound(int index)
    {
        if (index == 0)
            audio.PlayOneShot(headHitClip);
        if (index == 1)
            audio.PlayOneShot(bodyHitClip);
        if (index == 2)
            audio.PlayOneShot(deathClip);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        audio.PlayOneShot(reloadClip);
        if (animator != null) animator.SetTrigger("Reload");
        yield return new WaitForSeconds(reloadTime);

        if (currentAmmo - ammoCharge >= 0) 
        {
            currentAmmo -= ammoCharge;
            chargedBullets = ammoCharge;
        }
        else {
            chargedBullets = currentAmmo;
            currentAmmo = 0;   
        }

        isReloading = false;
    }

    IEnumerator BurstFire()
    {
        for (int i = 0; i < bulletsAmount; i++)
        {
            var randomspread = (playerController.isSprinting || !playerController.isGrounded || !playerController.safeGrounded ? Mathf.Lerp(maxSpread, minSpread, sprintAccuracy) : 
                                playerController.isCrouching ? Mathf.Lerp(maxSpread, minSpread, standingAccuracy) : 
                                playerController.isWalking ? Mathf.Lerp(maxSpread, minSpread, walkAccuracy) : 
                                Mathf.Lerp(maxSpread, minSpread, standingAccuracy));

            spread = cam.transform.right * Random.Range(randomspread, -randomspread) + cam.transform.up * Random.Range(randomspread, -randomspread);

            ShootServer(damage, cam.transform.position, cam.transform.forward + spread);

            //Local

            if (revolverShake) CameraRevolverAnimation();
            else CameraAnimation();
            WeaponAnimation();

            yield return new WaitForSeconds(timeBetweenBullets);
        }
    }

    [ServerRpc]
    private void SupressionServer(Transform supp)
    {
        SuppressionTarget(supp);
    }

    [ObserversRpc]
    private void SuppressionTarget(Transform supp)
    {
        supp.GetComponent<Suppression>().SuppressionTrigger();
    }
    

}
