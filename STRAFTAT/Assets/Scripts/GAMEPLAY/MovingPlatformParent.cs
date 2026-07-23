using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Component.Transforming;

public class MovingPlatformParent : NetworkBehaviour
{
    public bool doesEject = true;
    public Vector3 movingVector;
    Vector3 previousPosition;

    void Start()
    {
        if (GetComponent<NetworkTransform>() != null ) GetComponent<NetworkTransform>().enabled = false;
    }
    void Update()
    {
        movingVector = transform.position - previousPosition;

        

        previousPosition = transform.position;
    }

}
