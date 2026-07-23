using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using DG.Tweening;

public class HandGrenadeTwo : MonoBehaviour
{
    public bool isOwner;
    [SerializeField] float mass = 3.0f;
    private Vector3 impact = Vector3.zero;
    private CharacterController character;

    [SerializeField] private float airdeceleration = 0.9f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private int maxRebond = 10;
    [SerializeField] private float groundDetectionSlopeLimit = 60f;

    [SerializeField] private float ragdollEjectForce;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    [SerializeField] private LayerMask rebondLayer;
    
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject explosionVfx;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip explosionClip;

    [SerializeField] private float gravity;
    float _gravity;

    [SerializeField] private float friction = 10f;
    float _force;

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

    [HideInInspector] public Weapon weapon;

    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;

    float safeTimer;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        character = GetComponent<CharacterController>();
        _gravity = 0;
    }

    public void Initialize(Vector3 direction, float force, float passedTime, GameObject rootObject, GameObject gun)
    {
        _passedTime = passedTime;
        _rootObject = rootObject;
        _force = force;
        _gun = gun;

        explosionTimer = timeBeforeExplosion;

        AddForce(direction, force);
    }

    public void AddForce(Vector3 dir, float force){
        dir.Normalize();
        impact += dir.normalized * force / mass;
    }

    void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (collision.transform.gameObject.layer == 6 || collision.transform.gameObject.layer == 16) return;
        if (maxRebond <=  0) return;
        if (safeTimer > 0) return;

        maxRebond --;
        safeTimer = 0.1f;

        rebondForce -= friction;
        //impact = Vector3.zero;
        
        if (Vector3.Angle(collision.normal, Vector3.up) < groundDetectionSlopeLimit && impact.y < 0) {
            var tempimpacty = -impact.y;
            AddForce(new Vector3(collision.normal.x, 0, collision.normal.z), rebondForce);
            impact.y = tempimpacty;
        }
        /*else if (Vector3.Angle(collision.normal, Vector3.up) > 87 && Vector3.Angle(collision.normal, Vector3.up) < 93){
            Debug.Log("caca");
            impact = new Vector3(impact.x, impact.y, -impact.z);
        }*/
        else {
            AddForce(collision.normal, rebondForce);
        }


        
    }

    void Update()
    {
        velocity = lastPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(-velocity);

        float delta = Time.deltaTime;

        HandleExplosion();

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

        explosionTimer -= (delta + passedTimeDelta);
        safeTimer -= (delta + passedTimeDelta);

        //if (impact.magnitude > 0.2f) 

        if (Physics.Raycast(transform.position, -Vector3.up, 0.5f, rebondLayer)) {
            _gravity = 0;
            isGrounded = true;
        }

        _gravity += gravity * (delta + passedTimeDelta);

        impact = Vector3.Lerp(impact, Vector3.zero, (!isGrounded ? airdeceleration : deceleration) * (delta + passedTimeDelta));

        character.Move((impact - (Vector3.up * _gravity)) * (delta + passedTimeDelta));

        lastPosition  = currentPosition;

    }

    private void HandleExplosion()
    {
        //Handle Explosion after timeout
        if (explosionTimer < 0 && explosionTimer > -2)
        {
            explosionTimer = -3;
            Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
            if (explosionColliders.Length != 0 && isOwner)
            {
                ph2 = new PlayerHealth[explosionColliders.Length];
                for (int i = 0; i < explosionColliders.Length; i++)
                {
                    if (explosionColliders[i].GetComponentInParent<PlayerHealth>() != null)
                        ph2[i] = explosionColliders[i].GetComponentInParent<PlayerHealth>();
                }

                for (int i = 0; i < ph2.Length; i++) {
                    if (ph2[i] != null) {

                        if (ph2[i].transform.gameObject == _rootObject && !touched) {
                            ph2[i].ChangeKilledState(true);
                            ph2[i].RemoveHealth(10);
                            ph2[i].Explode(false, true, ph2[i].gameObject.name, ph2[i].transform.position - transform.position, ragdollEjectForce, transform.position);
                            ph2[i].SetKiller(_rootObject.transform);
                            touched = true;
                        }
                        else if (ph2[i].transform.gameObject != _rootObject && !touched2) {
                            ph2[i].ChangeKilledState(true);
                            ph2[i].RemoveHealth(10);
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
                
            

            Destroy(gameObject);
            Instantiate(explosionVfx, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySound(explosionClip);
        }
    }

    
}
