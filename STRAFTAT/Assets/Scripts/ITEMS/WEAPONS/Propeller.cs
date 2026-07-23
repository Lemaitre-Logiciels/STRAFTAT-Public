using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using LambdaTheDev.NetworkAudioSync;


public class Propeller : Weapon
{
    [Header("Weapon Specials")]
    [SerializeField] private float flySpeed;
    [SerializeField] private float decelSpeed = 8;
    [SerializeField] private float maxPower;
    [SerializeField] private AudioClip propOutClip;
    [SerializeField] private Transform rotativePart;
    [SerializeField] private float rotationSpeed = 1000;
    [SerializeField] private float lerpSpeed = 100;
    private float power = 4;
    float rotateSpeed;
    NetworkAudioSource networkAudioSource;

    void Start()
    {
        networkAudioSource = GetComponent<NetworkAudioSource>();
    }

    bool isflying;
    bool active3;
    bool active4;
    bool pressed;
    bool released = true;

    private void Update()
    {
        WeaponUpdate();

        //If not in hands
        if (gameObject.layer == 7) return;

        if (!playerController.IsOwner) return;

        if (playerController.isGrounded) {
            power = maxPower;
        }

        if (power <= 0 && active3) {
            AudioStop();
            active3 = false;
        }

        if (jump.ReadValue<float>() > 0.1f) 
            Fire();
        else {
            isflying = false;
        }

        if (jump.ReadValue<float>() > 0.1f) {
            released = false;
        }
        else {
            pressed = false;
        }

        if (jump.ReadValue<float>() > 0.1f && !pressed) {
            pressed = true;
            active3 = true;
            AudioPlay();
        }
        if (jump.ReadValue<float>() < 0.1f && !released) {
            released = true;
            AudioStop();
        }


        rotateSpeed = Mathf.Lerp(rotateSpeed, (isflying ? rotationSpeed : 0), lerpSpeed * Time.deltaTime);

        rotativePart.Rotate(rotateSpeed, 0, 0);

    }

    [ServerRpc]
    private void AudioPlay()
    {
        networkAudioSource.Play();
    }

    [ServerRpc]
    private void AudioStop()
    {
        networkAudioSource.Stop();
        PlayReleaseClipObservers();
    }

    private void Fire()
    {

        if (power < 0) isflying = false;
        else isflying = true;

        

        if (power < 0) return;

        if (power < 0.1f) {
            power = -1;
            audio.PlayOneShot(propOutClip);
        }

        Fly();

            
    }

    private void Fly()
    {
        power -= Time.deltaTime;

        if (playerController.moveDirection.y < 7) playerController.moveDirection.y += flySpeed * Time.deltaTime;
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void PlayReleaseClipObservers()
    {
        audio.PlayOneShot(propOutClip);
    }

    

}
