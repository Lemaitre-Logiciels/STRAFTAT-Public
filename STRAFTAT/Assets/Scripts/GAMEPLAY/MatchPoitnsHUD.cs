using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MatchPoitnsHUD : MonoBehaviour {
    /// <summary>
    /// This is the material used for the points HUD when a player has no points.
    /// </summary>
    [SerializeField] private Material inactiveMaterial;
    
    /// <summary>
    /// These are the materials for the points HUD.
    /// 0 is the first player, 2 is the 2nd player, 3 is the 3rd player, etc.
    /// </summary>
    [SerializeField] private Material[] activeMaterials;
    
    /// <summary>
    /// This is the hud overlay thing from before 2 players
    /// Materal Index 1 is the first player, index 2 is the winner, index 3 is the second player
    /// </summary>
    [SerializeField] private MeshRenderer primaryPointMesh;
    
    /// <summary>
    /// These are the spheres that represent the points for players 2 and beyond.
    /// </summary>
    [SerializeField] private MeshRenderer[] secondaryPointObjects;
    
    [SerializeField] private TMP_Text[] pointsTexts;
    
    public void Start() {
        UpdateVisuals(-1, ScoreManager.Instance.GetRoundScoreDictionary());
    }
    
    public void UpdateVisuals(int winnerTeamId, Dictionary<int, int> roundScores) {
        if (ClientInstance.playerInstances.Count <= 1) return;
        
        // Hack so that I can deal with mid match joining and leaving players
        List<int> teamsWithConnectedPlayers = new List<int>();
        foreach (int teamId in ScoreManager.Instance.TeamIdToPlayerIds.Keys) {
            foreach (int playerId in ScoreManager.Instance.TeamIdToPlayerIds[teamId]) {
                if (ClientInstance.playerInstances.ContainsKey(playerId)) {
                    teamsWithConnectedPlayers.Add(teamId);
                    break; // No need to check other players in the same team
                }
            }
        }
        
        int numberOfSecondaryPointsToEnable = teamsWithConnectedPlayers.Count - 2; // -2 because primaryPointMesh already has 2 players (1 and 2)
        for (int i = 0; i < numberOfSecondaryPointsToEnable; i++) {
            secondaryPointObjects[i].gameObject.SetActive(true);
        }
        int pointsToDisable = secondaryPointObjects.Length - numberOfSecondaryPointsToEnable;
        for (int i = secondaryPointObjects.Length - 1; i >= secondaryPointObjects.Length - pointsToDisable; i--) {
            if (i < 0) { continue; } // Safety check
            secondaryPointObjects[i].gameObject.SetActive(false);
        }
        foreach (TMP_Text pointsText in pointsTexts) { pointsText.text = ""; }
        
        Material[] primaryPointMeshMaterials = primaryPointMesh.materials;
        
        for (int i = 0; i < ScoreManager.Instance.TeamIdToPlayerIds.Count; i++) {
            int teamID = ScoreManager.Instance.TeamIdToPlayerIds.Keys.ElementAt(i);
            int roundScore = roundScores.GetValueOrDefault(teamID, 0);
            UpdateVisuals(i, roundScore,teamID == winnerTeamId, primaryPointMeshMaterials);
        }
        
        primaryPointMesh.materials = primaryPointMeshMaterials;
    }
    
    // teamNumber is not the teamId, but position of the team in the list, // so team 1 is 0, team 2 is 1, team 3 is 2, etc. even if team 3 is teamId 4 or smth
    public void UpdateVisuals(int teamNumber, int roundScore, bool isWinner, Material[] primaryPointMeshMaterials) {
        bool hasPoints = roundScore > 0;
        
        Material material = hasPoints ? activeMaterials[teamNumber] : inactiveMaterial;
        
        if (teamNumber < 2) { // teams 1 and 2
            int materialIndex = teamNumber == 0 ? 1 : 3;
            primaryPointMeshMaterials[materialIndex] = material;
        } else { // teams 3 and beyond
            int secondaryPlayerIndex = teamNumber - 2; // Adjust index for secondary points
            
            // It's okay to set this here since each object is only used by one player, so it would only be updated once, unlike the primary point mesh which is shared
            Material[] secondaryPointObjectsMaterials = secondaryPointObjects[secondaryPlayerIndex].materials;
            secondaryPointObjectsMaterials[1] = material;
            secondaryPointObjects[secondaryPlayerIndex].materials = secondaryPointObjectsMaterials;
        }
        
        // !isWinner since we don't want to show the score if it has won with the normal score of 2.
        if (roundScore == 2 && !isWinner || roundScore > 2) { pointsTexts[teamNumber].text = roundScore.ToString(); }

        // Set the winner material
        if (isWinner) { primaryPointMeshMaterials[2] = material; }
    }
}
