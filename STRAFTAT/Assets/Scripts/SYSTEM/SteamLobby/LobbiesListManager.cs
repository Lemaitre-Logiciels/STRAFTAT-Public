using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager Instance;

    //Lobbies List Variables
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;

    public GameObject lobbiesButton, hostButton;

    public List<GameObject> listOfLobbies = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void GetListOfLobbies()
    {
        if (!lobbyListContent.activeInHierarchy) return;
        SteamLobby.Instance.GetLobbiesList();
    }

    public void DestroyLobbies()
    {
        foreach (GameObject lobbyItem in listOfLobbies)
        {
            Destroy(lobbyItem);
        }
        listOfLobbies.Clear();
    }

    public void DisplayLobbyWithData(CSteamID lobbyID) {
        string gameMode = SteamMatchmaking.GetLobbyData(lobbyID, "gamemode");
        if (gamemodeFilter != "All" && gamemodeFilter != gameMode) { return; }
        
        GameObject createdItem = Instantiate(lobbyDataItemPrefab, lobbyListContent.transform, true);
        LobbyDataEntry lobbyDataEntry = createdItem.GetComponent<LobbyDataEntry>();
            
        lobbyDataEntry.lobbyID = lobbyID;
        lobbyDataEntry.lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "name");
        lobbyDataEntry.lobbyName = FilterSystem.FilterString(lobbyDataEntry.lobbyName);
        lobbyDataEntry.ownDlc0 = SteamMatchmaking.GetLobbyData(lobbyID, "ownDlc0");
        lobbyDataEntry.gamemode = FilterSystem.FilterString(gameMode);
        lobbyDataEntry.playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);;
        lobbyDataEntry.maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
        
        lobbyDataEntry.RenderLobby();

        createdItem.transform.localScale = Vector3.one;

        listOfLobbies.Add(createdItem);
    }
    
    public string gamemodeFilter = "All";
    public void SetGamemodeFilter(string filter) {
        gamemodeFilter = filter;
        GetListOfLobbies();
    }
}
