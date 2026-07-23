using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class OnlyForHost : MonoBehaviour
{

    void Update()
    {
        if (InstanceFinder.NetworkManager == null) return;

        if (!InstanceFinder.NetworkManager.IsServer) transform.localScale = Vector3.zero;
        else transform.localScale = Vector3.one;
    }
}
