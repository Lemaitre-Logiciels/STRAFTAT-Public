using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceChatUIObject : MonoBehaviour {
    [HideInInspector] public ClientInstance playerClientInstance;
    
    public TMP_Text playerNameText;
    public CanvasGroup canvasGroup;
    public void Update() {
        if (!playerClientInstance) {
            Destroy(gameObject);
            return;
        }

        canvasGroup.alpha = playerClientInstance.IsTalking ? 1f : 0f;
    }
}



