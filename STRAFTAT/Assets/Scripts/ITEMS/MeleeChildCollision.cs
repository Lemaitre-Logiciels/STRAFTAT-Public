using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeChildCollision : MonoBehaviour
{
    [SerializeField] private MeleeWeapon weaponScript;

    
    [SerializeField] private ItemBehaviour behaviour;
    [SerializeField] private GameObject graphicalObject;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject trailObject;
    [SerializeField] private AudioClip hitSFX;
    public bool canHit = false;
    public bool canHitEnvi = false;
    int hitsCounter;
    
    void Update()
    {
        if (gameObject.layer != 21) gameObject.layer = 21;
        //if (graphicalObject.layer != 21 && behaviour.rootObject != null) graphicalObject.layer = 21;
        //else if (behaviour.rootObject != null) graphicalObject.layer = 8;
        //else graphicalObject.layer = 0;

        if (trailObject != null) trailObject.SetActive(canHit);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (weaponScript.gameObject.layer == 7) return;

        if (behaviour.rootObject != null)
            if (collision.transform.root == behaviour.rootObject.transform) return;
            
        if (collision.transform.tag == "ShatterableGlass" && canHitEnvi)
        {
            weaponScript.BounceHolder();
            weaponScript.BreakGlassServer(collision.contacts[0].point, -collision.contacts[0].normal, collision.transform.gameObject);

            Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            GetComponentInParent<AudioSource>().PlayOneShot(hitSFX);
        }

        hitsCounter ++;

        if (canHit)
        {
            if (collision.transform.GetComponentInParent<PlayerHealth>() != null)
            {
                weaponScript.HitServer(collision.transform.GetComponentInParent<PlayerHealth>(), collision.contacts[0].point, collision.contacts[0].normal, collision.transform.gameObject.name);
                if (hitsCounter >= weaponScript.hitsAmount) canHit = false;
            }

            if (collision.transform.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
            {
                var bodies = collision.transform.root.GetComponentsInChildren<Rigidbody>();
                Instantiate(weaponScript.bodyImpact, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                Instantiate(weaponScript.bloodSplatter, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                foreach(var body in bodies)
                {
                    body.AddExplosionForce(weaponScript.ragdollEjectForce, collision.contacts[0].point - weaponScript.rootObject.transform.forward, 100, 1f, ForceMode.Impulse);
                }
            }

            
            
        }

        if ((collision.transform.gameObject.layer == LayerMask.NameToLayer("Default") ||collision.transform.gameObject.layer == LayerMask.NameToLayer("ShootThrough") || collision.transform.gameObject.layer == LayerMask.NameToLayer("InteractEnvironment")) && canHitEnvi)
        {
            weaponScript.BounceHolder();
            canHitEnvi = false;
            Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            GetComponentInParent<AudioSource>().PlayOneShot(hitSFX);
        }


        
    }

}
