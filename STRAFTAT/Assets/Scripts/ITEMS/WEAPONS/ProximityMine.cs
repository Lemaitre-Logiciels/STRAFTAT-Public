using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using DG.Tweening;

public class ProximityMine : NetworkBehaviour
{
    [SerializeField] private string weaponName;
    [SerializeField] private float explosionRadius;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    [SerializeField] private bool instantExplode;
    [SerializeField] private float damage = 5;
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

    [Space]

    [SerializeField] private Material realMat;
    [SerializeField] private GameObject indicator;

    [SyncVar] public  Weapon weapon;

    [Space]
    [SerializeField] private bool stunMine;
    [SerializeField] private float stunTime;
    public bool isOwner;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        
        StartCoroutine(ActivateTrap());
        
    }

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

    IEnumerator ActivateTrap()
    {
        
        yield return new WaitForSeconds(instantExplode ? 0 : 1);
        canExplode = true;
        audio.PlayOneShot(activationSound);
        if (!instantExplode) graph.transform.DOScale(new Vector3(1.858382f, 1.858382f, 1.858382f), 0.2f);
        yield return new WaitForSeconds(0.2f);
        Material[] mats = {realMat} ;
        if (!instantExplode) graph.GetComponent<MeshRenderer>().materials = mats;
        if (!instantExplode) indicator.SetActive(true); 

    }

    bool touched;
    bool touched2;
    bool activated = true;

    void Update()
    {
        

        if (detonated && activated)
        {
            HandleExplosion();
            detonated = false;
            activated = false;
        }
    }

    public void HandleExplosion()
    {
        if (!IsOwner) return;
        //Handle Explosion after timeout
        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
        if (explosionColliders.Length != 0)
        {
            List<PlayerHealth> ph2List = new List<PlayerHealth>();
            for (int i = 0; i < explosionColliders.Length; i++)
            {
                if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null) {
                    if (!ph2List.Contains(explosionColliders[i].GetComponentInParent<PlayerHealth>())) ph2List.Add(explosionColliders[i].GetComponentInParent<PlayerHealth>());
                }
            }
            ph2 = ph2List.ToArray();

            for (int i = 0; i < ph2.Length; i++) {
                if (ph2[i] != null && IsOwner && !ph2[i].isKilled) {

                    if (ph2[i].transform.gameObject == _rootObject) {
                        if (ph2[i].health - damage <= 0) {
                            ph2[i].ChangeKilledState(true);
                            ph2[i].suicide = true;
                            IncreaseSuicidesAmount();
                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        }

                        HitMarker(false);
                        
                        ph2[i].RemoveHealth(damage);
                        ph2[i].SetKiller(_rootObject.transform);
                        touched = true;
                        if (stunMine) ph2[i].TaserEnemy(ph2[i], stunTime);
                        
                    }
                    else if (ph2[i].transform.gameObject != _rootObject) {
                        if (ph2[i].health - damage <= 0) {
                            ph2[i].ChangeKilledState(true);
                            KillShockWave();
                            SendKillLog(ph2[i]);
                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        }

                        HitMarker(false);
                        
                        ph2[i].RemoveHealth(damage);
                        ph2[i].SetKiller(_rootObject.transform);
                        touched2 = true;
                        if (stunMine) ph2[i].TaserEnemy(ph2[i], stunTime);
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

    bool canActivate = true;

    private void OnTriggerEnter(Collider col)
    {
        if (instantExplode && canActivate)
        {
            Debug.Log("chiasse");
            canActivate = false;
            ChangeState();
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (!canExplode || instantExplode) return;

        if (col.CompareTag("Player"))
        {
            bipTimer -= Time.deltaTime;

            if (bipTimer < 0)
            {
                bipTimer = 0.006f;
                SoundManager.Instance.PlaySound(bipClip);
            }
        }


    }

    private void OnTriggerExit(Collider col)
    {
        if (!canExplode || instantExplode) return;

        if (col.CompareTag("Player") && canActivate)
        {
            canActivate = false;
            ChangeState();
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void ChangeState()
    {
        detonated = true;
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


    
}
