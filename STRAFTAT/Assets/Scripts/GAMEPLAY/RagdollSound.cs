using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollSound : MonoBehaviour
{
    float timer;

    bool trigger;
    [SerializeField] private AudioClip groundHitClip;
    [SerializeField] private AudioSource audio;

    void Update()
    {
        timer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.tag == "Killz") Settings.Instance.ragdollsThrownAway ++;
        if (trigger) return;

        if (col.gameObject.layer != 0) return;
        if (timer < 0)
        {
            timer = 1;
            audio.pitch = Random.Range(0.90f, 1.10f);
            audio.PlayOneShot(groundHitClip);
            trigger = true;
        }
    }
    
}
