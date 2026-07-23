using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class RebondBalle : MonoBehaviour
{
    private Vector3 _direction;
    private float _passedTime = 0f;
    [SerializeField] private float MOVE_RATE = 5f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float rebonds = 2;
    [SerializeField] private GameObject vfx;
    [SerializeField] private LayerMask playerLayer;
    
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private GameObject hitVfx;
    private GameObject _rootObject;
    private AudioSource audio;

    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityStart;
    [SerializeField] private float gravity;
    float _gravity;

    [SerializeField] private bool usePhysics;
    [SerializeField] private float friction;
    [SerializeField] private float ragdollEjectForce = 2;
    float _force;
    bool backupRaycast;
    bool forwardCast;
    float safeTimer;

    GameObject _gun;

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

        transform.rotation = Quaternion.LookRotation(-direction);
    }

    Vector3 currentPosition;
    Vector3 lastPosition;
    Vector3 velocity;

    void Update()
    {
        currentPosition = transform.position;
        safeTimer -= Time.deltaTime;

        Move();

        HandleCollision();

        lastPosition  = currentPosition;
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
        if (usePhysics) _force -= friction * Time.deltaTime;

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

            if (!gunCollision && !rootCollision)
            {
                PlayerHealth ph = hitColliders[0].GetComponentInParent<PlayerHealth>();

                if (ph != null) {
                    if (ph.transform.gameObject == _rootObject) return;
                    if (ph.health - damage <= 0) {
                        ph.Explode(false, true, ph.gameObject.name, ph.transform.position - transform.position, ragdollEjectForce, transform.position);
                        ph.isKilled = true;
                    }
                    ph.health -= damage;
                    ph.killer = _rootObject.transform;
                    if (vfx != null) Instantiate(vfx, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
            }

            if (!gunCollision && !rootCollision) {

                firstNormal = rayHit.normal;
                secondNormal = firstNormal;
                _direction += rayHit.normal;
                _direction = _direction.normalized;
                
                rebonds --;
                if (rebonds <= 0) Destroy(gameObject);
            }
                
        }
        /*else if (backupRaycast && safeTimer < 0 && rayHit.normal != secondNormal)
        {
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

            if (!gunCollision && !rootCollision)
            {
                PlayerHealth ph = rayHit.GetComponentInParent<PlayerHealth>();

                if (ph != null) {
                    if (ph.gameObject != _rootObject) {
                        ph.health -= damage;
                        ph.killer = _rootObject.transform;
                        ph.Explode(false, true, ph.gameObject.name, ph.transform.position - transform.position, ragdollEjectForce);
                        Destroy(gameObject);
                    }
                }
            }

            if (!gunCollision && !rootCollision) {
                firstNormal = rayHit.normal;
                secondNormal = firstNormal;
                _direction -= rebondHit.normal;
                _direction = _direction.normalized;
                rebonds --;
                if (rebonds <= 0) Destroy(gameObject);
            }
        }*/
        
            
    }

    
}
