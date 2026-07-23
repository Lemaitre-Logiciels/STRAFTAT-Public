using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class AnimationSoundEvent : NetworkBehaviour
{
    private AudioSource audio;
    [SerializeField] private AudioClip[] clips;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<AudioSource>() != null) audio = GetComponent<AudioSource>();
        else audio = gameObject.AddComponent<AudioSource>();
    }

    //[ServerRpc (RequireOwnership = false)]
    public void PlaySound(int index)
    {
        //PlaySoundObservers(index);
        audio.PlayOneShot(clips[index]);
    }

    //[ObserversRpc]
    private void PlaySoundObservers(int index)
    {
        audio.PlayOneShot(clips[index]);
    }

    
}
