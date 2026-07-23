using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Linq;
using FishNet.Connection;
using TMPro;

public class VictoryMenu : InteractEnvironment
{
    [SerializeField] private InputAction menu;
    [SerializeField] private InputAction menu2;

    [SerializeField] private bool returnMenu;
    [SerializeField] private bool restartGame;
    [SerializeField] private bool quitGame;
    [SerializeField] private bool savePlaylist;
    [SerializeField] private bool sharePlaylist;
    [SerializeField] private bool scrambleTeams;

    private Vector3 restPos;
    [SerializeField] private float focusOffset = 0.15f;
    [SerializeField] private float pressOffset = 0.33f;
    [SerializeField] private float moveSpeed = 12;
    [Space]
    [SerializeField] private GameObject textObject;
    [SerializeField] private TMP_Text voteRematchText;
    [SerializeField] private TMP_Text playersText;

    private bool act;
    private bool focused;

    
    private HashSet<NetworkObject> playersThatCanRequestARestart = new HashSet<NetworkObject>();
    
    void Start()
    {
        restPos = transform.localPosition;

        if (!InstanceFinder.NetworkManager.IsServer && !savePlaylist && !restartGame && !sharePlaylist) {
            HideButton(); 
            return;
        }

        if (restartGame) {
            playersThatCanRequestARestart = new HashSet<NetworkObject>(SteamLobby.Instance.players);
        }

        if (scrambleTeams) {
            bool foundTeamWithMoreThanOnePlayer = false;
            foreach (int team in GameManager.Instance.GetConnectedTeams()) {
                if (!ScoreManager.Instance.TeamIdToPlayerIds.TryGetValue(team, out List<int> playerIds) || playerIds.Count <= 1) { continue; }
                foundTeamWithMoreThanOnePlayer = true;
                break;
            }
            
            if (!foundTeamWithMoreThanOnePlayer) {
                HideButton();
                return;
            }
            
            OriginalTeamSetup = new Dictionary<int, List<int>>(ScoreManager.Instance.TeamIdToPlayerIds);
        }
    }

    void HideButton() {
        transform.localScale = Vector3.zero;
        if (textObject != null) textObject.SetActive(false);
    }

    public override void OnFocus()
    {
        PauseManager.Instance.interactPopup.gameObject.SetActive(true);

        PauseManager.Instance.interactPopup.text = popupText.ToLower();

        focused = true;
    }


    public override void OnInteract(Transform player)
    {
        if (returnMenu)
            SteamLobby.Instance.LeaveMatch();

        if (restartGame)
            VoteRematchServer();

        if (quitGame)
            Application.Quit();

        if (savePlaylist)
            SaveCurrentPoolToPlaylist();

        if (sharePlaylist)
            SharePlaylist();

        if (scrambleTeams)
            ScrambleTeams();
    }


    public override void OnLoseFocus() { focused = false; }

    bool rematch = true;

    private void Update() {
        var desiredPosition = focused ? restPos + new Vector3(focusOffset, 0, 0) : act ? restPos + new Vector3(pressOffset, 0, 0) : restPos;

        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, moveSpeed * Time.deltaTime);


        if (restartGame) {
            playersThatCanRequestARestart.RemoveWhere(c => !c.Owner.IsActive);
            
            voteRematchText.text = $"REMATCH : ({VotedPlayers.Count}/{playersThatCanRequestARestart.Count})";
            
            string text = "";
            foreach (int playerId in VotedPlayers) {
                if (ClientInstance.playerInstances.TryGetValue(playerId, out ClientInstance instance)) { text += $"{instance.PlayerName}\n"; }
            }
            playersText.text = text;
        }

        if (rematch && restartGame && VotedPlayers.Count >= playersThatCanRequestARestart.Count) {
            SceneMotor.Instance.ServerRestartGameScene();
            rematch = false;
        }
    }
    
    [SyncObject] readonly SyncList<int> VotedPlayers = new SyncList<int>();
    [ServerRpc(RequireOwnership = false)]
    public void VoteRematchServer(NetworkConnection sender = null) {
        ClientInstance foundInstance = null;
        foreach (ClientInstance instance in ClientInstance.playerInstances.Values) {
            if (instance.Owner != sender) { continue; }
            foundInstance = instance;
            break;
        }
        if (foundInstance == null) { return; }
        
        int playerId = foundInstance.PlayerId;
        if (VotedPlayers.Contains(playerId)) { VotedPlayers.Remove(playerId); }
        else { VotedPlayers.Add(playerId); }
    }
    
    //
    public bool hasAskedForMapPool = false;
    void SaveCurrentPoolToPlaylist() {
        if (hasAskedForMapPool) { return; }
        hasAskedForMapPool = true;
        RequestArrayDataForPlayedMaps();
    }

    public bool hasAskedForPlaylist = false;
    void SharePlaylist() {
        if (hasAskedForPlaylist) { return; }
        hasAskedForPlaylist = true;
        RequestArrayData();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestArrayData(NetworkConnection sender = null) {
        GetPlaylist(sender, SceneMotor.Instance.PlayListMaps.ToArray());
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestArrayDataForPlayedMaps(NetworkConnection sender = null) {
        GetPlaylistPlayedMaps(sender, SceneMotor.Instance.PlayedMaps.ToArray());
    }
    
    [TargetRpc]
    private void GetPlaylist(NetworkConnection conn, string[] playlist) {
        if (!hasAskedForPlaylist) { return; } // This is so a bad actor can't spam the clients with without asking for the playlist first
        MapsManager.Instance.AddPlaylistWithoutUpdate(playlist, "Session " +  DateTime.Now);
        PauseManager.Instance.WriteOfflineLog("Session " +  DateTime.Now);
        
        gameObject.SetActive(false);
        hasAskedForPlaylist = false;
    }

    [TargetRpc]
    private void GetPlaylistPlayedMaps(NetworkConnection conn, string[] playlist) {
        if (!hasAskedForMapPool) { return; } // This is so a bad actor can't spam the clients with without asking for the playlist first
        MapsManager.Instance.AddPlaylistWithoutUpdate(playlist, "Session " +  DateTime.Now);
        PauseManager.Instance.WriteOfflineLog("Session " +  DateTime.Now);
        
        gameObject.SetActive(false);
        hasAskedForMapPool = false;
    }
    //

    private Dictionary<int, List<int>> OriginalTeamSetup = new Dictionary<int, List<int>>();
    void ScrambleTeams() { 
        GameManager.Instance.ScrambleTeams(OriginalTeamSetup); 
        PauseManager.Instance.WriteLog("Teams have been mixed"); 
    }
}
