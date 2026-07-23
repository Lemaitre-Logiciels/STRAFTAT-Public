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
using HeathenEngineering.PhysKit;
using HeathenEngineering.PhysKit.API;

public struct trickShotData {
    public Vector3 forward;
    public float speed;
    public float radius;
    public BallisticPath[] prediction;
}

public class DualLauncher : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float launchForce = 12;
    [SerializeField] private float playerKnockback = 2;
    [SerializeField] private PredictedProjectile _projectile;
    [SerializeField] private ShrapnelBallistic shrapnelProj;
    [SerializeField] private RebondBalle _projectile2;
    [SerializeField] private HandGrenadeTwo _projectile3;
    [SerializeField] private TrickShot trickShot;
    [SerializeField] private Obus _projectile4;
    private const float MAX_PASSED_TIME = 0.3f;
    [SerializeField] private bool rebond;
    [SerializeField] private bool grenade;
    [SerializeField] private bool obus;
    [SerializeField] private bool kanye;
    [SerializeField] private bool kanyeShoot;
    [SerializeField] private bool shrapnel;
    [SerializeField] private bool bubble;
    [SerializeField] private List<GameObject> kanyeBullets = new List<GameObject>();
    float fireTimer;

    [Space]

    [SerializeField] private GameObject douilleGrenade;
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private AudioClip douilleClip;
    [SerializeField] private AudioClip beforeDetonationClip;
    [SerializeField] private ParticleSystem grenadeSmoke;
    float grenadeTimer;
    [SyncVar] public bool grenadeOpen;

    void Start()
    {
        grenadeTimer = timeBeforeExplosion;
    }

    [ServerRpc (RunLocally = true)] private void SetBool(bool value) => grenadeOpen = value;


    private void Update()
    {
        WeaponUpdate(); 

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (grenadeOpen && grenadeTimer == timeBeforeExplosion && grenade) {
            var smoke = Instantiate(grenadeSmoke, transform.position + new Vector3(-0.007f, -0.121f, -0.031f), Quaternion.identity, shootPoint);
            smoke.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
            smoke.transform.forward = transform.up;
            GrenadeSFXServer(0);
            GrenadeSFXServer(1);
            
            douilleGrenade.transform.SetParent(null);
            douilleGrenade.AddComponent<Rigidbody>();
            var tempRb = douilleGrenade.GetComponent<Rigidbody>();
            tempRb.interpolation = RigidbodyInterpolation.Interpolate;
            tempRb.AddForce(transform.right * 0.4f + Vector3.up * 2, ForceMode.Impulse);
            var randomInt = Random.Range(-1, 1);
            tempRb.AddTorque(transform.forward * 4 + transform.right * 4, ForceMode.Impulse);
        }
        if (grenadeOpen) grenadeTimer -= Time.deltaTime;
        if (grenadeTimer <= 0.06f && grenade && grenadeOpen && IsOwner) Fire();

        //If not in hands
        if (gameObject.layer == 7) return;

        if (grenadeOpen && grenade && Input.GetKeyDown(KeyCode.F) && IsOwner && behaviour.playerPickup.currentEnvironmentInteractable == null)
            Fire();

        if (!IsOwner) return;
        

        //Shoot

        if (grenade && grenadeTimer == timeBeforeExplosion) {
            if (!onePressShoot)
            {
                if ((invertFire ? fire2 : fire1).ReadValue<float>() > 0.1f && inRightHand) 
                    SetBool(true);

                if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand) 
                    SetBool(true);
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
                    SetBool(true);
                    isClicked = false;
                }

                if ((invertFire ? fire1 : fire2).ReadValue<float>() > 0.1f && inLeftHand) {
                    SetBool(true);
                    isClicked = false;
                }
            }
        }
        else {
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

    }

    IEnumerator LaunchWithDelay()
    {
        yield return new WaitForSeconds(0.1f);

        Fire();
    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (grenade)
            if (behaviour.playerPickup.currentEnvironmentInteractable != null) return;

        if (changePitchOnShoot) audio.pitch = Random.Range(0.97f, 1.03f);

        if (fireTimer > 0) return;

        fireTimer = timeBetweenFire;

        if (currentAmmo <=0) 
        {
            audio.PlayOneShot(nobulletClip);
            noAmmoClicks ++;
            if (noAmmoClicks > 1 && inRightHand) rootObject.GetComponent<PlayerPickup>().RightHandDrop();
            if (noAmmoClicks > 1 && inLeftHand) rootObject.GetComponent<PlayerPickup>().LeftHandDrop();
        }

        if (currentAmmo <=0) audio.PlayOneShot(nobulletClip);

        if (currentAmmo <= 0) return;

        if (kanye && kanyeShoot && kanyeBullets.Count > 1)
        {
            kanyeShoot = false;
            kanyeBullets[0].transform.SetParent(null);
            Destroy(kanyeBullets[0]);
            kanyeBullets.Remove(kanyeBullets[0]); 

        }
        else if (kanye) kanyeShoot = true;

        Vector3 position = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        if (shrapnel) {
            if (Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, defaultLayer)){
                position = hit.point;
                SpawnBulletTrailServer(hit.point);
            }
            else {
                position += direction * 15;
                SpawnBulletTrailServer(position);
            }
        }

        //Online

        trickShotData data = default;
        if (trickShot) {
            data = new trickShotData {
                forward = trickShot.selfTransform.forward,
                speed = trickShot.speed,
                radius = trickShot.radius,
                prediction = trickShot.prediction.ToArray()
            };
        }
        
        //if (IsServer) {
       //     SpawnProjectile(position, direction, 0f, data, Owner);
       // }
        if (needsAmmo) currentAmmo --;
        ServerFire(position, direction, base.TimeManager.Tick, data, Random.Range(0, int.MaxValue));
        if (playerKnockback != 0) playerController.AddForce(-cam.transform.forward, playerKnockback);

        if (gameObject.name == "RocketLauncher(Clone)") StartCoroutine(RocketJumpCheck(rootObject.transform.position));
        

        //Local

        CameraAnimation();
        WeaponAnimation();
            
    }

    IEnumerator RocketJumpCheck(Vector3 firstPosition)
    {
        yield return new WaitForSeconds(0.55f);
        if (rootObject != null)
            if ((rootObject.transform.position.y - firstPosition.y) > 2) Settings.Instance.rocketJumps ++;
    }
    
    private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime, trickShotData trickShotData, NetworkConnection owner, int id)
    {
        if (PauseManager.BetweenRounds) { return; }
        
        if (grenade) {
            if (IsServer) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                ServerManager.Spawn(GO, owner);
                GO.GetComponent<PhysicsGrenade>().Initialize(rootObject, gameObject, passedTime, grenadeTimer);
                GO.GetComponent<BallisticPathFollowSyncer>().Shoot(trickShotData, id);
            }
            else if (owner.IsLocalClient) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                GO.GetComponent<NetworkObject>().enabled = false;
                GO.GetComponent<PhysicsGrenade>().enabled = false;
                GO.GetComponent<BallisticPathFollowSyncer>().ShootNonRPC(trickShotData, id);
            }
        }
        else if (shrapnel) 
        {
            ShrapnelBallistic pp = Instantiate(shrapnelProj, position, Quaternion.LookRotation(direction));
            pp.Initialize(rootObject, gameObject, passedTime);
            pp.isOwner = IsOwner;
            pp.weapon = this;
        }
        else if (obus) {
            if (IsServer) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                ServerManager.Spawn(GO, owner);
                GO.GetComponent<Obus>().Initialize(rootObject, gameObject, passedTime);
                GO.GetComponent<BallisticPathFollowSyncer>().Shoot(trickShotData, id);
            }
            else if (owner.IsLocalClient) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                GO.GetComponent<NetworkObject>().enabled = false;
                GO.GetComponent<Obus>().enabled = false;
                GO.GetComponent<BallisticPathFollowSyncer>().ShootNonRPC(trickShotData, id);
            }
        }
        else if (bubble) {
            if (IsServer) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                ServerManager.Spawn(GO, owner);
                GO.GetComponent<Bubble>().Initialize(rootObject, gameObject, passedTime);
                GO.GetComponent<BallisticPathFollowSyncer>().Shoot(trickShotData, id);
            }
            else if (owner.IsLocalClient) {
                Quaternion rot = Quaternion.LookRotation(direction);
                GameObject GO = Instantiate(trickShot.template.gameObject, position, rot);
                GO.GetComponent<NetworkObject>().enabled = false;
                GO.GetComponent<Bubble>().enabled = false;
                GO.GetComponent<BallisticPathFollowSyncer>().ShootNonRPC(trickShotData, id);
            }
        }
        else if (rebond) 
        {
            RebondBalle pp = Instantiate(_projectile2, position, Quaternion.identity);
            pp.Initialize(direction, launchForce, passedTime, rootObject, gameObject);
            pp.weapon = this;
        }
        else {
            PredictedProjectile pp = Instantiate(_projectile, position, Quaternion.LookRotation(direction));
            pp.Initialize(direction, launchForce, passedTime, rootObject, gameObject);
            pp.isOwner = IsOwner;
            pp.weapon = this;
            //InstanceFinder.ServerManager.Spawn(pp.gameObject, base.IsOwner);
        }
        
    }

    [ServerRpc(RunLocally = true)]
    private void ServerFire(Vector3 position, Vector3 direction, uint tick, trickShotData data, int id)
    {
        ShootObserversEffect();
        ObserversFire(Owner, position, direction, tick, data, id);
    }

    [ObserversRpc(RunLocally = true, ExcludeOwner = true)]
    private void ObserversFire(NetworkConnection owner, Vector3 position, Vector3 direction, uint tick, trickShotData data, int id)
    {
        float passedTime = (float)base.TimeManager.TimePassed(tick, false);

        passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

        SpawnProjectile(position, direction, passedTime, data, owner, id);
    }

    [ObserversRpc(RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect()
    {
        audio.PlayOneShot(fireClip);
     
        if (Settings.Instance.reduceVFX) { return; }
        
        if (!grenade) {
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
    }

    [ServerRpc (RunLocally = true)]
    private void GrenadeSFXServer(int j)
    {
        GrenadeSFX(j);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void GrenadeSFX(int j)
    {
        if (j == 0)
            audio.PlayOneShot(douilleClip);
        if (j == 1)
            audio.PlayOneShot(beforeDetonationClip);

    }
    
    private void LocalSound(int index)
    {
        if (index == 0)
            audio.PlayOneShot(headHitClip);
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
    

}
