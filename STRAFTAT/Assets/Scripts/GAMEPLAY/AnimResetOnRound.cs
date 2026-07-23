using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Transforming;

public class AnimResetOnRound : MonoBehaviour
{
    [SerializeField] private bool ResetOnRound = true;

    void OnEnable()
    {
        PauseManager.OnRoundStarted += StartNewRound;
        PauseManager.OnBeforeSpawn += BeforeSpawn;
    }

    void OnDisable()
    {
        PauseManager.OnRoundStarted -= StartNewRound;
        PauseManager.OnBeforeSpawn -= BeforeSpawn;
    }

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<NetworkTransform>() != null ) GetComponent<NetworkTransform>().enabled = false;
        if (SceneMotor.Instance.testMap || PauseManager.Instance.nonSteamworksTransport) animator.enabled = true;
    }

    public void StartNewRound()
    {
        animator.enabled = true;

        if (!ResetOnRound) return;
        animator.Rebind();
        animator.Update(0f);

    }
    
    public void BeforeSpawn() 
    {
        if (!ResetOnRound) return;
        animator.Rebind();
        animator.Update(0f);
        animator.enabled = false;
    }

}
