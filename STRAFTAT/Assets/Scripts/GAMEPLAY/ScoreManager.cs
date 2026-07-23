using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : NetworkBehaviour {
    public static ScoreManager Instance;
    private void Awake() {
        if (!Instance) { Instance = this; }
        else { Destroy(gameObject); }
        
        
    }
    
    // I made the choice to use int and not some reference to an object because i don't know how syncobject would work with that. ofc this leads to some headace but it does work.
    /// <summary>
    /// Player ID to Team ID mapping.
    /// THIS HAS THE PLAYERS THAT ARE NOT IN THE GAME AS WELL FOR MID MATCH JOINING
    /// </summary>
    [SyncObject(WritePermissions = WritePermission.ServerOnly)] public readonly SyncDictionary<int, int> PlayerIdToTeamId = new SyncDictionary<int, int>();
    /// <summary>
    /// Dictionary to hold the player IDs for each team.
    /// THIS HAS THE PLAYERS THAT ARE NOT IN THE GAME AS WELL FOR MID MATCH JOINING
    /// </summary>
    [SyncObject(WritePermissions = WritePermission.ServerOnly)] public readonly SyncDictionary<int, List<int>>  TeamIdToPlayerIds = new SyncDictionary<int, List<int>>();

    /// <summary>
    /// Get the team ID for a player.
    /// Or set the team ID for a player if it doesn't exist.
    /// </summary>
    public int GetTeamId(int playerId) {
        if (PlayerIdToTeamId.TryGetValue(playerId, out int teamId)) { return teamId; }
        
        // default to the player's own ID as their team ID
        SetTeamId(playerId, playerId);
        return playerId;
    }
    /// <summary>
    /// Get the player IDs for a team.
    /// INCLUDES PLAYERS THAT ARE NOT IN THE GAME AS WELL FOR MID MATCH JOINING
    /// </summary>
    public List<int> GetPlayerIdsForTeam(int teamId) {
        if (TeamIdToPlayerIds.TryGetValue(teamId, out List<int> playerIds)) { return playerIds; }
        return new List<int>();
    }
    /// <summary>
    /// Sets the team ID for a player.
    /// will add/remove from all the lists and stuff
    /// </summary>
    public void SetTeamId(int playerId, int teamId) {
        // Remove from current team if exists
        if (PlayerIdToTeamId.TryGetValue(playerId, out int currentTeamId)) {
            if (currentTeamId == teamId) { return; }
            
            if (TeamIdToPlayerIds.TryGetValue(currentTeamId, out List<int> currentPlayerIds)) {
                List<int> newPlayerIds = new List<int>(currentPlayerIds);
                newPlayerIds.Remove(playerId);
                
                if (newPlayerIds.Count == 0) { TeamIdToPlayerIds.Remove(currentTeamId); }
                else { TeamIdToPlayerIds[currentTeamId] = newPlayerIds; }
            }
        }
        
        // Add to new team
        if (TeamIdToPlayerIds.TryGetValue(teamId, out List<int> playerIds)) {
            // No need to check if playerId already exists, as it was removed from the previous team above
            List<int> newPlayerIds = new List<int>(playerIds) { playerId };
            TeamIdToPlayerIds[teamId] = newPlayerIds;
        } else {
            List<int> newPlayerIds = new List<int> { playerId };
            TeamIdToPlayerIds[teamId] = newPlayerIds;
        }

        // Update PlayerIdToTeamId
        PlayerIdToTeamId[playerId] = teamId;
    }
    public void ResetTeams() {
        PlayerIdToTeamId.Clear();
        TeamIdToPlayerIds.Clear();
    }
    public void Default2v2() {
        SetTeamId(0, 0);
        SetTeamId(1, 0);
        SetTeamId(2, 1);
        SetTeamId(3, 1);
    }
    
    // I really don't like these two methods being here...
    [ServerRpc (RequireOwnership=false)]
    public void SetTeamIdServer(int playerId, int teamId, NetworkConnection conn = null){
        if (conn == null) {  return; }
        if (!conn.IsHost) {
            if (PauseManager.Instance == null || !PauseManager.Instance.inMainMenu) {  return; }
            if (!ClientInstance.playerInstances.TryGetValue(playerId, out ClientInstance instance) || instance.Owner != conn) {  return; }
        }

        SetTeamId(playerId, teamId);
        UpdateDropdown(playerId, teamId);
    }
    [ObserversRpc]
    public void UpdateDropdown(int playerId, int teamId) {
        if (GameObject.Find($"TeamIdDropdownPlayer{playerId}") != null)
            GameObject.Find($"TeamIdDropdownPlayer{playerId}").GetComponent<TMP_Dropdown>().value = teamId;
    }
    
    /// <summary>
    /// Dictionary to hold the points for each team.
    /// points are the number of times a team has won a round.
    /// </summary>
    [SyncObject(WritePermissions = WritePermission.ServerOnly)] public readonly SyncDictionary<int, int> Points = new SyncDictionary<int, int>();
    public int GetPoints(int teamId) { return Points.GetValueOrDefault(teamId, 0); }
    public void AddPoints(int teamId, int score = 1) { if (!Points.TryAdd(teamId, score)) { Points[teamId] += score; } }
    public void ResetScores() { Points.Clear(); }
    
    /// <summary>
    /// The number of rounds required to win a round and gain a point.
    /// winning team is the team with the most points at the end of the round.
    /// </summary>
    public int RoundScoreRequiredToWin = 5;
    public bool CheckForRoundWin(out int winningTeamId) {
        winningTeamId = -1;
        
        int highestScore = 0;
        foreach (int i in TeamIdToPlayerIds.Keys) { // Iterate through all teams
            int score = GetRoundScore(i);
            if (score > highestScore) {
                winningTeamId = i;
                highestScore = score;
            } else if (score == highestScore) {
                winningTeamId = -1; // Tie, no winner
            }
        }
        
        return winningTeamId != -1 && highestScore >= RoundScoreRequiredToWin;
    }
    
    /// <summary>
    /// Dictionary to hold the scores for the current round.
    /// Number of times a player has scored in the current round.
    /// </summary>
    [SyncObject(WritePermissions = WritePermission.ServerOnly)] public readonly SyncDictionary<int, int> RoundScore = new SyncDictionary<int, int>();

    public Dictionary<int, int> GetRoundScoreDictionary() { return RoundScore.GetCollection(false); }
    public int GetRoundScore(int playerId) { return RoundScore.GetValueOrDefault(playerId, 0); }
    public void AddRoundScore(int playerId, int score = 1) { if (!RoundScore.TryAdd(playerId, score)) { RoundScore[playerId] += score; } }
    public void ResetRound() {
        RoundScore.Clear(); 
        SetRoundIndex(0);
    }
    
    /// <summary>
    /// The progress of the current round.
    /// </summary>
    [SyncVar] public int TakeIndex = 0; // The client needs the updated round index too
    [ServerRpc(RequireOwnership=false, RunLocally=true)] public void SetRoundIndex(int n) { TakeIndex = n; }
}
