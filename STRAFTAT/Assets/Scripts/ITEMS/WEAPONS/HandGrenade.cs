using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class HandGrenade : MonoBehaviour
{
    private Vector3 _direction;
    private float _passedTime = 0f;
    [SerializeField] private float MOVE_RATE = 5f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float rebonds = 2;
    [SerializeField] private float rebondFactor = 0.6f;
    [SerializeField] private float rebondDecel = 0.2f;
    [SerializeField] private float rebondDrag = 1f;
    [SerializeField] private float ragdollEjectForce = 4;
    [SerializeField] private GameObject vfx;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask bodyLayer;
    
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject explosionVfx;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip explosionClip;
    private GameObject _rootObject;
    private AudioSource audio;

    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityStart;
    [SerializeField] private float gravity;
    float _gravity;

    [SerializeField] private bool usePhysics;
    [SerializeField] private float friction;
    public float explosionTimer;
    [SerializeField] private float timeBeforeExplosion = 3;
    float _force;
    bool backupRaycast;
    bool forwardCast;
    float safeTimer;

    GameObject _gun;

    private Vector3 lastFrameVelocity;
    
    [HideInInspector] public Weapon weapon;

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

        explosionTimer = timeBeforeExplosion;

        transform.rotation = Quaternion.LookRotation(-direction);
    }

    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;

    void Update()
    {
        currentPosition = transform.position;
        safeTimer -= Time.deltaTime;
        explosionTimer -= Time.deltaTime;

        Move();

        HandleCollision();

        HandleExplosion();

        lastPosition  = currentPosition;
        lastFrameVelocity = velocity;
    }

    private void Move()
    {
        velocity = lastPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(-velocity);
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

        if (useGravity) _gravity += gravity * Time.deltaTime;
        if (usePhysics) {
            _force -= friction * Time.deltaTime;
            rebondDrag -= rebondDecel * Time.deltaTime;
        }

        rebondDrag = Mathf.Clamp(rebondDrag, 0, 1);
        _force = Mathf.Clamp(_force, 0, 100);

        //Move the projectile using moverate, delta, and passed time delta.
        transform.position += _direction * ((usePhysics ? _force : MOVE_RATE) * (delta + passedTimeDelta));

        if (useGravity)
        {
            transform.position -= Vector3.up * (_gravity * (delta + passedTimeDelta));
        }
    }

    [SerializeField] private float backupRayLength = 1.5f;
    [SerializeField] private float backupRayDistance = 1f;
    [SerializeField] private float forwardCastLength = 1.5f;

    private Vector3 firstNormal;
    private Vector3 secondNormal;
    private Vector3 lastVel;

    private void HandleCollision()
    {
        backupRaycast = Physics.Raycast(transform.position - transform.forward * backupRayDistance, transform.forward, out RaycastHit rayHit, backupRayLength, playerLayer);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, playerLayer);

        if (hitColliders.Length != 0 && safeTimer < 0 && rayHit.normal != firstNormal)
        {
            bool gunCollision = false;
            
            for (int i = 0; i < hitColliders.Length; i++)
                if (hitColliders[i].gameObject == _gun)
                    gunCollision = true;
                else gunCollision = false;

            bool rootCollision = false;
            
            for (int i = 0; i < hitColliders.Length; i++) {
                if (hitColliders[i].gameObject == _rootObject)
                    rootCollision = true;
                else rootCollision = false;
            }

            if (InstanceFinder.IsClient && !gunCollision && !rootCollision)
            {
                audio.PlayOneShot(hitClip);
                Instantiate(hitVfx, transform.position, Quaternion.identity);
            }

            if (!gunCollision && !rootCollision) {

                firstNormal = rayHit.normal;
                secondNormal = firstNormal;
                _direction += rayHit.normal;
                _direction = _direction.normalized * rebondFactor * rebondDrag;
                
                rebonds --;
                _gravity = 1;
                lastVel = velocity;
            }
                
        }
        else if (backupRaycast && safeTimer < 0 && velocity != lastVel)
        {
            safeTimer = 0.1f;
            bool gunCollision = false;
            
                if (rayHit.transform.gameObject == _gun)
                    gunCollision = true;
                else gunCollision = false;

            bool rootCollision = false;
            
                if (rayHit.transform.gameObject == _rootObject)
                    rootCollision = true;
                else rootCollision = false;

            if (InstanceFinder.IsClient && !gunCollision && !rootCollision)
            {
                SoundManager.Instance.PlaySound(hitClip);
                Instantiate(hitVfx, transform.position, Quaternion.identity);
            }

            if (!gunCollision && !rootCollision) {
                firstNormal = rayHit.normal;
                secondNormal = firstNormal;
                _direction += rayHit.normal;
                _direction = _direction.normalized * rebondFactor * rebondDrag;
                rebonds --;
                _gravity = 0;
            }
        }
        
            
    }

    private void HandleExplosion() {
        //Handle Explosion after timeout
        if (!(explosionTimer < 0) || !(explosionTimer > -2))  { return; }
        
        explosionTimer = -3;
        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, bodyLayer);
        foreach (Collider collider in explosionColliders) {
            PlayerHealth playerHealth = collider.GetComponentInParent<PlayerHealth>();
            if (!playerHealth || playerHealth.isKilled) { continue; }
            
            if (playerHealth.transform.gameObject == _rootObject) { Settings.Instance.IncreaseSuicidesAmount(); }
            else { Settings.Instance.IncreaseKillsAmount(); }
            
            playerHealth.isKilled = true;
            playerHealth.health -= 10;
            playerHealth.Explode(false, true, playerHealth.gameObject.name, playerHealth.transform.position - transform.position, ragdollEjectForce, transform.position);
            playerHealth.killer = _rootObject.transform;
        }

        Destroy(gameObject);
        Instantiate(explosionVfx, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySound(explosionClip);
    }

    
}
