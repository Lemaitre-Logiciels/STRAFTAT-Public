using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] hitClips;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(hitClips[(int)Mathf.RoundToInt(Random.Range(0, hitClips.Length))]);
    }
}
