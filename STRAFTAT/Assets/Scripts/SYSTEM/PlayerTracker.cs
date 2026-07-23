using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using System.Linq;
using FishNet.Object.Synchronizing;
using FishNet.Object;

public class PlayerTracker : MonoBehaviour
{
    //[SyncObject]
    //public readonly SyncList<NetworkObject> players = new SyncList<NetworkObject>();
    public List<NetworkObject> players = new List<NetworkObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            for (int i = 0; i < players.Count; i++)
            {
            }
        }
    }

}
