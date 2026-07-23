using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class GameParameters : MonoBehaviour
{
    [SerializeField] private GameObject display;
    [SerializeField] private Transform mapSelectionWindow;
    // Start is called before the first frame update

    void Update()
    {
        if (!InstanceFinder.NetworkManager.IsServer) display.transform.localScale = Vector3.zero;
        else if (mapSelectionWindow.localScale == Vector3.one) display.transform.localScale = Vector3.zero;
        else display.transform.localScale = Vector3.one;
    }
    
    public void ToggleOutlines(bool state) {
        if (GameManager.Instance.IsServer) { GameManager.Instance.EnemyOutlinesEnabled = state; }
    }
    
    public void ToggleFriendlyFire(bool state) {
        if (GameManager.Instance.IsServer) { GameManager.Instance.FriendlyFireEnabled = state; }
    }
}
