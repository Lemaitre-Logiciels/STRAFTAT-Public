using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShrapnelBallistic : MonoBehaviour
{
    [SerializeField] private string weaponName;
    [SerializeField] private float damage = 1;
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
    [SerializeField] private GameObject explosionDecal;
    [SerializeField] private GameObject bloodSplatter;

    [SerializeField] private float timeBeforeExplosion = 2;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float lineDecreaseSpeed = 10f;
    [SerializeField] private float lineFadeSpeed = 10f;
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

    [HideInInspector] public Weapon weapon;

    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;

    [SerializeField] private bool fragGrenade;
    [SerializeField] private float numberOfRays;

    float safeTimer;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
    
    public void Initialize(GameObject rootObject, GameObject gun, float passedTime)
    {
        _rootObject = rootObject;
        _passedTime = passedTime;
        _gun = gun;
        
        StartCoroutine(Action());
    }

    IEnumerator Action() {
        yield return new WaitForSeconds(0.1f);
        Explode();
    }

    bool makeBlood = true;
    bool hit;
    bool headshot;

    private void Explode()
    {
        int roundedX = Mathf.RoundToInt(transform.position.x / 5f) * 5;
        int roundedZ = Mathf.RoundToInt(transform.position.z / 5f) * 5;
        int randomSeed = roundedX * 10000 + roundedZ;
        
        audio.PlayOneShot(explosionClip);
        //Handle Explosion after timeout
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
                if (Physics.Raycast(transform.position, tempDir, out fragHit, explosionRadius, bodyLayer)) {
                    tempRayHits.Add(fragHit.transform.GetComponent<Collider>());
                    headshot = fragHit.transform.gameObject.name == "Head_Col" || fragHit.transform.gameObject.name == "Neck_1_Col";
                }

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
                    if (bloodSplatter && makeBlood) {
                        Instantiate(bloodSplatter, explosionColliders[i].transform.position, Quaternion.Euler(0 , Random.Range(0, 360) , 0));
                        makeBlood = false;
                    }
                }
            }

            for (int i = 0; i < ph2.Length; i++) {
                if (ph2[i] != null && isOwner) {

                    if (ph2[i].transform.gameObject == _rootObject && !touched) {

                        if (ph2[i].health - (headshot ? damage*2 : damage) <= 0) {
                            ph2[i].ChangeKilledState(true);
                            ph2[i].suicide = true;
                            IncreaseSuicidesAmount();
                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        }

                        HitMarker(headshot);
                        
                        ph2[i].RemoveHealth((headshot ? damage*2 : damage));
                        ph2[i].SetKiller(_rootObject.transform);
                        touched = true;
                    }
                    else if (ph2[i].transform.gameObject != _rootObject && !touched2) {

                        if (ph2[i].health - (headshot ? damage*2 : damage) <= 0) {
                            ph2[i].ChangeKilledState(true);
                            KillShockWave();
                            SendKillLog(ph2[i]);
                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                        }

                        HitMarker(headshot);
                        
                        ph2[i].RemoveHealth((headshot ? damage*2 : damage));
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
        if (!fragGrenade) Instantiate(explosionVfx, transform.position, Quaternion.identity);
        Instantiate(explosionDecal, transform.position, Quaternion.identity);
        audio.Play();
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


    void RenderObject(Vector3 direction, float maxDistance)
    {
        GameObject lineObject = new GameObject("RaycastLine");

        // Add and configure the LineRenderer component
        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        var lineFx = lineObject.AddComponent<LineFade>();
        lineFx.decreaseInSize = true;
        lineFx.decreaseInSizeSpeed = lineDecreaseSpeed;
        lineFx.color = Color.white;
        lineFx.speed = lineFadeSpeed;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.03f;
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
