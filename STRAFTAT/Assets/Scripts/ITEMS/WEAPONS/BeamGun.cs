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
using LambdaTheDev.NetworkAudioSync;
using UnityEngine.UI;


public class BeamGun : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float launchForce = 12;
    [SerializeField] private float playerKnockback = 2;
    [SerializeField] private PredictedProjectile _projectile;
    [SerializeField] private GameObject muzzleFlash2;
    [SerializeField] private AudioClip smallFireClip;
    private const float MAX_PASSED_TIME = 0.3f;

    float accumulatedPower;
    [SerializeField] private AudioSource newSource;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private bool hasIntermediateStates;
    [SerializeField] private float radius = 0.1f;
    float fireTimer;
    bool isClicked2 = false;
    bool isClicked3 = true;
    bool isClicked4;
    bool lastInRightHand;
    bool lastInLeftHand;

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

        

        if (!IsOwner) return;

        //If not in hands
        if (gameObject.layer == 7) 
        {
            accumulatedPower = 0;
            AudioStop();
            CancelChargeEffect();
            return;
        }

        if (PauseManager.Instance.pause) return;
        
        // Detect hand switch and reset charge state
        if ((lastInRightHand && !inRightHand) || (lastInLeftHand && !inLeftHand))
        {
            accumulatedPower = 0;
            isClicked = false;
            isClicked2 = false;
            isClicked3 = true;
            isClicked4 = false;
            AudioStop();
            CancelChargeEffect();
        }
        
        lastInRightHand = inRightHand;
        lastInLeftHand = inLeftHand;
        
        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand) 
            isClicked2 = true;

        if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand) 
            isClicked2 = true;
        

        if (isClicked2)
        {
            if (accumulatedPower < 0.3f && isClicked4) {
                if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand && isClicked3) {
                    FireBlast();
                    isClicked2 = false;
                }

                if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && inLeftHand && isClicked3) {
                    FireBlast();
                    isClicked2 = false;
                }
            }
        }

        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && isClicked == false && isClicked3 && fireTimer <= 0) {
            ShootServerEffect();
            AudioPlay();
            isClicked4 = true;
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && isClicked == false && isClicked3 && fireTimer <= 0) {
            ShootServerEffect();
            AudioPlay();
            isClicked4 = true;
        }

        //Charge
        if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand && fireTimer <= 0 && isClicked3) {
            accumulatedPower += Time.deltaTime;
            cam.DOShakeRotation(duration, strength * (accumulatedPower/maxChargeTime), vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
            isClicked = true;
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand && fireTimer <= 0 && isClicked3) {
            accumulatedPower += Time.deltaTime;
            cam.DOShakeRotation(duration, strength * (accumulatedPower/maxChargeTime), vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
            isClicked = true;
        }
        
        //decharge
        if (isClicked && inRightHand && accumulatedPower >= maxChargeTime)
        {
            isClicked = false;
            isClicked2 = false;
            isClicked3 = false;
            isClicked4 = false;
            Fire();
            accumulatedPower = 0;
        }
        else if (isClicked && inLeftHand && accumulatedPower >= maxChargeTime)
        {
            isClicked = false;
            isClicked2 = false;
            isClicked3 = false;
            isClicked4 = false;
            Fire();
            accumulatedPower = 0;
        }
        else if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && isClicked && inRightHand) {
            isClicked = false;
            isClicked2 = false;
            isClicked3 = true;
            Fire();
            accumulatedPower = 0;
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && isClicked && inLeftHand) {
            isClicked = false;
            isClicked2 = false;
            isClicked3 = true;
            Fire();
            accumulatedPower = 0;
        }
        else if ((invertFire ? fire2 : fire1).ReadValue<float>() < 0.1f && inRightHand) {
            isClicked3 = true;
        }
        else if ((invertFire ? fire1 : fire2).ReadValue<float>() < 0.1f && inLeftHand) {
            isClicked3 = true;
        }

        

        

    }

    private void FireBlast()
    {
        if (PauseManager.Instance.pause) return;

        if (!playerController.IsOwner) return;

        if (fireTimer > 0) return;

        fireTimer = timeBetweenFire;    

        if (currentAmmo <=0) audio.PlayOneShot(nobulletClip);

        if (currentAmmo <= 0) return;

        Vector3 position = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        //Local

        if (revolverShake) CameraRevolverAnimation();
        else CameraAnimation();
        WeaponAnimation();

        

        //Online

        if (IsServer) SpawnProjectile(position, direction, 0f);
        ServerFire(position, direction, base.TimeManager.Tick);
        if (playerKnockback != 0) playerController.AddForce(-cam.transform.forward, playerKnockback);

            
    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (changePitchOnShoot) audio.pitch = Random.Range(0.97f, 1.03f);

        AudioStop();
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

        if (revolverShake) AltCameraRevolverAnimation();
        else CameraAnimation();
        WeaponAnimation();



            
    }

    bool hitOK;

    private void ShootServer(float damageToGive, Vector3 position, Vector3 direction)
    {
        BeamEffectServer(0);

        SoundServerEffect();

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

                if (envHits[i].transform.gameObject.layer != 11 && envHits[i].transform.gameObject.layer != 14 && envHits[i].transform.gameObject.layer != 18  && envHits[i].transform.gameObject.layer != 19 && envHits[i].transform.gameObject.layer != 24) break;
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
    private void GiveDamage(float damageToGive, PlayerHealth enemyHealth, string name)
    {
        enemyHealth.killer = rootObject.transform;
        enemyHealth.health -= damageToGive;

        enemyHealth.KillCam();
        HitFeeback(enemyHealth);
        enemyHealth.Dismemberment(name);
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
    private void ShootServerEffect()
    {
        ShootObserversEffect2();
    }

    ParticleSystem tempBeam;

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect2()
    {
        var flash = Instantiate(muzzleFlash, muzzleFlashPoint.position, shootPoint.rotation, muzzleFlashPoint);
        tempBeam = flash.GetComponent<ParticleSystem>();
        tempBeam.Play();
    }

    [ServerRpc (RunLocally = true)]
    private void ShootServerEffect2()
    {
        ShootObserversEffect();
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

        Destroy(bulletTrailEffect, 1.2f);
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
    private void SoundServerEffect2()
    {
        SoundObserversEffect2();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void SoundObserversEffect2()
    {
        newSource.PlayOneShot(smallFireClip);
    }

    private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime)
    {

            PredictedProjectile pp = Instantiate(_projectile, position, Quaternion.identity);
            pp.Initialize(direction, launchForce, passedTime, rootObject, gameObject);
            pp.isOwner = IsOwner;
            pp.weapon = this;
            //InstanceFinder.ServerManager.Spawn(pp.gameObject, base.IsOwner);
        
        
    }

    [ServerRpc (RunLocally = true)]
    private void ServerFire(Vector3 position, Vector3 direction, uint tick)
    {
        if (needsAmmo) currentAmmo --;

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
        
        audio.PlayOneShot(smallFireClip);

        if (Settings.Instance.reduceVFX) { return; }
        
            var flash = Instantiate(muzzleFlash2, muzzleFlashPoint.position, shootPoint.rotation);
            flash.GetComponent<ParticleSystem>().Play();
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
    
    private void LocalSound(int index)
    {
        if (index == 0)
            audio.PlayOneShot(headHitClip);
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
    private void CancelChargeEffect() { CancelChargeEffectObservers(); }

    [ObserversRpc (RunLocally = true)]
    private void CancelChargeEffectObservers() {
        if (tempBeam != null) {
            tempBeam.Stop();
            Destroy(tempBeam.gameObject);
            tempBeam = null;
        }
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
    

}
