using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public struct Death {
    public int PlayerId;
    public float TimeOfDeath;
}

public class GameManager : NetworkBehaviour {
    public static GameManager Instance;
    private void Awake() {
        if (!Instance) { Instance = this; } 
        else { Destroy(this); }
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public string MainMenuSceneName = "MainMenu";
    public string VictorySceneName = "VictoryScene";
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == MainMenuSceneName) { ResetGame(); } // Removed || scene.name == VictorySceneName because it would reset scores in the victory scene screen : added it in SceneMotor.cs instead (line 229)
    }
    
    public void ResetGame() {
        ScoreManager.Instance.ResetScores();
        ScoreManager.Instance.ResetRound();
        if (waitForDrawCoroutine != null) { StopCoroutine(waitForDrawCoroutine); }
        waitForDrawCoroutine = null;
        alivePlayers.Clear();
        recentDeaths.Clear();

        timeTillStart = uint.MaxValue;
        roundWasWon = false;
    }
    
    public readonly HashSet<int> alivePlayers = new HashSet<int>();
    
    private readonly List<Death> recentDeaths = new List<Death>();
    
    [SyncVar] public bool roundWasWon = false;
    
    [ServerRpc(RequireOwnership = false)]
    public void PlayerDied(int playerId) {
        if (PauseManager.Instance && (PauseManager.Instance.inMainMenu || PauseManager.Instance.inVictoryMenu)) { return; }
        
        if (alivePlayers.Contains(playerId)) { alivePlayers.Remove(playerId); }
        recentDeaths.Add(new Death { PlayerId = playerId, TimeOfDeath = Time.time });
        
        // Find alive teams 
        int[] aliveTeams = GetAliveTeams();
        
        if (aliveTeams.Length <= 1 && waitForDrawCoroutine == null) {
            waitForDrawCoroutine = StartCoroutine(WaitForDraw());
        }
    }

    void Start() {
        if (IsServer) {
            if (SpawnerManager.Instance != null && RNGWeaponButton.toggle != null) SpawnerManager.Instance.randomiseWeapons = RNGWeaponButton.toggle.isOn;
            EnemyOutlinesEnabled = SteamLobby.Instance.enemyOutlineToggle.isOn;
            FriendlyFireEnabled = SteamLobby.Instance.friendlyFireToggle.isOn;
        }
    }
    
    public void StartGame() {
        ResetGame();
        if (IsServer) { 
            if (SpawnerManager.Instance != null && RNGWeaponButton.toggle != null) SpawnerManager.Instance.randomiseWeapons = RNGWeaponButton.toggle.isOn;
            EnemyOutlinesEnabled = SteamLobby.Instance.enemyOutlineToggle.isOn;
            FriendlyFireEnabled = SteamLobby.Instance.friendlyFireToggle.isOn;
            
            ConnectionsToStartGame.Clear();
            foreach (NetworkConnection client in Observers) { ConnectionsToStartGame.Add(client); }
            NetworkConnectionsReady.Clear();
        }
    }
    
    private float rttLastUpdated;
    public void Update() {
        if (IsOwner) {
            if (Time.time - rttLastUpdated > 0.1f) {
                AddRoundTripTime(Time.time, TimeManager.RoundTripTime / 1000f);
                rttLastUpdated = Time.time;
            }
        }
        
        if (!IsServer) { return; }
      
        if (PauseManager.Instance.inMainMenu || PauseManager.Instance.inVictoryMenu) { return; }

        
        recentDeaths.RemoveAll(death => Time.time - death.TimeOfDeath > 1f); // Remove deaths older than 1 second
    }
    
    private Coroutine waitForDrawCoroutine;
    IEnumerator WaitForDraw() {
        // wait a frame so if this throws for some reason, it 100000% will not throw on the first frame.
        // we want to avoid throwing on the first frame since its called from a server RPC and if you throw in one of those fihs net shits the bed.
        yield return null;
        
        Debug.Log("Waiting for draw...");
        
        ConnectionsToStartGame.Clear();
        foreach (NetworkConnection client in Observers) {
            if (!client.IsActive) { continue; }
            ConnectionsToStartGame.Add(client);
        }
        
        float elapsedTime = 0f;
        while (GetAliveTeams().Length > 0 && elapsedTime < 1f) {
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        int[] aliveTeams = GetAliveTeams();
        
        if (aliveTeams.Length > 1) {
            Debug.Log("A player came back to life?? This should never happen!");
            waitForDrawCoroutine = null;
            yield break;
        }
        
        if (SceneMotor.Instance.testMap) { 
            ProgressToNextTake(); 
            waitForDrawCoroutine = null;
            yield break;
        }
        
        Debug.Log($"aliveTeams length: {aliveTeams.Length}");
        if (aliveTeams.Length == 1) {
            int winningTakeTeamId = aliveTeams[0];
            ScoreManager.Instance.AddRoundScore(winningTakeTeamId);
            
             List<int> winningTeamPlayerIds = ScoreManager.Instance.GetPlayerIdsForTeam(winningTakeTeamId);
            
            StringBuilder logMessage = new StringBuilder();
            for (int index = 0; index < winningTeamPlayerIds.Count; index++) {
                int playerId = winningTeamPlayerIds[index];
                if (!ClientInstance.playerInstances.ContainsKey(playerId)) { continue; } // Check if player is still in the game, we keep playerIDs in the team because they could rejoin.
                
                logMessage.Append($"<color=#{PauseManager.Instance.selfNameLogColor}>{ClientInstance.playerInstances[playerId].PlayerNameTag}</color>");
                if (index < winningTeamPlayerIds.Count - 2) { logMessage.Append(", "); } 
                else if (index == winningTeamPlayerIds.Count - 2) { logMessage.Append(" and "); }
            }
            logMessage.Append(" won the take");
            
            PauseManager.Instance.WriteLog(logMessage.ToString());
        } else {
            int[] recentDeathsPlayerIds = recentDeaths.Select(death => death.PlayerId).ToArray();
            List<int> winningTakeTeamIds = new List<int>();
            foreach (int playerId in recentDeathsPlayerIds) {
                int teamId = ScoreManager.Instance.GetTeamId(playerId);
                if (winningTakeTeamIds.Contains(teamId)) { continue; }
                ScoreManager.Instance.AddRoundScore(teamId);
                winningTakeTeamIds.Add(teamId);
            }
            
            PauseManager.Instance.WriteLog($"A draw happened, players: {string.Join(", ", recentDeathsPlayerIds.Select(id => $"<color=#{PauseManager.Instance.selfNameLogColor}>{ClientInstance.playerInstances[id].PlayerNameTag}</color>"))} have scored a point for their teams.");
        }

        bool isRoundWon = ScoreManager.Instance.CheckForRoundWin(out int winningTeamId);
        UpdateMatchPointsHUD(isRoundWon ? winningTeamId : -1, ScoreManager.Instance.GetRoundScoreDictionary());
        
        yield return new WaitForSeconds(1f - elapsedTime);
        
        if (isRoundWon) { 
            RoundWon(winningTeamId);
            
            List<int> winningTeamPlayerIds = ScoreManager.Instance.GetPlayerIdsForTeam(winningTeamId);
            StringBuilder logMessage = new StringBuilder();
            for (int index = 0; index < winningTeamPlayerIds.Count; index++) {
                int playerId = winningTeamPlayerIds[index];
                if (!ClientInstance.playerInstances.ContainsKey(playerId)) { continue; } // Check if player is still in the game, we keep playerIDs in the team because they could rejoin.
                logMessage.Append($"<color=#{PauseManager.Instance.selfNameLogColor}>{ClientInstance.playerInstances[playerId].PlayerNameTag}</color>");
                if (index < winningTeamPlayerIds.Count - 2) { logMessage.Append(", "); } 
                else if (index == winningTeamPlayerIds.Count - 2) { logMessage.Append(" and "); }
            }
            logMessage.Append(" won the round");

            PauseManager.Instance.WriteLog(logMessage.ToString());
            RoundManager.Instance.CmdEndRound(winningTeamId);
            yield return new WaitForSeconds(4);
            SceneMotor.Instance.ChangeNetworkScene();
        }
        else {
            yield return new WaitForSeconds(2);
            ProgressToNextTake();
        }
        
        // repopulate the alive players list so if a player dies at the wrong time they don't win immediately
        alivePlayers.Clear();
        foreach (ClientInstance client in ClientInstance.playerInstances.Values) {
            alivePlayers.Add(client.PlayerId);
        }

        waitForDrawCoroutine = null;
    }
    
    [ObserversRpc(RunLocally = true)]
    private void UpdateMatchPointsHUD(int winningTeamId, Dictionary<int, int> roundScores) {
        ClientInstance.Instance?.PlayerSpawner.player?.matchPoitnsHUD.UpdateVisuals(winningTeamId, roundScores);
    }
    
    private void RoundWon(int winningTeamId) {
        ScoreManager.Instance.ResetRound();
        ScoreManager.Instance.AddPoints(winningTeamId);
        
        //SceneMotor.Instance.ChangeNetworkScene(); // Moved up
    }
    
    public void ProgressToNextTake() {
        ScoreManager.Instance.SetRoundIndex(ScoreManager.Instance.TakeIndex+1);
        ObserversRoundSpawn();
    }
    
    private int[] GetAliveTeams() {
        List<int> aliveTeams = new List<int>();
        foreach (int alivePlayerId in alivePlayers) {
            int teamId = ScoreManager.Instance.GetTeamId(alivePlayerId);
            if (!aliveTeams.Contains(teamId)) { aliveTeams.Add(teamId); }
        }
        return aliveTeams.ToArray();
    }
    
    public int[] GetConnectedTeams() {
        List<int> connectedTeams = new List<int>();
        foreach (ClientInstance player in ClientInstance.playerInstances.Values) {
            int teamId = ScoreManager.Instance.GetTeamId(player.PlayerId);
            if (!connectedTeams.Contains(teamId)) { connectedTeams.Add(teamId); }
        }
        return connectedTeams.ToArray();
    }

    [ObserversRpc(RunLocally = true)]
    public void ObserversRoundSpawn() {
        PauseManager.Instance.InvokeBeforeSpawn();
        ClientInstance.Instance.PlayerSpawner.RoundSpawn();
    }
    
    //Kick Client
    public void CmdKickPlayer(NetworkConnection conn, string message = null) {
        KickPlayerTarget(conn, message);
        conn.Disconnect(false); // Disconnect the player
    }

    [TargetRpc]
    void KickPlayerTarget(NetworkConnection conn, string message = null) {
        LobbyController.Instance.LocalPlayerController.KickSelf(); 
        if (!string.IsNullOrEmpty(message)) { PauseManager.Instance.WriteOfflineLog(message); }
    }
    
    // Couldnt think of a better place to put this, so here it is.
    // This is set by the server when the round starts, and is used to sync the round start time across clients.
    public float timeTillStart = float.MinValue;
    public bool hasSetStartTime = false;
    
    [ObserversRpc(RunLocally = true)]
    public void SetStartTime(float serverTimeTillStart) {
        float offset = (IsServer) ? 0f : GetAverageRoundTripTime() / 2f; // If we are the server, we don't need to adjust for round trip time
        timeTillStart = serverTimeTillStart - offset;
        hasSetStartTime = true;
    }
    public readonly HashSet<NetworkConnection> ConnectionsToStartGame = new HashSet<NetworkConnection>();
    public readonly HashSet<NetworkConnection> NetworkConnectionsReady = new HashSet<NetworkConnection>();
    public void RemoveNetworkConnection(NetworkConnection conn) {
        ConnectionsToStartGame.Remove(conn);
        NetworkConnectionsReady.Remove(conn);
    }
    
    public void SteamIdLeftHandlerShit(CSteamID steamId) {
        if (!IsServer) { return; }
        foreach (ClientInstance client in ClientInstance.playerInstances.Values) {
            if (client.PlayerSteamID != steamId.m_SteamID) { continue; }
            RemoveNetworkConnection(client.Owner);
            client.Owner.Disconnect(true);
            return;
        }
    }
    
    [SyncVar] public bool playingTeams = false;
    
    /// <summary>
    /// Scrambles the teams of all connected players.
    /// Keeps the number of players in each team the same, but randomly assigns players to teams.
    /// </summary>
    /// <param name="originalTeamSetup"> (TeamId, Players) before scramble </param>
    public void ScrambleTeams(Dictionary<int, List<int>> originalTeamSetup) {
        if (!IsServer) { return; } // Only the server can scramble teams, and I don't want to deal with this being called on clients so no more rpc
        
        // Get the connected teams
        int[] connectedTeams = GetConnectedTeams();
        int teamCount = connectedTeams.Length;
        if (teamCount < 2) { return; } // Not enough teams to scramble, so don't do anything
        
        // Get team sizes based on the connected players
        Dictionary<int, int> teamSizes = new Dictionary<int, int>();
        foreach (int teamId in connectedTeams) {
            // get the connected players in each team
            List<int> teamPlayers = ScoreManager.Instance.TeamIdToPlayerIds[teamId];
            int teamSize = 0;
            foreach (int playerId in teamPlayers) {
                if (!ClientInstance.playerInstances.ContainsKey(playerId)) { continue; }
                teamSize++;
            }
            if (teamSize > 0) { teamSizes[teamId] = teamSize; }
        }

        // Loop to prevent infinite loops
        for (int attempt = 0; attempt < 10; attempt++) {
            // Shuffled Player Ids
            int[] allPlayerIds = ClientInstance.playerInstances.Keys.OrderBy(x => Random.value).ToArray();

            // set the team IDs for each player based on the shuffled player IDs
            int playerIdIndex = 0;
            foreach (int teamId in connectedTeams) {
                if (!teamSizes.TryGetValue(teamId, out int teamSize)) { continue; }
                for (int i = 0; i < teamSize; i++) {
                    ScoreManager.Instance.SetTeamId(allPlayerIds[playerIdIndex], teamId);
                    playerIdIndex++;
                }
            }

            // Check if the new team setup is different from the original
            bool isDifferent = false;
            foreach (int teamId in connectedTeams) {
                HashSet<int> currentTeamPlayers = ScoreManager.Instance.GetPlayerIdsForTeam(teamId).ToHashSet();
                HashSet<int> originalTeamPlayers = originalTeamSetup[teamId].ToHashSet();

                if (currentTeamPlayers.SetEquals(originalTeamPlayers)) { continue; }
                
                isDifferent = true;
                break;
            }

            if (isDifferent) { break; }
        }
    }
    
    [SyncVar] public bool EnemyOutlinesEnabled = false;
    [SyncVar] public bool FriendlyFireEnabled = true;

    private readonly List<RoundTripTime> RoundTripTimes = new List<RoundTripTime>();
    private void AddRoundTripTime(float time, float roundTripTime) {
        RoundTripTimes.Add(new RoundTripTime(time, roundTripTime));
        RoundTripTimes.RemoveAll(rtt => Time.time - rtt.Time > 10f);
    }
    private float GetAverageRoundTripTime() {
        RoundTripTimes.RemoveAll(rtt => Time.time - rtt.Time > 10f);
        
        if (RoundTripTimes.Count == 0) { return 0f; }

        float totalRTT = 0f;
        foreach (RoundTripTime rtt in RoundTripTimes) { totalRTT += rtt.RTT; }

        return totalRTT / RoundTripTimes.Count;
    }
    private struct RoundTripTime {
        public readonly float Time;
        public readonly float RTT;
        public RoundTripTime(float time, float roundTripTime) {
            Time = time;
            RTT = roundTripTime;
        }
    }
}
