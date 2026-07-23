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

public class ChargeGun : Weapon
{
    [Header("Weapon Specials")]
    float accumulatedPower;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private bool hasIntermediateStates;
    [SerializeField] private float radius = 0.1f;
    [SerializeField] private float playerKnockback = 2;
    [SerializeField] private AudioSource newSource;
    float fireTimer;
    bool touched;
    NetworkAudioSource networkAudioSource;

    void Start()
    {
        networkAudioSource = GetComponent<NetworkAudioSource>();
    }

    private void Update()
    {
        WeaponUpdate();

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        //If not in hands
        if (gameObject.layer == 7) 
        {
            accumulatedPower = 0;
            AudioStop();
            return;
        }

        if (!IsOwner) return;

        if (PauseManager.Instance.pause) return;
        
        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && isClicked == false && fireTimer <= 0) {
            ShootServerEffect();
            AudioPlay();
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && isClicked == false && fireTimer <= 0) {
            ShootServerEffect();
            AudioPlay();
        }

        //Charge
        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && fireTimer <= 0) {
            accumulatedPower += Time.deltaTime;
            cam.DOShakeRotation(duration, strength * (accumulatedPower/maxChargeTime), vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
            isClicked = true;
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && fireTimer <= 0) {
            accumulatedPower += Time.deltaTime;
            cam.DOShakeRotation(duration, strength * (accumulatedPower/maxChargeTime), vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
            isClicked = true;
        }
    
        
        camAnimScript.rotateBack = true;


        //decharge
        if (isClicked && inRightHand && accumulatedPower >= maxChargeTime)
        {
            isClicked = false;
            Fire();
        }
        else if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && isClicked && inRightHand) {
            isClicked = false;
            Fire();
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && isClicked && inLeftHand) {
            isClicked = false;
            Fire();
        }

        

    }
    

    private void Fire()
    {
        if (PauseManager.Instance.pause) return;

        if (!playerController.IsOwner) return;

        AudioStop();

        if (fireTimer > 0) {
            shot = true;
            return;
        }

        if (accumulatedPower < maxChargeTime) 
        {
            BeamEffectServer(1);
            accumulatedPower = 0;
            return;
        }
        accumulatedPower = 0;
        shot = false;

        fireTimer = timeBetweenFire;    

        ShootServer(damage, cam.transform.position, cam.transform.forward);

        //Local

        if (revolverShake) CameraRevolverAnimation();
        else CameraAnimation();
        WeaponAnimation();
        if (playerKnockback != 0) playerController.AddForce(-cam.transform.forward, playerKnockback);
            
    }

    bool hitOK;

    private void ShootServer(float damageToGive, Vector3 position, Vector3 direction)
    {
        BeamEffectServer(0);

        SoundServerEffect();
        RemoveAmmo();

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

        if (Physics.CapsuleCast(position, position, radius, cam.transform.forward, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {

            if (hit.transform.root.TryGetComponent(out PlayerHealth enemyHealth)) {
                
                if (enemyHealth.gameObject == transform.root.gameObject) 
                    return;
                
                // Friendly fire check
                if (FriendlyFireCheck(enemyHealth)) { return; }
                
                var headshot = hit.transform.gameObject.name == "Head_Col";

                if (hit.transform.gameObject.name == "Head_Col") {
                    damageToGive *= 2;
                    Settings.Instance.IncreaseHeadshotsAmount();
                }
                else Settings.Instance.IncreaseBodyshotsAmount();

                Instantiate(bodyImpact, hit.point, Quaternion.identity);
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

                if (hit.transform.gameObject.name == "Head_Col")
                {
                    LocalSound(0);
                    LocalSound(1);
                }
                else {
                    LocalSound(1);
                }


                if (enemyHealth.health-damageToGive <= 0) {
                    LocalSound(2);
                    enemyHealth.GetComponent<CharacterController>().enabled = false;
                    enemyHealth.Explode(false, false, hit.transform.gameObject.name, direction, ragdollEjectForce, hit.point);
                    enemyHealth.graphics.SetActive(false);
                    enemyHealth.controller.playerPickupScript.fpArms.gameObject.SetActive(false);
                    KillServer(enemyHealth);
                    enemyHealth.DisablePlayerObjectWhenKilled();
                    PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was {(headshot ? "headshot" : "killed")} with {(StartsWithVowel(behaviour.weaponName) ? "an" : "a")} <b><color=white>{behaviour.weaponName}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
                }
                else GiveDamage(damageToGive, enemyHealth, hit.transform.gameObject.name);

                hitOK = true;
                
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

    [ServerRpc]
    private void AudioPlay()
    {
        networkAudioSource.Play();
    }

    [ServerRpc]
    private void AudioStop()
    {
        networkAudioSource.Stop();
    }

    [ServerRpc (RunLocally = true)]
    private void SoundServerEffect()
    {
        SoundObserversEffect();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void SoundObserversEffect()
    {
        newSource.PlayOneShot(fireClip);
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

    ParticleSystem tempBeam;

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect()
    {
        //if (Settings.Instance.reduceVFX) { return; } hand cannon cannt have this

        var flash = Instantiate(muzzleFlash, muzzleFlashPoint.position, shootPoint.rotation, transform);
        tempBeam = flash.GetComponent<ParticleSystem>();
        tempBeam.Play();
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

    [ServerRpc (RunLocally = true)]
    private void BeamEffectServer(int i)
    {
        BeamEffectObservers(i);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void BeamEffectObservers(int i)
    {
        if (i == 0)
            tempBeam.transform.SetParent(null);

        if (i == 1)
            tempBeam.gameObject.SetActive(false);
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
    

}
