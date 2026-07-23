using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class BumpBullet : MonoBehaviour
{
    private Vector3 _direction;
    private float _passedTime = 0f;
    [SerializeField] private float MOVE_RATE = 5f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float bumpForce = 1f;
    [SerializeField] private float bumpDecel = 6f;
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
    float _force;

    GameObject _gun;
    PlayerHealth[] ph2;

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
    }

    void Update()
    {
        Move();

        HandleCollision();
    }

    private void Move()
    {
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

    private void HandleCollision()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, playerLayer);
        if (hitColliders.Length != 0)
        {
            bool gunCollision = false;
            
            for (int i = 0; i < hitColliders.Length; i++)
                if (hitColliders[i].gameObject == _gun)
                    gunCollision = true;
                else gunCollision = false;

            bool rootCollision = false;
            
            for (int i = 0; i < hitColliders.Length; i++)
                if (hitColliders[i].gameObject == _rootObject)
                    rootCollision = true;
                else rootCollision = false;

            if (InstanceFinder.IsClient && !gunCollision && !rootCollision)
            {
                SoundManager.Instance.PlaySound(hitClip);
                Instantiate(hitVfx, transform.position, Quaternion.identity);
            }

            if (InstanceFinder.IsServer && !gunCollision && !rootCollision)
            {
                PlayerHealth ph = hitColliders[0].GetComponentInParent<PlayerHealth>();

                if (ph != null)
                    if (ph.gameObject != _rootObject) {
                        ph.AddForce((ph.transform.position - transform.position) + Vector3.up, bumpForce);
                    }

                Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);
                if (explosionColliders.Length != 0)
                {
                    ph2 = new PlayerHealth[explosionColliders.Length];
                    for (int i = 0; i < explosionColliders.Length; i++)
                    {
                        if (explosionColliders[i].GetComponentInParent<PlayerHealth>())
                            ph2[i] = explosionColliders[i].GetComponentInParent<PlayerHealth>();
                    }

                    
                    
                    for (int i = 0; i < ph2.Length; i++)
                        if (ph2[i] != null)
                            ph2[i].AddForce((ph2[i].transform.position - transform.position) + Vector3.up, bumpForce);
                }
            }

            

            if (!gunCollision && !rootCollision)
                Destroy(gameObject);
        }
            
    }

    
}
