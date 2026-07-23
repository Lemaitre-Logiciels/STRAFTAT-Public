using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using FishNet.Component.Animating;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class MeleeWeapon : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private GameObject collisionObj;
    [SerializeField] private MeleeChildCollision collisionScript;
    [SerializeField] private NetworkAnimator attackAnimator;
    [SerializeField] private string baseAttackAnim;
    [SerializeField] private string baseAttackLeftAnim;
    [SerializeField] private string secondAttackAnim;
    [SerializeField] private float timeBetweenBaseAttack = 0.7f;
    [SerializeField] private float timeBetweenSecondAttack = 1f;
    [SerializeField] private float baseAttackDuration = 0.2f;
    [SerializeField] private float secondAttackDuration = 0.4f;
    [SerializeField] private bool propulsion;
    [SerializeField] private float basePropulsionAmount;
    [SerializeField] private float secondPropulsionAmount;
    [SerializeField] private bool bforce;
    [SerializeField] private float bforceDecel = 9;
    [SerializeField] private float baseAttackDamage;
    [SerializeField] private float secondAttackDamage;
    [SerializeField] private AudioClip secondAttackClip;
    [SerializeField] private AudioClip firstAttackStartClip;
    [SerializeField] private AudioClip secondAttackStartClip;

    [Space]
    [SerializeField] private bool bounceHolder;
    [SerializeField] private float repulseForce;
    [SerializeField] private float playerKnockback;
    public int hitsAmount = 1;

    [Space]
    [SerializeField] private float firstAttackDelay;
    [SerializeField] private float secondAttackDelay;
    [SerializeField] private bool minusScale;

    float fireTimer;
    bool touched;
    bool zoomTrigger;
    bool secondAttackPlaying;

    private void Update()
    {
        WeaponUpdate();

        //Fire timer countdown
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (behaviour.outofhandsAnim != "")
            attackAnimator.Animator.SetBool(behaviour.outofhandsAnim, gameObject.layer == 7);

        //If not in hands
        if (gameObject.layer == 7) return;

        if (minusScale && inLeftHand) transform.localScale = new Vector3(-1, 1 ,1 );

        if (fireTimer < timeBetweenBaseAttack - baseAttackDuration && !playerController.isAiming) collisionScript.canHit = false;
        else if (fireTimer < timeBetweenSecondAttack - secondAttackDuration && playerController.isAiming) {
            collisionScript.canHit = false;
            secondAttackPlaying = false;
        }

        if (fireTimer < timeBetweenBaseAttack - baseAttackDuration && !playerController.isAiming) collisionScript.canHitEnvi = false;
        else if (fireTimer < timeBetweenSecondAttack - secondAttackDuration && playerController.isAiming) collisionScript.canHitEnvi = false;

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
        
        if (fireTimer > 0.1f && secondAttackDelay > 0 && secondAttackPlaying) {
            playerController.isZooming = true;
            zoomTrigger = true;
            
        }
        else if (zoomTrigger && !collisionScript.canHit && secondAttackDelay > 0)
        {
            playerController.isZooming = false;
            zoomTrigger = false;
        }
            
        

    }

    private void Fire()
    {
        if (PauseManager.Instance.pause || !playerController.canMove) return;

        if (!playerController.IsOwner) return;

        if (fireTimer > 0) return;

        StartCoroutine(FireCoroutine());
        if (firstAttackStartClip != null && (behaviour.aimWeapon ? !playerController.isAiming : true) ) ShootServerEffect(2);
        else if (secondAttackStartClip != null && behaviour.aimWeapon && playerController.isAiming) ShootServerEffect(3);
        fireTimer = behaviour.aimWeapon && playerController.isAiming ? timeBetweenSecondAttack : timeBetweenBaseAttack;
        secondAttackPlaying = behaviour.aimWeapon && playerController.isAiming;
        attackAnimator.SetTrigger(behaviour.aimWeapon && playerController.isAiming ? secondAttackAnim : inLeftHand ? baseAttackLeftAnim : baseAttackAnim);
            
    }

    IEnumerator FireCoroutine()
    {
        yield return new WaitForSeconds(behaviour.aimWeapon && playerController.isAiming ? secondAttackDelay : firstAttackDelay);

        TriggerAttack();

        //Local

        if (revolverShake) CameraRevolverAnimation();
        else CameraAnimation();
    }

    bool hitOK;

    private void TriggerAttack()
    {
        ShootServerEffect(behaviour.aimWeapon && playerController.isAiming ? 1 : 0);
        collisionScript.canHit = true;
        collisionScript.canHitEnvi = true;
        if (bforce && propulsion) playerController.BForce(cam.transform.forward, (behaviour.aimWeapon && playerController.isAiming ? secondPropulsionAmount : basePropulsionAmount), true, false, bforceDecel, true);
        else if (propulsion) playerController.CustomAddForce(cam.transform.forward, (behaviour.aimWeapon && playerController.isAiming ? secondPropulsionAmount : basePropulsionAmount));
    }

    public void BounceHolder()
    {
        if (!bounceHolder) return;
        if (playerKnockback != 0) playerController.AddForce(-cam.transform.forward, playerKnockback);
    }

    public void HitServer(PlayerHealth enemyHealth, Vector3 hitPosition, Vector3 hitNormal, string hitName)
    {

        BumpPlayerServer(cam.transform.forward + Vector3.up * 2, repulseForce, enemyHealth);

        var damageToGive = secondAttackPlaying ? secondAttackDamage : baseAttackDamage;

        if (enemyHealth.gameObject == transform.root.gameObject) 
           return;
        
        // Friendly fire check
        if (FriendlyFireCheck(enemyHealth)) { return; }

        if (hitName == "Head_Col"){
                        damageToGive *= headMultiplier;
                        Settings.Instance.IncreaseHeadshotsAmount();
                    }
                    else Settings.Instance.IncreaseBodyshotsAmount();

        Instantiate(bodyImpact, hitPosition, Quaternion.LookRotation(hitNormal));
        Instantiate(bloodSplatter, hitPosition, Quaternion.identity);
        if (behaviour.aimWeapon && playerController.isAiming) {
            Instantiate(headImpact, hitPosition, Quaternion.LookRotation(hitNormal));
            Instantiate(genericBodyImpact, hitPosition, Quaternion.LookRotation(hitNormal));
        }

        if (marker == null) {
                        marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
                        marker.transform.DOPunchScale((hitName == "Head_Col" ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
                        if (hitName == "Head_Col") {
                            marker.GetComponent<Image>().color = Color.red;
                        }
                        Destroy(marker, 0.3f);
                    }
                    else{
                        Destroy(marker);
                        marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
                        marker.transform.DOPunchScale((hitName == "Head_Col" ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
                        if (hitName == "Head_Col") {
                            marker.GetComponent<Image>().color = Color.red;
                        }
                        Destroy(marker, 0.3f);
                    }

        if (hitName == "Head_Col")
        {
            LocalSound(0);
            LocalSound(1);
        }
        else {
            LocalSound(1);
        }


        if (enemyHealth.health-damageToGive <= 0) {
            LocalSound(2);
            enemyHealth.Explode(false, false, hitName, cam.transform.forward, ragdollEjectForce, hitPosition);
            enemyHealth.graphics.SetActive(false);
            enemyHealth.GetComponent<CharacterController>().enabled = false;
            PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was {(hitName == "Head_Col" ? "beheaded" : "slain")} with {(StartsWithVowel(behaviour.weaponName) ? "an" : "a")} <b><color=white>{behaviour.weaponName}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
            KillShockWave();
            KillServer(enemyHealth);
            enemyHealth.DisablePlayerObjectWhenKilled();
        }
        else GiveDamage(damageToGive, enemyHealth, hitName);
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

        NextRound();
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

    [ObserversRpc]
    void NextRound()
    {
        //playerValues.playerClient.StartCoroutine(playerValues.playerClient.NextRound());
    }

    [ServerRpc (RunLocally = true)]
    private void ShootServerEffect(int x)
    {
        ShootObserversEffect(x);
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

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void ShootObserversEffect(int x)
    {
        if (x == 0) audio.PlayOneShot(fireClip);
        else if (x == 1) audio.PlayOneShot(secondAttackClip);
        else if (x == 2) audio.PlayOneShot(firstAttackStartClip);
        else if (x == 3) audio.PlayOneShot(secondAttackStartClip);
        


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
        if (index == 3)
            audio.PlayOneShot(secondAttackClip);
    }
    

}
