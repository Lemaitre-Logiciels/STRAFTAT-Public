using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using HeathenEngineering.PhysKit;
using UnityEngine;
using UnityEngine.Serialization;

public struct BallisticPathFollowSyncData {
    public float time;
    public int previous;
    public bool playing;
    public BallisticPath[] path;
    public bool isAppending;
}

[RequireComponent(typeof(BallisticPathFollow))]
public class BallisticPathFollowSyncer : NetworkBehaviour {
    private BallisticPathFollow pathFollow; 

    private void Awake() {
        pathFollow = GetComponent<BallisticPathFollow>(); 
        gameObject.SetActive(true);
    }
    
    private void Start() {
        gameObject.SetActive(true);
    }

    int previousLength = 0;
    [ObserversRpc(ExcludeOwner = true)]
    public void ReconcileTheThing(Vector3 position, Quaternion rotation, BallisticPathFollowSyncData data) {
        pathFollow.previous = data.previous;
        pathFollow.playing = data.playing;

        if (!data.isAppending) {
            pathFollow.path = data.path.ToList();
            previousLength = data.path.Length;
        }
        else {
            pathFollow.path = pathFollow.path.Take(previousLength).ToList();
            pathFollow.path.AddRange(data.path);
            previousLength += data.path.Length;
        }
        
        float distance = Vector3.Distance(transform.position, position);
        if (distance < 5) {
            pathFollow.time = data.time;
            transform.SetPositionAndRotation(position, rotation);
        }
    }

    [ServerRpc]
    public void ReconcileTheThingServer(Vector3 position, Quaternion rotation, BallisticPathFollowSyncData data) { ReconcileTheThing(position, rotation, data); }

    private uint _fixedUpdateCount = 0;
    private void FixedUpdate() {
        if (!IsOwner) { return; }
        _fixedUpdateCount++;
        if (_fixedUpdateCount % 15 != 0) { return; }

        BallisticPath[] newPoints = pathFollow.path.Skip(previousLength).ToArray();
        if (newPoints.Length == 0) { return; }
        
        BallisticPathFollowSyncData data = new BallisticPathFollowSyncData {
            time = pathFollow.time,
            previous = pathFollow.previous,
            playing = pathFollow.playing,
            path = newPoints,
            isAppending = true
        };
        previousLength = pathFollow.path.Count;
        
        ReconcileTheThingServer(transform.position, transform.rotation, data);
    }

    [ObserversRpc(RunLocally = true)]
    public void Shoot(trickShotData data, int id) {
        pathFollow.projectile = new BallisticsData { velocity = data.forward * data.speed, radius = data.radius };
        pathFollow.path = data.prediction.ToList();

        if (IsOwner) {
            if (!idToSyncer.TryGetValue(id, out BallisticPathFollowSyncer syncer)) { return; }
            pathFollow.time = syncer.pathFollow.time;
            pathFollow.previous = syncer.pathFollow.previous;
            pathFollow.playing = syncer.pathFollow.playing;
            pathFollow.path = syncer.pathFollow.path;
            transform.SetPositionAndRotation(syncer.transform.position, syncer.transform.rotation);

            BallisticPathFollowSyncData data2 = new BallisticPathFollowSyncData {
                previous = pathFollow.previous,
                playing = pathFollow.playing,
                path = pathFollow.path.ToArray(),
                isAppending = false
            };
            previousLength = pathFollow.path.Count;

            ReconcileTheThingServer(transform.position, transform.rotation, data2);
            Destroy(syncer.gameObject);
        }
    }
    
    public static Dictionary<int, BallisticPathFollowSyncer> idToSyncer = new Dictionary<int, BallisticPathFollowSyncer>();
    
    public void ShootNonRPC(trickShotData data, int id) {
        idToSyncer[id] = this;
        gameObject.SetActive(true);
        pathFollow.projectile = new BallisticsData { velocity = data.forward * data.speed, radius = data.radius };
        pathFollow.path = data.prediction.ToList();
    }
}
