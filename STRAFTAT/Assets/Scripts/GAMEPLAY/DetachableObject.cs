using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class DetachableObject : NetworkObject
{
    [SerializeField] private GameObject vfx;
    [SerializeField] private float ejectForce = 4;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void Detach(Vector3 hitNormal, Vector3 direction)
    {
        rb.isKinematic = false;

        gameObject.layer = 18;

        transform.SetParent(null);
        Instantiate(vfx, transform.position, Quaternion.LookRotation(hitNormal));

        rb.AddForce(direction * ejectForce, ForceMode.Impulse);
    }

}
