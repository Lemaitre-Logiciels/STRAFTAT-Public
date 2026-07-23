using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;

public class PlayerTrackerEnable : MonoBehaviour
{
    [SerializeField] private Behaviour playerTracker;

    // Update is called once per frame
    void Update()
    {
        if (InstanceFinder.ServerManager.Started) 
        {
            playerTracker.enabled = true;
            this.enabled = false;
            GetComponent<NetworkObject>().enabled = true;
        }
        else
        {
            playerTracker.enabled = false;
        }
    }
}
