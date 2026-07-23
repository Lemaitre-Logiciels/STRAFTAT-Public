using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using FishNet.Connection;
using TMPro;

public class DisableTimeAttackParkour : InteractEnvironment
{
    [SerializeField] private InputAction menu;
    [SerializeField] private InputAction menu2;

    private Vector3 restPos;
    [SerializeField] private float focusOffset = 0.15f;
    [SerializeField] private float pressOffset = 0.33f;
    [SerializeField] private float moveSpeed = 12;
    [Space]
    [SerializeField] private TMP_Text text;

    TimerManager timerManager;

    private bool act;
    private bool focused;

    void Start()
    {
        restPos = transform.localPosition;

        timerManager = FindObjectOfType<TimerManager>();

        text.text = (timerManager.enabled ? "Disable Parkour Races" : "Enable Parkour Races");
    }

    public override void OnFocus()
    {
        PauseManager.Instance.interactPopup.gameObject.SetActive(true);

        PauseManager.Instance.interactPopup.text = popupText.ToLower();

        focused = true;
    }


    public override void OnInteract(Transform player)
    {
        timerManager.Enable(!timerManager.enabled);
        text.text = (timerManager.enabled ? "Disable time attack course" : "Enable time attack course");
    }


    public override void OnLoseFocus()
    {
        focused = false;
    }

    bool rematch = true;

    private void Update()
    {
        

        var desiredPosition = focused ? restPos + new Vector3(focusOffset, 0, 0) : act ? restPos + new Vector3(pressOffset, 0, 0) : restPos;

        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, moveSpeed * Time.deltaTime);
    }
    
}
