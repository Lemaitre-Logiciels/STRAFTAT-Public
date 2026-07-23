using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using LambdaTheDev.NetworkAudioSync;

public class ProximityChatPlayer : NetworkBehaviour
{
    [SerializeField] private NetworkAudioSource outputSource;
    [SerializeField] private AudioSource localOutputSource;
    public AudioClip clip;
    [ServerRpc]
    private void PlaybackServer(bool play)
    {
        
        if (play)
            outputSource.Play();
        else
            outputSource.Stop();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner) GetComponent<AudioSource>().volume = 0;
    }

    /*void Update()
    {
        if (IsOwner) PlayServer();
        
        if (!IsOwner) localOutputSource.clip = clip;
    }

    [ServerRpc]
    private void PlayServer()
    {
        PlayServerObs();
        
    }
    [ObserversRpc]
    private void PlayServerObs()
    {
        clip = localOutputSource.clip;
    }*/


}
