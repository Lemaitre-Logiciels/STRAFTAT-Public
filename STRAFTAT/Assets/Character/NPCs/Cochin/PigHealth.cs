using FishNet.Component.ColliderRollback.Demo;
using FishNet.Demo.AdditiveScenes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigHealth : Pigeon
{
    void Start()
    {
        return;
    }

    public void Die()
    {
        //Instantiate(dieVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        return;
    }
}
