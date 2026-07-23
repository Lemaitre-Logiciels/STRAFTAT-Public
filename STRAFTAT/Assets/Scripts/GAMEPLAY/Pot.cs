using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class Pot : NetworkObject
{
    [SerializeField] private GameObject vfx;

    public void Die()
    {
        Destroy(gameObject);
        Instantiate(vfx, transform.position, Quaternion.Euler(-90, 0, 0));
    }

}
