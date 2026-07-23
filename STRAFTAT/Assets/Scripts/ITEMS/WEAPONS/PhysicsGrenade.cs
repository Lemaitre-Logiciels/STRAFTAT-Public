using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;
using FishNet.Object;
using Random = UnityEngine.Random;

public class PhysicsGrenade : NetworkBehaviour
{
    [SerializeField] private string weaponName;
    public bool isOwner;
    private Vector3 impact = Vector3.zero;
    private CharacterController character;

    [SerializeField] private float ragdollEjectForce;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject explosionVfx;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip swooshClip;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector3 rotateAxis;
    [SerializeField] private Vector3 vfxScale = Vector3.one;
    [SerializeField] private Transform graph;
    [SerializeField] private GameObject explosionDecal;
    [SerializeField] private GameObject bloodSplatter;

    [SerializeField] private float timeBeforeExplosion = 2;
    [SerializeField] private float explosionRadius = 3f;
    private float explosionTimer;

    [SerializeField] private float rebondForce;
    bool isGrounded;

    [Header("Screenshake values")]
    [SerializeField] private float duration;
    [SerializeField] private float minStrength;
    [SerializeField] private float maxStrength;
    [SerializeField] private int vibrato;
    [SerializeField] private float randomness;
    [SerializeField] private Ease shakeEase;
    [SerializeField] private float maxDistance;

    bool touched;
    bool touched2;
    GameObject _gun;
    PlayerHealth[] ph2;
    private float _passedTime = 0f;
    private GameObject _rootObject;
    private AudioSource audio;
    
    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;

    [SerializeField] private bool fragGrenade;
    [SerializeField] private float numberOfRays;

    [SerializeField] private bool stunGrenade;
    [SerializeField] private float stunTime;

    float safeTimer;

    void OnEnable()
    {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnBeforeSpawn -= StartNewRound;

    }

    void StartNewRound()
    {
        if (IsServer) { Despawn(); }
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
    
    private void Start() {
        isOwner = Owner.IsLocalClient;
    }

    [ObserversRpc(RunLocally = true)]
    public void Initialize(GameObject rootObject, GameObject gun, float passedTime, float grenadeOpenSince)
    {
        _rootObject = rootObject;
        _passedTime = passedTime;
        _gun = gun;

        explosionTimer = grenadeOpenSince;
        audio.PlayOneShot(swooshClip);
    }

    private bool exploded = false;

    void Update()
    {
        float delta = Time.deltaTime;
        float passedTimeDelta = 0f;
        velocity = lastPosition - currentPosition;
        explosionTimer -= (delta + passedTimeDelta);

        graph.Rotate(rotateAxis * rotateSpeed * Time.deltaTime);

        
        //Handle Explosion after timeout
        if (explosionTimer < 0 && explosionTimer > -2 && IsOwner && !exploded) {
            exploded = true;
            if (PauseManager.BetweenRounds) { return; }
            ServerHandlerExplosion(transform.position);
        }

        lastPosition = currentPosition;
    }

    bool makeBlood = true;

    [ServerRpc(RunLocally = true)]
    private void ServerHandlerExplosion(Vector3 position) {
        HandleExplosion(position);
    }

    [ObserversRpc(RunLocally = true, ExcludeOwner = true)]
    private void HandleExplosion(Vector3 position) {
        int roundedX = Mathf.RoundToInt(position.x / 5f) * 5;
        int roundedZ = Mathf.RoundToInt(position.z / 5f) * 5;
        int randomSeed = roundedX * 10000 + roundedZ;
        
        transform.position = position;
        
        Collider[] explosionColliders = new Collider[1];
        List<Collider> tempRayHits = new List<Collider>();
        if (!fragGrenade) explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
        else {
            System.Random localRandom = new System.Random(randomSeed);
            for (int i=0; i < numberOfRays; i++)
            {
                double theta = localRandom.NextDouble() * 2 * Mathf.PI;
                double phi = Math.Acos(2 * localRandom.NextDouble() - 1);
                float x = (float)(Math.Sin(phi) * Math.Cos(theta));
                float y = (float)(Math.Sin(phi) * Math.Sin(theta));
                float z = (float)(Math.Cos(phi));
                Vector3 tempDir = new Vector3(x, y, z).normalized;

                RaycastHit fragHit;
                if (Physics.Raycast(transform.position, tempDir, out fragHit, explosionRadius, bodyLayer))
                    tempRayHits.Add(fragHit.transform.GetComponent<Collider>());

                RenderObject(tempDir, (fragHit.collider == null ? explosionRadius : Vector3.Distance(transform.position, fragHit.point)));
            }
            explosionColliders = new Collider[tempRayHits.Count];
            for (int i=0; i < tempRayHits.Count; i++)
            {
                explosionColliders[i] = tempRayHits[i];
            }
        }

        if (explosionColliders.Length != 0)
        {
            ph2 = new PlayerHealth[explosionColliders.Length];
            for (int i = 0; i < explosionColliders.Length; i++)
            {
                if (explosionColliders[i].transform.tag == "ShatterableGlass")
                {
                    if (explosionColliders[i].gameObject.GetComponent<ShatterableGlass>() != null) explosionColliders[i].gameObject.GetComponent<ShatterableGlass>().Shatter3D(explosionColliders[i].transform.position, explosionColliders[i].transform.position - transform.position);
                }
                    

                if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null) {
                    ph2[i] = explosionColliders[i].GetComponentInParent<PlayerHealth>();
                    if (bloodSplatter && makeBlood && !stunGrenade) {
                        Instantiate(bloodSplatter, explosionColliders[i].transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));
                        makeBlood = false;
                    }
                }
            }

            for (int i = 0; i < ph2.Length; i++) {
                if (ph2[i] != null && isOwner) {

                    if (ph2[i].isKilled == false) {
                            
                        if (!stunGrenade)  {
                            ph2[i].ChangeKilledState(true);
                            ph2[i].RemoveHealth(10);
                                

                            if (ph2[i].transform.gameObject == _rootObject) {
                                IncreaseSuicidesAmount();
                                ph2[i].suicide = true;
                            }
                            else {
                                KillShockWave();
                                SendKillLog(ph2[i]);
                            }

                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                            ph2[i].SetKiller(_rootObject.transform);
                        }

                        if (stunGrenade) ph2[i].TaserEnemy(ph2[i], stunTime);

                    }
                }
            }
        }

        var players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            var distance = Vector3.Distance(transform.position, players[i].transform.position);
            players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
        }
                
            

        Destroy(gameObject, 3);
        this.enabled = false;
        graph.gameObject.SetActive(false);
        if (!fragGrenade) {
            GameObject obj = Instantiate(explosionVfx, transform.position, Quaternion.identity);
            if (obj.transform.Find("ball") != null) obj.transform.Find("ball").localScale = this.vfxScale;
        }
        Instantiate(explosionDecal, transform.position, Quaternion.identity);
        audio.Play();
    }

    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        //if (sendKillLog) return;
        sendKillLog = true;

        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was killed with a <b><color=white>{weaponName}</color></b> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
    }

    bool suicide = true;
    void IncreaseSuicidesAmount(){

        if (suicide) {
            suicide = false;
            Settings.Instance.IncreaseSuicidesAmount();
        }
    }

    bool increaseKillAmount;
    public void KillShockWave()
    {
        //if (!increaseKillAmount){
            Settings.Instance.IncreaseKillsAmount();
            increaseKillAmount = true;
        //} 
        _rootObject.GetComponent<FirstPersonController>().lensDistortion.intensity.value = _rootObject.GetComponent<FirstPersonController>().killShockWaveStrength;
        _rootObject.GetComponent<FirstPersonController>().colorGrading.saturation.value = -100;
    }


    void RenderObject(Vector3 direction, float maxDistance)
    {
        GameObject lineObject = new GameObject("RaycastLine");

        // Add and configure the LineRenderer component
        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        var lineFx = lineObject.AddComponent<LineFade>();
        lineFx.decreaseInSize = false;
        lineFx.decreaseInSizeSpeed = 10;
        lineFx.color = Color.white;
        lineFx.speed = 9;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        var rayOrigin = transform.position;
        var rayDirection = direction;
        
        lineRenderer.SetPosition(0, rayOrigin);
        lineRenderer.SetPosition(1, rayOrigin + rayDirection * maxDistance);
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        Destroy(lineObject, 2.5f);
    }

    int maxHits = 5;
    int hits = 0;
    private void OnCollisionEnter(Collision col)
    {
        //prevent audio spam if the grenade gets stuck somehow
        if (hits > maxHits)
            return;

        //dont play sound if grenade exists but has exploded (waiting for destroy)
        if (explosionTimer < 0.0f)
            return;

        audio.PlayOneShot(hitClip, 0.56f);
        hits++;
    }
}
