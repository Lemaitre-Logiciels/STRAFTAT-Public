using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class PropDamage : NetworkBehaviour
{
    [SerializeField] private GameObject[] states;
    [SerializeField] private AudioClip[] hitClips;
    private int index;
    
    public void Damage()
    {
        
        index ++;
        if (index >= states.Length) return;

        GetComponent<AudioSource>().PlayOneShot(hitClips[(int)Mathf.RoundToInt(Random.Range(0, hitClips.Length))]);

        states[index-1].SetActive(false);
        states[index].SetActive(true);

        
    }
}
