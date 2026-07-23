using HeathenEngineering.DEMO;
using UnityEngine;

public class ChatShowEnforcer : MonoBehaviour {
    [SerializeField] private LobbyChatUILogic lobbyChatUILogic;

    private void OnEnable() { lobbyChatUILogic.EnableForceShowMessages(); }
    private void OnDisable() { lobbyChatUILogic.DisableForceShowMessages(); }
}
