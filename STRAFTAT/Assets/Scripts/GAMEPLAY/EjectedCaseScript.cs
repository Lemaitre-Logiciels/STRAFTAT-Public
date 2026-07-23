using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EjectedCaseScript : MonoBehaviour
{
    [SerializeField] private AudioClip[] normalCase;
    [SerializeField] private AudioClip shotgunCase;
    public int ejectCaseIndex;
    private AudioSource source;

    public bool shouldPlaySound;
    private bool triggered;

    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    void Awake()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        source = GetComponent<AudioSource>();
        source.pitch = Random.Range(0.9f, 1.1f);
    }

    

    void OnParticleCollision(GameObject other)
    {
        
        float pipu = Random.Range(-1, 2);
        if (!triggered && shouldPlaySound) source.PlayOneShot(ejectCaseIndex == 1 ? shotgunCase: normalCase[(pipu > 0 ? 0 : 1)]);
        triggered = true;

    }
}
