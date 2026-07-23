using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] menuTracks;

    private AudioSource audio;
    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
    void Start()
    {
        int x = (int)Mathf.RoundToInt(Random.Range(0, menuTracks.Length));
        audio.clip = menuTracks[x];
        audio.Play();
    }

}
