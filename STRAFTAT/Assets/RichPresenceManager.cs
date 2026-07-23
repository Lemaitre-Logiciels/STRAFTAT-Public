using System.Text;
using ComputerysModdingUtilities;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RichPresenceManager : MonoBehaviour {
    public static RichPresenceManager Instance { get; private set; }

    private void Start() {
        if (Instance != null) {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        
        SteamFriends.SetRichPresence("steam_display", "#Status_Raw");
        SteamFriends.SetRichPresence("status", "#Status_Raw");
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        currentMapName = SceneManager.GetActiveScene().name;
        UpdateStatusFromGameState();
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    
    public void SetStatusText(string status) {
        if (status.Length > 256) { status = status.Substring(0, 256); } // I think this is the max length for a rich presence value bcs it's the max number of bytes a value can have.
        SteamFriends.SetRichPresence("raw_text", status);
        #if UNITY_EDITOR
        Debug.Log($"Set rich presence status: {status}");
        #endif
    }

    private string currentMapName = string.Empty;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name is "EmptyScene" or "MovedObjectsHolder") { return; }
        currentMapName = scene.name;
        UpdateStatusFromGameState();
    }

    public void UpdateStatusFromGameState() {
        if (!SteamLobby.Instance || SteamLobby.Instance.CurrentLobbyID == 0) {
            SteamFriends.SetRichPresence("steam_player_group", null);
            SteamFriends.SetRichPresence("steam_player_group_size", null);
            SteamFriends.SetRichPresence("connect", null);
            SetStatusText("In The Main Menu");
            return;
        }

        int playerCount = SteamMatchmaking.GetNumLobbyMembers((CSteamID)SteamLobby.Instance.CurrentLobbyID);
        if (SteamLobby.Instance.players.Count > 1) {
            SteamFriends.SetRichPresence("steam_player_group", SteamLobby.Instance.CurrentLobbyID.ToString());
            SteamFriends.SetRichPresence("steam_player_group_size", playerCount.ToString());
        } else {
            SteamFriends.SetRichPresence("steam_player_group", null);
            SteamFriends.SetRichPresence("steam_player_group_size", null);
        }
        
        StringBuilder sb = new();
        bool inGame = currentMapName != "MainMenu";
        sb.Append(inGame ? $"In Match: {currentMapName}" : "In a Lobby");
        int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)SteamLobby.Instance.CurrentLobbyID);
        sb.Append($" ({playerCount}/{maxPlayers})");
        
        bool isJoinable = false;
        if (playerCount < maxPlayers) {
            if (!inGame || SteamMatchmaking.GetLobbyData((CSteamID)SteamLobby.Instance.CurrentLobbyID, "allowMidMatchJoining") == "True") { isJoinable = true; }
        }
        
        SteamFriends.SetRichPresence("connect", isJoinable ? SteamLobby.Instance.CurrentLobbyID.ToString() : null);

        if (inGame && isJoinable) { sb.Append($" (Joinable)"); }
        if (!string.IsNullOrEmpty(AssemblyScanner.MatchMakingKey)) { sb.Append(" [MODDED]"); }
        
        SetStatusText(sb.ToString());
    }
}
