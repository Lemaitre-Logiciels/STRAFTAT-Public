using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PredictedProjectile : MonoBehaviour
{
    [HideInInspector] public bool isOwner;
    private Vector3 _direction;
    private float _passedTime = 0f;
    private float actualPassedTime = 0f;
    [SerializeField] private float MOVE_RATE = 5f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private bool explosionBullet = true;
    [SerializeField] private bool spawnObject = false;
    [SerializeField] private GameObject objToSpawn ;
    [SerializeField] private float bumpForce;
    [SerializeField] private bool orientateVfx = true;
    [SerializeField] private float launchOffset;
    [SerializeField] private bool shouldDestroy = true;
    [SerializeField] private float damage = 100f;
    [SerializeField] private bool skipSafeStartTimer;
    [SerializeField] private bool destroySelf;
    [SerializeField] private float ragdollEjectForce = 4;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    [SerializeField] private LayerMask headLayer;
    
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip launchClip;
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject bloodVfx;
    [SerializeField] private GameObject headBloodVfx;
    [SerializeField] private GameObject bloodSplatter;
    private GameObject _rootObject;
    private AudioSource audio;

    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityStart;
    [SerializeField] private float gravity;
    float _gravity;

    [SerializeField] private bool usePhysics;
    [SerializeField] private float friction;
    float _force;

    [Header("Screenshake values")]
    [SerializeField] private float duration;
    [SerializeField] private float minStrength;
    [SerializeField] private float maxStrength;
    [SerializeField] private int vibrato;
    [SerializeField] private float randomness;
    [SerializeField] private Ease shakeEase;
    [SerializeField] private float maxDistance;

    GameObject _gun;
    [SerializeField] GameObject graph;
    PlayerHealth[] ph2;
    PlayerHealth[] ph3;
    bool backupRaycast;

    [HideInInspector] public Weapon weapon;

    [Space]
    [SerializeField] private GameObject explosionDecal;

    [ToggleGroup("SurfacesImpact")]
	[SerializeField] private bool SurfacesImpact;

	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject concreteHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject sandHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject dirtHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject metalHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject tauleHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject waterHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject woodHitImpact;
	[ToggleGroup("SurfacesImpact")] [SerializeField] private GameObject softbodyHitImpact;

    [ToggleGroup("SurfacesVFX")]
	[SerializeField] private bool SurfacesVFX;

	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject sandHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject dirtHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject metalHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject tauleHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject waterHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject woodHitFx;
	[ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject softbodyHitFx;
    [ToggleGroup("SurfacesVFX")] [SerializeField] private GameObject bulletHole;

    void OnEnable()
    {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnBeforeSpawn -= StartNewRound;
    }
    
    void OnHit(Vector3 position, Vector3 normal) {
        if (!spawnObject) { return; }
        GameObject obj = Instantiate(objToSpawn, position, Quaternion.identity);
        obj.transform.up = normal;
    }

    void StartNewRound()
    {
        Destroy(gameObject);
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        _gravity = gravityStart;
    }

    public void Initialize(Vector3 direction, float force, float passedTime, GameObject rootObject, GameObject gun)
    {
        _direction = direction;
        _passedTime = passedTime;
        _rootObject = rootObject;
        _force = force;
        _gun = gun;

        transform.rotation = Quaternion.LookRotation(-direction);
        transform.position += rootObject.transform.forward * launchOffset;
    }

    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;
    bool headshot;
    bool isGlass;

    void Update()
    {
        currentPosition = transform.position;

        Move();

        HandleCollision();

        lastPosition  = currentPosition;
    }

    float distanceTraveled;

    private void Move()
    {
        velocity = lastPosition - currentPosition;
        if (!useGravity) transform.rotation = Quaternion.LookRotation(_direction);
        else if (velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(-velocity.normalized);
        //Frame delta, nothing unusual here.
        float delta = Time.deltaTime;

        //See if to add on additional delta to consume passed time.
        float passedTimeDelta = 0f;
        if (_passedTime > 0f)
        {

            float step = (_passedTime * 0.08f);
            _passedTime -= step;

            if (_passedTime <= (delta / 2f))
            {
                step += _passedTime;
                _passedTime = 0f;
            }
            passedTimeDelta = step;
        }

        actualPassedTime += (delta + passedTimeDelta);

        if (useGravity) _gravity += gravity * (delta + passedTimeDelta);
        if (usePhysics) _force -= friction * (delta + passedTimeDelta);

        //Move the projectile using moverate, delta, and passed time delta.
        transform.position += _direction * ((usePhysics ? _force : MOVE_RATE) * (delta + passedTimeDelta));
        distanceTraveled += ((usePhysics ? _force : MOVE_RATE) * (delta + passedTimeDelta));
        if (distanceTraveled > 150 && destroySelf) {
            Destroy(gameObject, 3);
            this.enabled = false;
            graph.SetActive(false);
        }

        if (useGravity)
        {
            transform.position -= Vector3.up * (_gravity * (delta + passedTimeDelta));
        }
    }

    [SerializeField] private float backupRayLength = 1.5f;
    bool touched;
    bool touched2;
    [SerializeField] private bool prophet;
    bool prophetHit;

    private void HandleCollision()
    {
        if (prophet && prophetHit) return;

        if (!skipSafeStartTimer && actualPassedTime < 0.1f) return; // DO not collide in the first frames when spawned

        backupRaycast = Physics.Raycast(transform.position - transform.forward, transform.forward, out RaycastHit rayHit, backupRayLength, playerLayer);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, playerLayer);

        if (Physics.Raycast(transform.position, new Vector3(transform.forward.x, 0, transform.forward.z), out RaycastHit hita2, 2, headLayer))
                if (hita2.transform.gameObject.name == "Head_Col" || hita2.transform.gameObject.name == "Neck_1_Col") headshot = true;
        Debug.DrawRay(transform.position, new Vector3(transform.forward.x, 0, transform.forward.z), Color.red, 1);
        
        if (hitColliders.Length != 0)
        {
            isGlass = false;
            bool gunCollision = false;

            for (int i = 0; i < hitColliders.Length; i++) {
                if (hitColliders[i].transform.tag == "ShatterableGlass")
                {
                    isGlass = true;
                    if (hitColliders[i].gameObject.GetComponent<ShatterableGlass>() != null)  hitColliders[i].gameObject.GetComponent<ShatterableGlass>().Shatter3D(hitColliders[i].transform.position, hitColliders[i].transform.position - transform.position);
                }
            }
            
            for (int i = 0; i < hitColliders.Length; i++) {
                if (hitColliders[i].gameObject == _gun)
                    gunCollision = true;
                else gunCollision = false;
            }

            bool rootCollision = false;
            
            for (int i = 0; i < hitColliders.Length; i++) {
                if (hitColliders[i].gameObject == _rootObject)
                    rootCollision = true;
                else rootCollision = false;
            }

            

            if (!gunCollision && !rootCollision)
            {
                PlayerHealth ph = hitColliders[0].GetComponentInParent<PlayerHealth>();

                if (ph != null) {
                    if ((shouldDestroy ? ph != _rootObject.GetComponent<PlayerHealth>() : 1==1)) {

                        if (isOwner && !ph.isKilled){
                            if (headshot) damage *= 2;
                            if (headshot) Settings.Instance.IncreaseHeadshotsAmount();
                            else Settings.Instance.IncreaseBodyshotsAmount();
                            if (ph.health-damage <= 0) ph.ChangeKilledState(true);
                            ph.SetKiller(_rootObject.transform);
                            if (ph.health-damage <= 0) {
                                ph.Explode(false, true, ph.gameObject.name, ph.transform.position - transform.position, ragdollEjectForce, transform.position);
                                KillShockWave();
                                SendKillLog(ph);
                            }
                            ph.RemoveHealth(damage);
                            //if (prophet) prophetHit = true;
                            HitMarker(headshot);
                            Instantiate(bloodVfx, transform.position, Quaternion.identity);
                            if (headshot && headBloodVfx) Instantiate(headBloodVfx, transform.position, Quaternion.identity);
                        }

                        if (bloodSplatter) Instantiate(bloodSplatter, ph.transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));

                        if (shouldDestroy) {
                            Destroy(gameObject, 3);
                            this.enabled = false;
                            graph.SetActive(false);
                            var players = GameObject.FindGameObjectsWithTag("Player");
                            for (int i = 0; i < players.Length; i++)
                            {
                                var distance = Vector3.Distance(transform.position, players[i].transform.position);
                                players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
                            }
                        }
                        audio.Play();

                
                    }
                }

                if (InstanceFinder.IsClient && !gunCollision && !rootCollision && shouldDestroy && !isGlass && ph == null)
                {
                    if (Physics.SphereCast(transform.position - transform.forward, radius*1.4f, transform.forward, out RaycastHit hita, 1, playerLayer)) {
                        Instantiate(hitVfx, transform.position, (orientateVfx ? Quaternion.LookRotation(hita.normal) : Quaternion.identity));
                        OnHit(transform.position, hita.normal);
                        if (SurfacesImpact) SpawnVFX(0, transform.position, hita.normal, hita.transform.tag, hita.transform);
                        if (SurfacesVFX) SpawnVFX(1, transform.position, hita.normal, hita.transform.tag, hita.transform);
                    }
                }

                Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
                if (explosionColliders.Length != 0 && !isGlass && explosionRadius > 0)
                {
                    Glass();
                    List<PlayerHealth> ph2List = new List<PlayerHealth>();
                    for (int i = 0; i < explosionColliders.Length; i++)
                    {
                        if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null) {
                            if (!ph2List.Contains(explosionColliders[i].GetComponentInParent<PlayerHealth>())) ph2List.Add(explosionColliders[i].GetComponentInParent<PlayerHealth>());
                        }
                    }

                    ph2 = ph2List.ToArray();

                    for (int i = 0; i < ph2.Length; i++) {
                        if (ph2[i] != null && !ph2[i].isKilled) {
                            
                            if (isOwner) ph2[i].SetKiller(_rootObject.transform);

                            if (bloodSplatter) Instantiate(bloodSplatter, ph2[i].transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));

                            if (ph2[i].health-damage <= 0 && isOwner) 
                            {
                                if (ph2[i].playerValues.playerClient == weapon.playerValues.playerClient) {
                                    ph2[i].ChangeKilledState(true);
                                    ph2[i].suicide = true;
                                    Settings.Instance.IncreaseSuicidesAmount();
                                }
                                else if (ph2[i].playerValues.playerClient != weapon.playerValues.playerClient)
                                {
                                    ph2[i].ChangeKilledState(true);
                                    ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                                    KillShockWave();
                                    SendKillLog(ph2[i]);
                                }
                            }

                            if (isOwner) ph2[i].RemoveHealth(damage);
                            if (isOwner) HitMarker(headshot);

                            touched2 = true;
                            touched = true;
                        }

                        
                    }
                }
            }

            if (!gunCollision && !rootCollision && shouldDestroy && !isGlass) {
                Destroy(gameObject, 3);
                if (explosionDecal) Instantiate(explosionDecal, transform.position, Quaternion.identity);
                this.enabled = false;
                graph.SetActive(false);
                audio.Play();

                var players = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < players.Length; i++)
                {
                    var distance = Vector3.Distance(transform.position, players[i].transform.position);
                    players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
                    
                }
            }
        }
        else if (backupRaycast && !touched && !touched2 && shouldDestroy)
        {
            isGlass = false;

            if (rayHit.transform.gameObject.tag == "ShatterableGlass")
            {
                isGlass = true;
                if (rayHit.transform.gameObject.GetComponent<ShatterableGlass>() != null) rayHit.transform.gameObject.GetComponent<ShatterableGlass>().Shatter3D(rayHit.point, rayHit.point - transform.position);
            }
            

            bool gunCollision = false;

            

            if (rayHit.transform.gameObject == _gun)
                gunCollision = true;
            else gunCollision = false;

            bool rootCollision = false;
            
            if (rayHit.transform.gameObject == _rootObject)
                rootCollision = true;
            else rootCollision = false;

            

            if (!gunCollision && !rootCollision)
            {
                PlayerHealth ph = rayHit.transform.GetComponentInParent<PlayerHealth>();

                if (ph != null) {

                    if ((shouldDestroy ? ph != _rootObject.GetComponent<PlayerHealth>() : 1==1)) {
                        if (isOwner && !ph.isKilled){
                            if (headshot) damage *= 2;
                            if (headshot) Settings.Instance.IncreaseHeadshotsAmount();
                            else Settings.Instance.IncreaseBodyshotsAmount();
                            if (ph.health-damage <= 0) ph.ChangeKilledState(true);
                            ph.SetKiller(_rootObject.transform);
                            if (ph.health-damage <= 0) {
                                ph.Explode(false, true, ph.gameObject.name, ph.transform.position - transform.position, ragdollEjectForce, transform.position);
                                KillShockWave();
                                SendKillLog(ph);
                            }
                            ph.RemoveHealth(damage);
                            //if (prophet) prophetHit = true;
                            HitMarker(headshot);
                            Instantiate(bloodVfx, transform.position, Quaternion.identity);
                            if (headshot && headBloodVfx) Instantiate(headBloodVfx, transform.position, Quaternion.identity);
                            
                        }

                        if (bloodSplatter) Instantiate(bloodSplatter, ph.transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));

                        if (shouldDestroy) {
                            Destroy(gameObject, 3);
                            this.enabled = false;
                            graph.SetActive(false);
                            var players = GameObject.FindGameObjectsWithTag("Player");
                            for (int i = 0; i < players.Length; i++)
                            {
                                var distance = Vector3.Distance(transform.position, players[i].transform.position);
                                players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
                            }
                        }
                        audio.Play();

                
                    }
                }

                if (InstanceFinder.IsClient && !gunCollision && !rootCollision && shouldDestroy && !isGlass && ph == null)
                {
                    Instantiate(hitVfx, transform.position, (orientateVfx ? Quaternion.LookRotation(rayHit.normal) : Quaternion.identity));
                    OnHit(transform.position, rayHit.normal);

                    if (SurfacesImpact) SpawnVFX(0, transform.position, rayHit.normal, rayHit.transform.tag, rayHit.transform);
                    if (SurfacesVFX) SpawnVFX(1, transform.position + transform.forward*1.5f, rayHit.normal, rayHit.transform.tag, rayHit.transform);
                }

                Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
                if (explosionColliders.Length != 0 && !isGlass && explosionRadius > 0)
                {
                    Glass();
                    List<PlayerHealth> ph2List = new List<PlayerHealth>();
                    for (int i = 0; i < explosionColliders.Length; i++)
                    {
                        if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null) {
                            if (!ph2List.Contains(explosionColliders[i].GetComponentInParent<PlayerHealth>())) ph2List.Add(explosionColliders[i].GetComponentInParent<PlayerHealth>());
                        }
                    }

                    ph2 = ph2List.ToArray();

                    for (int i = 0; i < ph2.Length; i++) {
                        if (ph2[i] != null && !ph2[i].isKilled) {
                            
                            if (isOwner) ph2[i].SetKiller(_rootObject.transform);

                            if (bloodSplatter) Instantiate(bloodSplatter, ph2[i].transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));

                            if (ph2[i].health-damage <= 0 && isOwner) 
                            {
                                if (ph2[i].playerValues.playerClient == weapon.playerValues.playerClient) {
                                    ph2[i].ChangeKilledState(true);
                                    ph2[i].suicide = true;
                                    Settings.Instance.IncreaseSuicidesAmount();
                                }
                                else if (ph2[i].playerValues.playerClient != weapon.playerValues.playerClient)
                                {
                                    ph2[i].ChangeKilledState(true);
                                    ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                                    KillShockWave();
                                    SendKillLog(ph2[i]);
                                }
                            }

                            if (isOwner) ph2[i].RemoveHealth(damage);
                            if (isOwner) HitMarker(headshot);

                            touched2 = true;
                            touched = true;
                        }

                        
                    }
                }
            }

            if (!gunCollision && !rootCollision && shouldDestroy && !isGlass) {
                Destroy(gameObject, 3);
                this.enabled = false;
                graph.SetActive(false);
                audio.Play();

                var players = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < players.Length; i++)
                {
                    var distance = Vector3.Distance(transform.position, players[i].transform.position);
                    players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
                }
            }


            
        }
            
    }

    [SerializeField] private GameObject hitMarker;
    [SerializeField] private AudioClip hitSfx;
    private GameObject marker;
    private void HitMarker(bool head)
    {
        audio.PlayOneShot(hitSfx);
        if (head) audio.PlayOneShot(Crosshair.Instance.headshotHitClip);
        if (marker == null) {
            marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
            marker.transform.DOPunchScale((head ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
            if (head) {
                            marker.GetComponent<Image>().color = Color.red;
                        }

            Destroy(marker, 0.3f);
        }
        else{
            Destroy(marker);
            marker = Instantiate(hitMarker, Crosshair.Instance.transform.position, Quaternion.identity, PauseManager.Instance.transform);
            marker.transform.DOPunchScale((head ? new Vector3(2.5f, 2.5f, 2.5f) : Vector3.one), 0.3f, 8, 2);
            if (head) {
                            marker.GetComponent<Image>().color = Color.red;
                        }
            Destroy(marker, 0.3f);
        }

    }

    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        //if (sendKillLog) return;
        sendKillLog = true;

        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was {(headshot ? "headshot" : "killed")} with {(StartsWithVowel(weapon.transform.GetComponent<ItemBehaviour>().weaponName) ? "an" : "a")} <b><color=white>{weapon.transform.GetComponent<ItemBehaviour>().weaponName}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
    }

    public void KillShockWave()
    {
        Settings.Instance.IncreaseKillsAmount();
        _rootObject.GetComponent<FirstPersonController>().lensDistortion.intensity.value = _rootObject.GetComponent<FirstPersonController>().killShockWaveStrength;
        _rootObject.GetComponent<FirstPersonController>().colorGrading.saturation.value = -100;
    }

    void Glass()
    {
        var explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);

        for (int i = 0; i < explosionColliders.Length; i++)
        {
            if (explosionColliders[i].transform.tag == "ShatterableGlass")
            {
                if (explosionColliders[i].gameObject.GetComponent<ShatterableGlass>() != null) explosionColliders[i].gameObject.GetComponent<ShatterableGlass>().Shatter3D(explosionColliders[i].transform.position, explosionColliders[i].transform.position - transform.position);
            }
        }
    }

    private void SpawnVFX(int index, Vector3 hitPoint, Vector3 hitNormal, string surface, Transform parent)
    {
        if (Settings.Instance.reduceVFX) { return; }

        if (parent.gameObject.layer == 3) return;
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
        
    }

    public bool StartsWithVowel(string name) {
        if (name.StartsWith("a") || name.StartsWith("e") || name.StartsWith("i") || name.StartsWith("o") || name.StartsWith("u") || name.StartsWith("y")) return true;
        else return false;
    }

    
}
