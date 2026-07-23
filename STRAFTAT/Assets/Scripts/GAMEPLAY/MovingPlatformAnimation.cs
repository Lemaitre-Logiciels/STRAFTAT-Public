using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class MovingPlatformAnimation : MonoBehaviour
{
    private Animation animation;

    void Start()
    {
        animation = GetComponent<Animation>();
    }

    void Update()
    {
        if (InstanceFinder.NetworkManager.IsServer) animation.enabled = true;
    }


}
