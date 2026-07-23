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
using LambdaTheDev.NetworkAudioSync;
using UnityEngine.UI;

public class Minigun : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float reloadTime;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip releaseClip;
    [SerializeField] private float timeBeforeShooting = 0.6f;
    [SerializeField] private Transform rotativePart;
    [SerializeField] private float rotationSpeed = 1000;
    float fireTimer;
    bool touched;
    Vector3 spread;
    float shootTimer;
    bool pressed = true;
    bool released = true;
    bool noammoBool;
    NetworkAudioSource networkAudioSource;

    void Start()
    {
        chargedBullets = ammoCharge;
        currentAmmo -= ammoCharge;
        networkAudioSource = GetComponent<NetworkAudioSource>();
    }

    private void Update()
    {
        WeaponUpdate();

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        //If not in hands
        if (gameObject.layer == 7) return;

        if (currentAmmo <= 0 && noammoBool) 
        {
            AudioStop();
            noammoBool = false;
        }
        if (currentAmmo > 0) noammoBool = true;

        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && currentAmmo > 0) {
            shootTimer += Time.deltaTime;
            released = false;
        }
        else if (fire2.ReadValue<float>() > 0.1f && inLeftHand && currentAmmo > 0) {
            shootTimer += Time.deltaTime;
            released = false;
        }
        else {
            shootTimer -= Time.deltaTime;
            pressed = false;
        }

        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && !pressed && currentAmmo > 0) {
            pressed = true;
            AudioPlay();
        }
        if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand && !released) {
            released = true;
            if (currentAmmo > 0) AudioStop();
        }

        shootTimer = Mathf.Clamp(shootTimer, 0, timeBeforeShooting);


        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && (!reloadWeapon ? currentAmmo >0 : 1 == 1) && shootTimer == timeBeforeShooting) {
            Fire();
        }
        else if (fire2.ReadValue<float>() > 0.1f && inLeftHand && (!reloadWeapon ? currentAmmo >0 : 1 == 1) && shootTimer == timeBeforeShooting) {
            Fire();
        }

        rotativePart.Rotate(0, 0, Mathf.Lerp(0, rotationSpeed * Time.deltaTime, shootTimer / timeBeforeShooting));
    


    }

    [ServerRpc]
    private void AudioPlay()
    {
        networkAudioSource.Play();
    }

    [ServerRpc]
    private void AudioStop()
    {
        networkAudioSource.Stop();
        PlayReleaseClipObservers();
    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (changePitchOnShoot) audio.pitch = Random.Range(0.97f, 1.03f);

        if (fireTimer > 0) {
            shot = true;
            return;
        }
        shot = false;

        fireTimer = timeBetweenFire;    

        if (currentAmmo <=0) 
        {
            audio.PlayOneShot(nobulletClip);
            noAmmoClicks ++;
            if (noAmmoClicks > 1 && inRightHand) rootObject.GetComponent<PlayerPickup>().RightHandDrop();
            if (noAmmoClicks > 1 && inLeftHand) rootObject.GetComponent<PlayerPickup>().LeftHandDrop();
        }

        if (!reloadWeapon && currentAmmo <= 0) return;

        if (reloadWeapon && isReloading) return;

        if (reloadWeapon && chargedBullets <= 0 && currentAmmo > 0) StartCoroutine(Reload());
        if (reloadWeapon && chargedBullets <= 0 && currentAmmo <= 0) audio.PlayOneShot(nobulletClip);
        if (reloadWeapon && chargedBullets <= 0) return;

        //Online
        if (burstGun) StartCoroutine(BurstFire());
        else {
            var randomspread = (ScopeAimWeapon && !playerController.isAiming ? notAimingAccuracy : playerController.isSprinting || !playerController.safeGrounded ? Mathf.Lerp(maxSpread, minSpread, sprintAccuracy) : 
                                playerController.isCrouching ? Mathf.Lerp(maxSpread, minSpread, standingAccuracy) : 
                                playerController.isWalking ? Mathf.Lerp(maxSpread, minSpread, walkAccuracy) : 
                                Mathf.Lerp(maxSpread, minSpread, standingAccuracy));

            spread = cam.transform.right * Random.Range(randomspread, -randomspread) + cam.transform.up * Random.Range(randomspread, -randomspread);

            ShootServer(damage, cam.transform.position, cam.transform.forward + spread);

            //Local

            if (revolverShake) CameraRevolverAnimation();
            else CameraAnimation();
            WeaponAnimation();
        }
            
    }

    bool hitOK;

    private PlayerHealth enemyHealth;

    private void ShootServer(float damageToGive, Vector3 position, Vector3 direction)
    {
        RemoveAmmo();
        ShootServerEffect();

        RaycastHit[] envHits = Physics.RaycastAll(position, direction, Mathf.Infinity, defaultLayer);
        System.Array.Sort(envHits, (x,y) => x.distance.CompareTo(y.distance));

        if (envHits.Length != 0)
        {
            for (int i = 0; i < envHits.Length; i++)
            {
                TriggerEnvironment(envHits[i].transform.gameObject, envHits[i].point, direction, envHits[i].normal);
                SpawnBulletTrailServer(envHits[i].point);

                if (envHits[i].transform.tag == "ShatterableGlass")
                {
                    BreakGlassServer(envHits[i].point, direction, envHits[i].transform.gameObject);
                }
                else if (envHits[i].transform.gameObject.layer != LayerMask.NameToLayer("Ragdoll")) {
                    SpawnVFXServer(0, envHits[i].point, envHits[i].normal, envHits[i].transform.tag, envHits[i].transform);
                    SpawnVFXServer(1, envHits[i].point, envHits[i].normal, envHits[i].transform.tag, envHits[i].transform);
                }

                if (envHits[i].transform.gameObject.layer != 10 && envHits[i].transform.gameObject.layer != 11 && envHits[i].transform.gameObject.layer != 14 && envHits[i].transform.gameObject.layer != 18  && envHits[i].transform.gameObject.layer != 19 && envHits[i].transform.gameObject.layer != 24) break;
            }
        }

        if (Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            if (hit.transform.gameObject.layer == 11) {

                if (hit.transform.root.CompareTag("Player") ? hit.transform.root.TryGetComponent(out enemyHealth) : hit.transform.GetComponentInParent<PlayerHealth>() != null) {

                    if (hit.transform.root.tag != "Player") enemyHealth = hit.transform.GetComponentInParent<PlayerHealth>();

                    if (enemyHealth.gameObject == transform.root.gameObject) 
                        return;

                    // Friendly fire check
                    if (FriendlyFireCheck(enemyHealth)) { return; }
                    
                    var headshot = hit.transform.gameObject.name == "Head_Col" || hit.transform.gameObject.name == "Neck_1_Col";

                    if (headshot){
                        damageToGive *= headMultiplier;
                        Settings.Instance.IncreaseHeadshotsAmount();
                    }
                    else Settings.Instance.IncreaseBodyshotsAmount();

                    if (headshot && headImpact) Instantiate(headImpact, hit.point, Quaternion.identity, (enemyHealth.health-damageToGive > 0 ? hit.transform : null));
                    else Instantiate(bodyImpact, hit.point, Quaternion.LookRotation(hit.normal));
                    if ((playGenericBodyImpactOnBody ? genericBodyImpact : genericBodyImpact && headshot)) Instantiate(genericBodyImpact, hit.point, Quaternion.identity, (enemyHealth.health-damageToGive > 0 ? hit.transform : null));

                    ServerFX(hit.point + direction, Quaternion.identity);

                    if (enemyHealth.health-damageToGive <= 0) KillShockWave();

                    if (marker == null) {
                        marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
                        marker.transform.DOPunchScale((hit.transform.gameObject.name == "Head_Col" ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
                        if (hit.transform.gameObject.name == "Head_Col") {
                            marker.GetComponent<Image>().color = Color.red;
                        }
                        Destroy(marker, 0.3f);
                    }
                    else{
                        Destroy(marker);
                        marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
                        marker.transform.DOPunchScale((hit.transform.gameObject.name == "Head_Col" ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
                        if (hit.transform.gameObject.name == "Head_Col") {
                            marker.GetComponent<Image>().color = Color.red;
                        }
                        Destroy(marker, 0.3f);
                    }

                    if (headshot)
                    {
                        LocalSound(0);
                        LocalSound(1);
                    }
                    else {
                        LocalSound(1);
                    }


                    if (enemyHealth.health-damageToGive <= 0) {
                        LocalSound(2);
                        enemyHealth.graphics.SetActive(false);
                        enemyHealth.controller.playerPickupScript.fpArms.gameObject.SetActive(false);
                        enemyHealth.GetComponent<CharacterController>().enabled = false;
                        enemyHealth.Explode(false, true, hit.transform.gameObject.name, direction, ragdollEjectForce, hit.point);
                        KillServer(enemyHealth);
                        enemyHealth.DisablePlayerObjectWhenKilled();
                        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was {(headshot ? "headshot" : "killed")} with {(StartsWithVowel(behaviour.weaponName) ? "an " : "a")} <b><color=white>{behaviour.weaponName}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
                    }
                    else GiveDamage(damageToGive, enemyHealth, hit.transform.gameObject.name);

                    hitOK = true;
                    
                }
            }
        }

        if (Physics.Raycast(position, direction, out RaycastHit supHit, Mathf.Infinity, supLayer) && !hitOK)
        {
            if (supHit.transform.gameObject.layer == 17)
                SupressionServer(supHit.transform);
        }

        //Doll Fight
        if (Physics.Raycast(position, direction, out RaycastHit doll, Mathf.Infinity) && doll.transform.TryGetComponent(out DollHealth dollHealth))
        {
            dollHealth.health -= damageToGive;
        }

        hitOK = false;
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

    [ServerRpc (RunLocally = true)]
    private void PlayReleaseClip()
    {
        PlayReleaseClipObservers();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void PlayReleaseClipObservers()
    {
        audio.PlayOneShot(releaseClip);
    }

    [ServerRpc (RunLocally = true)]
    public void ServerFX(Vector3 position, Quaternion rotation)
    {
        ObserversFX(position, rotation);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ObserversFX(Vector3 position, Quaternion rotation)
    {
        Instantiate(bloodSplatter, position, rotation);
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
            if (child.GetComponent<Light>() == null && child.tag != "vfx" && IsOwner)
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

        if (genericImpact) Instantiate(genericImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
        switch(surface)
             {
                 case "Footsteps/Concrete/Default":
                    if (index == 0)
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/Dalles":
                    if (index == 0)
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/Solide":
                    if (index == 0)
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Concrete/PleinAir":
                    if (index == 0)
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Wood/Creux":
                    if (index == 0)
                        Instantiate(woodHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(woodHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Dirt":
                    if (index == 0)
                        Instantiate(dirtHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(dirtHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Sand":
                    if (index == 0)
                        Instantiate(sandHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(sandHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Grass":
                    if (index == 0)
                        Instantiate(dirtHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(dirtHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Graviers":
                    if (index == 0)
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(bulletHole, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Water":
                    if (index == 0)
                        Instantiate(waterHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(waterHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Pipe2":
                    if (index == 0)
                        Instantiate(metalHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(metalHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Grille":
                    if (index == 0)
                        Instantiate(tauleHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(tauleHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Metal/Pipe":
                    if (index == 0)
                        Instantiate(tauleHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(tauleHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Matelas":
                    if (index == 0)
                        Instantiate(softbodyHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(softbodyHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Moquette":
                    if (index == 0)
                        Instantiate(softbodyHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    if (index == 1)
                        Instantiate(softbodyHitFx, hitPoint, Quaternion.LookRotation(hitNormal), parent);
                    break;
                 case "Footsteps/Wood/Sec":
                    if (index == 0)
                        Instantiate(woodHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
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
                        Instantiate(concreteHitImpact, hitPoint, Quaternion.LookRotation(hitNormal), parent);
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
