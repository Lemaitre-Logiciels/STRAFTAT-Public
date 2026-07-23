using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet;
using HeathenEngineering.SteamworksIntegration;

public class LobbyInviteInstance : MonoBehaviour
{
    public string lobbyName;
    public ulong lobbyID;
    [SerializeField] private TextMeshProUGUI text;

    void Start() {
        text.text = lobbyName;
    }


    public void Join()
    {
        if (InstanceFinder.NetworkManager.ServerManager.Started) SteamLobby.Instance.LeaveLobby();

        SteamLobby.Instance.DestroyInviteCards();

        if (lobbyID == 0) { return; }
        GameObject.Find("LobbyController").GetComponent<LobbyManager>().Join(lobbyID);
    }

    public void Refuse()
    {
        Destroy(gameObject);
    }
}
