using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Goodgulf.Graphics;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchChat : NetworkBehaviour
{

    private GameObject ChatBox;
    [BoxGroup("Input")] 
    public TMP_InputField inputLine;

    void Awake()
    {
        inputLine = GameObject.Find("ChatInputField2").GetComponent<TMP_InputField>();
        ChatBox = GameObject.Find("ChatInputField2");

        ChatBox.transform.localScale = Vector3.one;
        ChatBox.SetActive(false);
    }
    
    [Client]
    public void Update()
    {
        if (ChatBox.activeSelf) { inputLine.ActivateInputField(); }
        if (Input.GetKeyDown(KeyCode.Return)) { ChatBox.SetActive(!ChatBox.activeSelf); }
        if (Input.GetKeyDown(KeyCode.Escape)) { ChatBox.SetActive(false); }
    }
    
    void OnDisable()
    {
        ChatBox.transform.localScale = Vector3.zero;
        ChatBox.SetActive(true);
        inputLine.text = "";
        inputLine.DeactivateInputField(true);
    }
    
}



