using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;
using FishNet.Object;

public class Bubble : NetworkBehaviour
{
    public bool isOwner;
    private Vector3 impact = Vector3.zero;
    private CharacterController character;

    [SerializeField] private float ragdollEjectForce;

    [SerializeField] private float damage;
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private Transform graph;

    [SerializeField] private float timeBeforeDestroy = 5;
    private float explosionTimer;

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
    PlayerHealth ph2;
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

    void StartNewRound() {
        if (IsServer) { Despawn(); }
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    public void Start() { isOwner = Owner.IsLocalClient; }

    [ObserversRpc(RunLocally = true)]
    public void Initialize(GameObject rootObject, GameObject gun, float passedTime)
    {
        _rootObject = rootObject;
        _passedTime = passedTime;
        _gun = gun;

        explosionTimer = timeBeforeDestroy;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isOwner) { return; }
        
        if (other.transform.gameObject.layer == 11 || other.transform.gameObject.layer == 16) {
            ph2 = other.transform.GetComponentInParent<PlayerHealth>();
            if (ph2.health - damage <=0 ) {
                ph2.ChangeKilledState(true);
                KillShockWave();
                SendKillLog(ph2);
                ph2.Explode(false, true, ph2.gameObject.name, ph2.transform.position - transform.position, ragdollEjectForce, transform.position);
            }
            ph2.SetKiller(_rootObject.transform);
            ph2.RemoveHealth(damage);
            Instantiate(hitVfx, transform.position, Quaternion.identity);

            HandleExplosion(transform.position);
        }
    }


    void Update()
    {
        float delta = Time.deltaTime;
        float passedTimeDelta = 0f;
        velocity = lastPosition - currentPosition;
        //transform.rotation = Quaternion.LookRotation(-velocity);
        explosionTimer -= (delta + passedTimeDelta);

        //graph.Rotate(rotateAxis * rotateSpeed * Time.deltaTime);

        if (IsOwner && explosionTimer < 0) { HandleExplosion(transform.position); }

        lastPosition  = currentPosition;

    }
    
    private bool calledExplode = false;
    private void HandleExplosion(Vector3 position) {
        if (calledExplode) return;
        calledExplode = true;
        HandleExplosionServer(position);
    }
    
    [ServerRpc(RunLocally = true)]
    private void HandleExplosionServer(Vector3 position) { Explode(position); }

    [ObserversRpc(RunLocally = true, ExcludeOwner = true)]
    void Explode(Vector3 position) {
        transform.position = position;
        
        Destroy(gameObject, 3);
        this.enabled = false;
        GetComponent<Collider>().enabled = false;
        graph.gameObject.SetActive(false);
        Instantiate(hitVfx, transform.position, Quaternion.identity);
        audio.Play();

        var players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                var distance = Vector3.Distance(transform.position, players[i].transform.position);
                players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
            }
    }

    bool increaseKillAmount;
    public void KillShockWave()
    {
        if (!increaseKillAmount){
            Settings.Instance.IncreaseKillsAmount();
            increaseKillAmount = true;
        } 
        _rootObject.GetComponent<FirstPersonController>().lensDistortion.intensity.value = _rootObject.GetComponent<FirstPersonController>().killShockWaveStrength;
        _rootObject.GetComponent<FirstPersonController>().colorGrading.saturation.value = -100;
    }

    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        if (sendKillLog) return;
        sendKillLog = true;

        PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{enemyHealth.playerValues.playerClient.PlayerNameTag}</color></b> was killed with the Bublee by <b><color=#{PauseManager.Instance.enemyNameLogColor}>{ClientInstance.Instance.PlayerNameTag}</color></b>");
    }

    
}
