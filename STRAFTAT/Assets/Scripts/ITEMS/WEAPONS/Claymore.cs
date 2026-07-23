using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using DG.Tweening;

public class Claymore : NetworkBehaviour
{
    [SerializeField] private Vector3 boxdimensions;
    [SerializeField] private string weaponName;
    float rayLength;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    [SyncVar] public bool detonated;
    [SerializeField] private float ragdollEjectForce = 3;
    
    [SerializeField] private GameObject explosionVfx;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip bipClip;
    [SyncVar] public GameObject _rootObject;
    [SerializeField] private GameObject graph;
    [SerializeField] private AudioClip activationSound;
    public bool canExplode = false;
    private AudioSource audio;
    private float bipTimer;
    PlayerHealth[] ph2;

    [Header("Screenshake values")]
    [SerializeField] private float duration;
    [SerializeField] private float minStrength;
    [SerializeField] private float maxStrength;
    [SerializeField] private int vibrato;
    [SerializeField] private float randomness;
    [SerializeField] private Ease shakeEase;
    [SerializeField] private float maxDistance;

    [SyncVar] public  Weapon weapon;
    public bool isOwner;

    void OnEnable()
    {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnBeforeSpawn -= StartNewRound;

        if (lineObject != null)  Destroy(lineObject);
    }

    void StartNewRound() {
        if (IsServer) { Despawn(); }
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(ActivateTrap());
        
    }

    IEnumerator ActivateTrap()
    {
        
        yield return new WaitForSeconds(1);
        canExplode = true;
        audio.PlayOneShot(activationSound);

        if (Physics.Raycast(transform.position + transform.forward * 0.2f, transform.forward, out RaycastHit hit, boxdimensions.z, playerLayer)) {
            RenderObject(transform.forward, Vector3.Distance(hit.point, transform.position));
            rayLength = Vector3.Distance(hit.point, transform.position);
        }
        else {
            RenderObject(transform.forward, boxdimensions.z);
            rayLength = boxdimensions.z;
        }

        
        
    }

    bool touched;
    bool touched2;
    bool activated = true;

    void Update()
    {
        if (canExplode)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayLength, bodyLayer)) {
                detonated = true;
                canExplode = false;
            }
        }
        

        if (detonated && activated)
        {
            HandleExplosion();
            detonated = false;
            activated = false;
        }
    }

    Collider[] explosionColliders;

    public void HandleExplosion()
    {
        //Handle Explosion after timeout

        if (!IsOwner) return;

        explosionColliders = Physics.OverlapBox(transform.position + transform.forward*(rayLength/2), new Vector3(boxdimensions.x/2,boxdimensions.y/2, rayLength/2), Quaternion.LookRotation(transform.forward), bodyLayer);
        if (explosionColliders.Length != 0)
        {
            
            ph2 = new PlayerHealth[explosionColliders.Length];
            for (int i = 0; i < explosionColliders.Length; i++)
            {
                if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null)
                    ph2[i] = explosionColliders[i].GetComponentInParent<PlayerHealth>();
            }

            for (int i = 0; i < ph2.Length; i++) {
                if (ph2[i] != null && IsOwner) {

                    if (ph2[i].transform.gameObject == _rootObject && !touched) {
                        ph2[i].ChangeKilledState(true);
                        ph2[i].RemoveHealth(10);
                        ph2[i].suicide = true;
                        IncreaseSuicidesAmount();
                        ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        ph2[i].SetKiller(_rootObject.transform);
                        touched = true;
                        
                    }
                    else if (ph2[i].transform.gameObject != _rootObject && !touched2) {
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


        ExplodeServer();
        
    }

    [ServerRpc (RunLocally = true)]
    void ExplodeServer()
    {
        ExplodeObservers();
    }

    [ObserversRpc]
    void ExplodeObservers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null) {
                var distance = Vector3.Distance(transform.position, players[i].transform.position);
                players[i].GetComponent<PlayerHealth>().LocalScreenshake(duration, Mathf.Lerp(maxStrength, minStrength, Mathf.Clamp(distance/maxDistance, 0, 1)), vibrato, randomness, shakeEase);
            }
        }

        Destroy(gameObject);
        Instantiate(explosionVfx, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySound(explosionClip);
    }

    bool sendKillLog;
    void SendKillLog(PlayerHealth enemyHealth)
    {
        if (sendKillLog) return;
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
        if (!increaseKillAmount){
            Settings.Instance.IncreaseKillsAmount();
            increaseKillAmount = true;
        } 
        _rootObject.GetComponent<FirstPersonController>().lensDistortion.intensity.value = _rootObject.GetComponent<FirstPersonController>().killShockWaveStrength;
        _rootObject.GetComponent<FirstPersonController>().colorGrading.saturation.value = -100;
    }

    GameObject lineObject;

    void RenderObject(Vector3 direction, float maxDistance)
    {
        lineObject = new GameObject("RaycastLine");

        // Add and configure the LineRenderer component
        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        //var lineFx = lineObject.AddComponent<LineFade>();
        //lineFx.color = Color.red;
        //lineFx.speed = 0;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.025f);
        lineRenderer.endColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.025f);

        var rayOrigin = transform.position;
        var rayDirection = direction;
        
        lineRenderer.SetPosition(0, rayOrigin);
        lineRenderer.SetPosition(1, rayOrigin + rayDirection * maxDistance);
        //lineRenderer.startColor = new Color(255, 0, 0, 100);
        //lineRenderer.endColor = new Color(255, 0, 0, 100);
    }

    bool canActivate = true;

    [ServerRpc (RequireOwnership = false)]
    public void ChangeState()
    {
        detonated = true;
    }

    
}
