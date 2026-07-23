using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;
using FishNet.Object;

public class Obus : NetworkBehaviour
{
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
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector3 rotateAxis;
    [SerializeField] private Transform graph;
    [SerializeField] private GameObject explosionDecal;

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


    float safeTimer;
    bool hit;

    void OnEnable()
    {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnBeforeSpawn -= StartNewRound;

    }

    void StartNewRound() { if (IsServer) { Despawn(); } }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    private void Start() {
        isOwner = Owner.IsLocalClient;
    }

    [ObserversRpc(RunLocally = true)]
    public void Initialize(GameObject rootObject, GameObject gun, float passedTime)
    {
        _rootObject = rootObject;
        _passedTime = passedTime;
        _gun = gun;

        explosionTimer = timeBeforeExplosion;
    }

    private void OnCollisionEnter()
    {
        if (!isOwner) return;
        hit = true;
    }

    bool calledExplode = false;

    void Update()
    {
        float delta = Time.deltaTime;
        float passedTimeDelta = 0f;
        velocity = lastPosition - currentPosition;
        if (velocity.sqrMagnitude > 0f) { transform.rotation = Quaternion.LookRotation(-velocity); }
        if (hit) explosionTimer -= (delta + passedTimeDelta);

        //graph.Rotate(rotateAxis * rotateSpeed * Time.deltaTime);

        if (explosionTimer < 0 && explosionTimer > -2 && IsOwner && !calledExplode) {
            calledExplode = true;
            ExplodeServer(transform.position);
        }
        lastPosition = currentPosition;
    }

    [ServerRpc(RunLocally = true)]
    private void ExplodeServer(Vector3 position) {
        HandleExplosion(position);
    }
    
    [ObserversRpc(RunLocally = true, ExcludeOwner = true)]
    private void HandleExplosion(Vector3 position) {
        transform.position = position;

        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);

        if (explosionColliders.Length != 0 && isOwner)
        {
            ph2 = new PlayerHealth[explosionColliders.Length];
            for (int i = 0; i < explosionColliders.Length; i++)
            {
                if (explosionColliders[i].transform.tag == "ShatterableGlass")
                {
                    if (explosionColliders[i].gameObject.GetComponent<ShatterableGlass>() != null) explosionColliders[i].gameObject.GetComponent<ShatterableGlass>().Shatter3D(explosionColliders[i].transform.position, explosionColliders[i].transform.position - transform.position);
                }
                    

                if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null)
                    ph2[i] = explosionColliders[i].GetComponentInParent<PlayerHealth>();
            }

            for (int i = 0; i < ph2.Length; i++) {
                if (ph2[i] != null && !ph2[i].isKilled) {

                    if (ph2[i].transform.gameObject == _rootObject) {
                        Settings.Instance.IncreaseSuicidesAmount();
                        ph2[i].ChangeKilledState(true);
                        ph2[i].RemoveHealth(10);
                        ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        ph2[i].SetKiller(_rootObject.transform);
                        touched = true;
                    }
                    else if (ph2[i].transform.gameObject != _rootObject) {
                        ph2[i].ChangeKilledState(true);
                        ph2[i].RemoveHealth(10);
                        KillShockWave();
                        SendKillLog(ph2[i]);
                        ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        ph2[i].SetKiller(_rootObject.transform);
                        touched2 = true;
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
        Instantiate(explosionVfx, transform.position, Quaternion.identity);
        Instantiate(explosionDecal, transform.position, Quaternion.identity);
        audio.Play();
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

    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        //if (sendKillLog) return;
        sendKillLog = true;

        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was killed with a <color=white>grenade launcher</color> by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
    }

    
}
