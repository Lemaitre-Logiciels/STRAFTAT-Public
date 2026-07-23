using FishNet.Managing.Client;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;

[RequireComponent(typeof(ClientManager))]
public class ClientManagerLogger : MonoBehaviour {
    private ClientManager ClientManager;
    
    private void Awake() {
        ClientManager = GetComponent<ClientManager>();
        ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
        ClientManager.OnRemoteConnectionState += OnRemoteConnectionStateChanged;
        ClientManager.OnAuthenticated += OnAuthenticated;
    }
    
    private void OnDestroy() {
        if (ClientManager == null) { return; }
        ClientManager.OnClientConnectionState -= OnClientConnectionStateChanged;
        ClientManager.OnRemoteConnectionState -= OnRemoteConnectionStateChanged;
        ClientManager.OnAuthenticated -= OnAuthenticated;
    }

    private void OnClientConnectionStateChanged(ClientConnectionStateArgs args) {
        switch (args.ConnectionState) {
            case LocalConnectionState.Started:
                Debug.Log("[FishNet] Client started.");
                break;
            case LocalConnectionState.Stopped:
                Debug.LogWarning("[FishNet] Client stopped.");
                if (SteamLobby.Instance.CurrentLobbyID != 0) SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.Instance.CurrentLobbyID));
                SteamLobby.Instance.CurrentLobbyID = 0;
                break;
            case LocalConnectionState.Starting:
                Debug.Log("[FishNet] Client is starting.");
                break;
            case LocalConnectionState.Stopping:
                Debug.LogWarning("[FishNet] Client is stopping.");
                break;
        }
    }
    
    private void OnRemoteConnectionStateChanged(RemoteConnectionStateArgs args) {
        Debug.Log($"[FishNet] Remote client {args.ConnectionId} state changed: {args.ConnectionState}.");
        if (args.ConnectionState == RemoteConnectionState.Stopped) {
            Debug.LogWarning($"[FishNet] Remote client {args.ConnectionId} disconnected.");
        }
    }
    
    private void OnAuthenticated() {
        Debug.Log("[FishNet] Client successfully authenticated.");
    }
}