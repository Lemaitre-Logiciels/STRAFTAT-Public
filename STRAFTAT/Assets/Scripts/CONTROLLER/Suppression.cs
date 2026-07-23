using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class Suppression : NetworkBehaviour
{
    private AudioSource audio;
    [SerializeField] private AudioClip[] supClip;
    [SerializeField] private CameraEffect camPP;
    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    public void SuppressionTrigger()
    {
        if (IsOwner) {
            audio.PlayOneShot(supClip[(int)Mathf.RoundToInt(Random.Range(0, supClip.Length))]);
            camPP.TrigSup();
        }
    }
}
