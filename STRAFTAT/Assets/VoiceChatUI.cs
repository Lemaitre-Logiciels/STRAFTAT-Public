using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class VoiceChatUI : MonoBehaviour {
    public static VoiceChatUI Instance;
    
    public Toggle disableVoiceChatUIToggle;

    private void Awake() {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        DisableVoiceChatUI = PlayerPrefs.GetInt("DisableVoiceChatUI", 0) == 1;
        disableVoiceChatUIToggle.isOn = DisableVoiceChatUI;
        disableVoiceChatUIToggle.onValueChanged.AddListener(SetDisableVoiceChatUI);
    }
    
    public VoiceChatUIObject voiceChatUIObject;
    private readonly List<VoiceChatUIObject> _voiceChatUIObjects = new List<VoiceChatUIObject>();
    
    public void CreateVoiceChatUIObject(ClientInstance clientInstance) {
        if (DisableVoiceChatUI) return;
        
        VoiceChatUIObject thing = Instantiate(voiceChatUIObject, transform);
        thing.playerNameText.text = clientInstance.PlayerName;
        thing.playerClientInstance = clientInstance;
        thing.gameObject.SetActive(true);
        _voiceChatUIObjects.Add(thing);
    }

    public bool DisableVoiceChatUI = false;
    public void SetDisableVoiceChatUI(bool disable) {
        DisableVoiceChatUI = disable;
        if (disable) {
            foreach (VoiceChatUIObject obj in _voiceChatUIObjects) { Destroy(obj.gameObject); }
            _voiceChatUIObjects.Clear();
        }
        
        PlayerPrefs.SetInt("DisableVoiceChatUI", disable ? 1 : 0);
        PlayerPrefs.Save();
    }
}
