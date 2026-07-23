using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using FishNet.Managing;

public class PlayerListItems {
    public PlayerListItem PlayerListItem;
    public PlayerListItem PlayerListItemTab;
}

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    //UI Elements
    public TextMeshProUGUI LobbyNameText;
    [SerializeField] private Transform hostPosition;
    public Transform[] clientPosition;

    [SerializeField] private Transform tabhostPosition;
    public Transform[] tabclientPosition;
    [Space]
    public AboubiPreviewLobby[] previews;
    [Space]

    //Player Data
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;

    //Other Data
    public ulong CurrentLobbyID;
    private Dictionary<int, PlayerListItems> PlayerIdToListItem = new Dictionary<int, PlayerListItems>();
    public Dictionary<ulong, PlayerListItems> Steamidtolistiem = new Dictionary<ulong, PlayerListItems>();
    public ClientInstance LocalPlayerController;

    //Ready status
    [SerializeField] private Button StartGameButton;
    [SerializeField] private TextMeshProUGUI ReadyButtonText;

    private SteamLobby manager;

    [Space]
    [SerializeField] private Transform tabScreen;

    void Awake() {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    void Start() { manager = SteamLobby.Instance; }

    public void ReadyPlayer() { LocalPlayerController.ChangeReady(); }

    public void UpdateButton() {
        string expectedText = LocalPlayerController.Ready ? "Unready" : "Ready !";
        if (ReadyButtonText.text != expectedText) { ReadyButtonText.text = expectedText; }
    }

    private bool HasEnoughPlayers() { return SteamLobby.Instance.players.Count >= 2 || Application.isEditor; }

    void Update()
    {
        if (LocalPlayerController == null) { return; }
        UpdatePlayerList();
        UpdateButton();
    }
    
    public void UpdateLobbyName()
    {
        CurrentLobbyID = manager.CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
        LobbyNameText.text = FilterSystem.FilterString(LobbyNameText.text);
    }

    public void UpdatePlayerList() {
        int readyPlayers = 0;
        int[] playerIds = new int[ClientInstance.playerInstances.Count];
        int index = 0;
        foreach ((int playerId, ClientInstance clientInstance) in ClientInstance.playerInstances) {
            playerIds[index] = clientInstance.PlayerId;
            index++;
            
            if (!PlayerIdToListItem.TryGetValue(playerId, out PlayerListItems listItems)) { CreateNewObjects(playerId, clientInstance); }
            else {
                listItems.PlayerListItem.PlayerName = clientInstance.PlayerName;
                listItems.PlayerListItemTab.PlayerName = clientInstance.PlayerName;
                listItems.PlayerListItem.Ready = clientInstance.Ready;
                listItems.PlayerListItemTab.Ready = clientInstance.Ready;
                listItems.PlayerListItem.SetPlayerValues();
                listItems.PlayerListItemTab.SetPlayerValues();
            }
            
            if (clientInstance.Ready) { readyPlayers++; }
        }

        int[] keys = PlayerIdToListItem.Keys.ToArray();
        foreach (int key in keys) {
            if (playerIds.Contains(key)) { continue; }
            PlayerListItems listItems = PlayerIdToListItem[key];
            PlayerIdToListItem.Remove(key);
            Destroy(listItems.PlayerListItem.gameObject);
            Destroy(listItems.PlayerListItemTab.gameObject);
        }
        
        bool allReady = readyPlayers == manager.players.Count;
        bool canStartGame = allReady && HasEnoughPlayers() && LocalPlayerController.IsServer;
        StartGameButton.interactable = canStartGame;
        SteamLobby.Instance.AllReady = canStartGame;
    }

    private void CreateNewObjects(int playerId, ClientInstance clientInstance) {
        GameObject newPlayerItem = Instantiate(PlayerListItemPrefab, PlayerListViewContent.transform, true);
        GameObject newPlayerItemTab = Instantiate(PlayerListItemPrefab, tabScreen, true);

        if (playerId == 0) {
            newPlayerItem.transform.position = hostPosition.position;
            newPlayerItemTab.transform.position = tabhostPosition.position;
        } else {
            int index = Mathf.Clamp(clientInstance.PlayerId - 1, 0, clientPosition.Length - 1);
            newPlayerItem.transform.position = clientPosition[index].position;
            newPlayerItemTab.transform.position = tabclientPosition[index].position;
        }
        newPlayerItem.transform.localScale = Vector3.one;
        newPlayerItemTab.transform.localScale = Vector3.one;
                
        PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();
        PlayerListItem newPlayerItemTabScript = newPlayerItemTab.GetComponent<PlayerListItem>();
        newPlayerItemScript.PlayerName = clientInstance.PlayerName;
        newPlayerItemTabScript.PlayerName = clientInstance.PlayerName;
        newPlayerItemScript.ConnectionID = clientInstance.ConnectionID;
        newPlayerItemTabScript.ConnectionID = clientInstance.ConnectionID;
        newPlayerItemScript.PlayerIdNumber = clientInstance.PlayerId;
        newPlayerItemTabScript.PlayerIdNumber = clientInstance.PlayerId;
        newPlayerItemScript.PlayerSteamID = clientInstance.PlayerSteamID;
        newPlayerItemTabScript.PlayerSteamID = clientInstance.PlayerSteamID;
        newPlayerItemScript.Ready = clientInstance.Ready;
        newPlayerItemTabScript.Ready = clientInstance.Ready;
        newPlayerItemScript.SetPlayerValues();
        newPlayerItemTabScript.SetPlayerValues();

        PlayerListItems newListItems = new PlayerListItems {
            PlayerListItem = newPlayerItemScript,
            PlayerListItemTab = newPlayerItemTabScript
        };
        
        Steamidtolistiem[clientInstance.PlayerSteamID] = newListItems;
        PlayerIdToListItem[playerId] = newListItems;
    }
}
