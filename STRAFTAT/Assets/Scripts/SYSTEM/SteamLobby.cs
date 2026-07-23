using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using FishNet;
using System;
using System.Text;
using ComputerysModdingUtilities;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using FishNet.Managing;
using FishNet.Managing.Transporting;
using FishNet.Object;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using HeathenEngineering.DEMO;
using UnityEngine.Serialization;

public class SteamLobby : MonoBehaviour {
    public const string GameVersion = "Version 1.4.8a";
    
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;
    protected Callback<LobbyInvite_t > LobbyInvited;
    protected Callback<LobbyKicked_t > LobbyKicked;
    protected Callback<LobbyChatUpdate_t> LobbyChatUpdate;
    protected Callback<GameRichPresenceJoinRequested_t> RichPresenceJoinRequested;

    [Header("Lobby Settings")]
    public int maxPlayers = 4;
    public bool _allowMidMatchJoining = true;
    
    [Header("Current State (don't touch)")]
    public UserData localSteamUser;
    public ulong CurrentLobbyID;
    public bool inSteamLobby;
    public bool playingTeams;
    public bool findingQuickMatchLobby;
    public List<CSteamID> lobbyIDs = new List<CSteamID>();
    public List<NetworkObject> players = new List<NetworkObject>();
    private float CooldownTimer;
    private float lastActivityTime;
    public static bool ownDlc0 = false;
    public static bool ownDlc1 = false;
    public static HashSet<ulong> blockedLobbies = new HashSet<ulong>();
    public LobbyManager lobbyManager;
    public string QuickMatchFilterString = "All";
    public void SetQuickMatchFilterString(int value) {
        switch (value) {
            case 0: QuickMatchFilterString = "All"; break;
            case 1: QuickMatchFilterString = "Teams"; break;
            case 2: QuickMatchFilterString = "FFA"; break;
            case 3: QuickMatchFilterString = "1v1"; break;
        }
    }
    
    [Header("GameObject References")]
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;
    [SerializeField] private GameObject _sceneMotorPrefab;
    public GameObject ShutdownWindow;
    public GameObject LobbyTypeDropdownBeforeLobbyGameObject;
    public GameObject ServerNameObject;
    public GameObject MaxPlayersObject;
    public GameObject MapSelectionWindow;
    public GameObject QuickMatchFilter;
    public TMP_Dropdown LobbyTypeDropdown;
    public TMP_Dropdown LobbyTypeDropdownBeforeLobby;
    public TMP_Dropdown MaxPlayersDropdown;
    public TMP_Dropdown GamemodeDropdown;
    public Toggle enemyOutlineToggle;
    public Toggle friendlyFireToggle;
    public AlphaYoyo ReadyTextScript;
    public MenuHUDTween MapSelectionWindow3D;
    public GameObject LobbyWindow;
    public GameObject LobbiesBrowser;
    public GameObject MatchmakingBanner;
    public GameObject MatchmakingController;
    public GameObject HostButton;
    public GameObject StopButton;
    public GameObject InviteButton;
    public GameObject lobbyInviteInstance;
    public GameObject LobbyIdTextInfo;
    public Transform invitePopupViewport;
    public TextMeshProUGUI LobbyNameText;
    public LobbyIDHandler lobbyIdText;
    public TextMeshProUGUI versionText;
    [SerializeField] private MenuHUDTween[] lobby3D;
    
    // Singleton References
    private static PauseManager PauseManager => PauseManager.Instance;
    private static NetworkManager Manager => InstanceFinder.NetworkManager;
    
    // Singleton-ish
    public static SteamLobby Instance;

    void Awake() {
        versionText.text = GameVersion;
        if (Instance == null) Instance = this; // We don't destroy this object as other scripts will
    }

    [Space]
    [SerializeField] MapsManager mapsManager;

    void Start() {
        if (lobbyManager == null) { lobbyManager = GameObject.Find("LobbyController").GetComponent<LobbyManager>(); }

        AssemblyScanner.CreateMatchMakingKey();
        if (!string.IsNullOrEmpty(AssemblyScanner.MatchMakingKey)) {
            StringBuilder incompatibleMods = new StringBuilder("Incompatible mods detected: ");
            foreach (string assemblyName in AssemblyScanner.IncompatibleAssemblyNamesTrimmed) {
                incompatibleMods.Append(assemblyName + ", ");
            }
            
            PauseManager.Instance.ShowInfoPopup($"Non-vanilla friendly mods detected, you will be using a separate matchmaking pool for this session.\n{incompatibleMods}");
        }

        localSteamUser = (ulong)SteamUser.GetSteamID();
        
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyInvited = Callback<LobbyInvite_t >.Create(OnInviteReceived);
        LobbyKicked = Callback<LobbyKicked_t >.Create(OnLobbyKicked);
        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
        LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        RichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnRichPresenceJoinRequested);
        
        
        lobbyManager.searchArguments.stringFilters[0] = new StringFilter() {
            key = "version",
            value = $"{GameVersion}{AssemblyScanner.MatchMakingKey}"
        };
        lobbyManager.createArguments.metadata[0] = new MetadataTempalate() {
            key = "version",
            value = $"{GameVersion}{AssemblyScanner.MatchMakingKey}"
        };
        lobbyManager.createArguments.slots = maxPlayers;
        
        lastActivityTime = Time.time; // set AFK timer to current time

        StartCoroutine(FetchLobbyBlockList());
    }

    IEnumerator FetchLobbyBlockList() {
        string url = "https://raw.githubusercontent.com/C0mputery/StraftatLeaderboardWhitelist/main/LobbyBlockList";
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) {
                string[] lines = request.downloadHandler.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    if (ulong.TryParse(line.Trim(), out ulong lobbyId)) {
                        blockedLobbies.Add(lobbyId);
                    }
                }
                Debug.Log($"Fetched {blockedLobbies.Count} blocked lobbies.");
            } else {
                Debug.LogWarning($"Failed to fetch lobby block list: {request.error}");
            }
        }
    }
    
    void OnRichPresenceJoinRequested(GameRichPresenceJoinRequested_t callback) {
        if (!ulong.TryParse(callback.m_rgchConnect, out ulong lobbyId)) {
            Debug.LogError("Failed to parse lobby ID from rich presence join request.......................................................................................................................................................................................................................................");
            return;
        }

        if (!inSteamLobby) { SteamMatchmaking.JoinLobby((CSteamID)lobbyId); }
        else {
            LeaveLobby();
            StartCoroutine(JoinRichPresenceLobbyWithDelayyyyy((CSteamID)lobbyId));
        }
    }
    
    IEnumerator JoinRichPresenceLobbyWithDelayyyyy(CSteamID lobbyId) {
        yield return new WaitUntil(() => Manager.IsOffline);
        yield return new WaitForSeconds(0.3f);
        SteamMatchmaking.JoinLobby(lobbyId);
    }

    void AFKTimerThing() {
        // Check most inputs
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.anyKeyDown || Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            lastActivityTime = Time.time; // reset AFK timer
            return;
        }

        // 2.5 minutes AFK kick in the main menu
        if (!(Time.time - lastActivityTime > 150)) { return; }
        PauseManager.ShowInfoPopup($"You have been removed from the lobby's waiting room due to inactivity. (AFK for 2.5 minutes)");
        LeaveLobby();
    }
    
    public GameObject PSAObject;
    
    void Update() {
            
        // Okay so this might look entirely pointless but it is actually needed
        // For some reason without this getting checked and set every update
        // the FishySteamworks instance is null after leaving the lobby
        // --> thats because NetworkManager gameobject is reloaded each time you leave a lobby
        if (_fishySteamworks == null && Manager != null) { _fishySteamworks = Manager.GetComponent<FishySteamworks.FishySteamworks>(); }
        
        CooldownTimer -= Time.deltaTime;
        if (findingQuickMatchLobby && !inSteamLobby && CooldownTimer <= 0f) {
            findingQuickMatchLobby = false;
            Create();
        }
        
        if (MatchmakingBanner.activeSelf && players.Count >= 2) MatchmakingBanner.SetActive(false);
        
        if (LobbyIdTextInfo.activeSelf != inSteamLobby) LobbyIdTextInfo.SetActive(inSteamLobby);
        if (LobbyTypeDropdownBeforeLobbyGameObject.activeSelf == inSteamLobby) LobbyTypeDropdownBeforeLobbyGameObject.SetActive(!inSteamLobby);
        if (ServerNameObject.activeSelf == inSteamLobby) ServerNameObject.SetActive(!inSteamLobby);
        if (MaxPlayersObject.activeSelf == inSteamLobby) MaxPlayersObject.SetActive(!inSteamLobby);
        if (QuickMatchFilter.activeSelf == inSteamLobby) QuickMatchFilter.SetActive(!inSteamLobby);
        if (PSAObject.activeSelf == inSteamLobby) PSAObject.SetActive(!inSteamLobby);
        
        if (InviteButton.activeSelf != (Manager.IsServer && players.Count < maxPlayers)) InviteButton.SetActive(Manager.IsServer && players.Count < maxPlayers);


        //Safe Lock lobby 
        if (inSteamLobby && !Manager.IsOffline && Manager.IsServer){
            if (!PauseManager.inMainMenu && !_allowMidMatchJoining) {
                if (lobbyType != ELobbyType.k_ELobbyTypePrivate) { SetLobbyType(ELobbyType.k_ELobbyTypePrivate); }
            } 
            else if (lobbyType != GetLobbyType(LobbyTypeDropdown) && !privateLobby) {
                SetLobbyType(LobbyTypeDropdown);
            }
        }

        //Kick extra players
        if (players.Count > maxPlayers) 
        {
            if (LobbyController.Instance.LocalPlayerController != null)
            {
                int l=players.Count;
                for (int i=maxPlayers; i<l; i++) { if (LobbyController.Instance.LocalPlayerController == players[i].GetComponent<ClientInstance>()) KickSelf(); }
            } 
        }

        if (Manager.IsServer && PauseManager.inMainMenu && inSteamLobby && lobbyType == ELobbyType.k_ELobbyTypePublic) { AFKTimerThing(); }
        else { lastActivityTime = Time.time; }

        if (Input.GetKeyDown(KeyCode.F3) && PlayerListItem.DevloperIds.ContainsKey(SteamUser.GetSteamID().m_SteamID)) {
            showDev = !showDev; 
            if (inSteamLobby) { SteamMatchmaking.SetLobbyMemberData((CSteamID)CurrentLobbyID, "ShowDev", showDev ? "true" : "false"); }
            PauseManager.WriteOfflineLog(showDev ? "Dev indicator shown" : "Dev indicator hidden");
        }
    }

    public void SetMaxPlayers(TMP_Dropdown _dropdown)
    {
        maxPlayers = _dropdown.value+2;
        lobbyManager.createArguments.slots = maxPlayers;
        InstanceFinder.TransportManager.Transport.SetMaximumClients(maxPlayers-1); // -1 because the host is not counted as a player LOL
        _fishySteamworks.SetMaximumClients(maxPlayers-1); // okay what the fuck why does this not use fishnets transport system
        
        if (inSteamLobby) {
            SteamMatchmaking.SetLobbyMemberLimit(new CSteamID(CurrentLobbyID), maxPlayers);
            UpdatePlayerCountDisplay();

            if (LobbyController.Instance.LocalPlayerController != null) LobbyController.Instance.LocalPlayerController.UpdateServerMaxPlayers();
            
            SetGamemodeString();
        }
    }

    public Toggle midMatchJoiningToggle;
    public void SetAllowMidMatchJoining(bool value) {
        _allowMidMatchJoining = value; 
        if (inSteamLobby) {
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "allowMidMatchJoining", _allowMidMatchJoining ? "True" : "False");
        }
        midMatchJoiningToggle.SetIsOnWithoutNotify(_allowMidMatchJoining);
    }
    
    public void CreateLobbyDirect() { lobbyManager.Create(); }
    public void Create() {
        if (inSteamLobby) {
            LeaveLobby();
            StartCoroutine(CreateLobbyWithDelay());
        } else {
            CreateLobbyDirect();
        }

        CooldownTimer = 5f;
    }
    IEnumerator CreateLobbyWithDelay() {
        yield return new WaitUntil(() => Manager.IsOffline);
        yield return new WaitForSeconds(0.4f);
        CreateLobbyDirect();
    }

    bool privateLobby;
    [FormerlySerializedAs("isTestingMap")] [HideInInspector] public bool isInExplorationMap;

    public void EnterExplorationMap(string mapName) {
        if (!Manager.IsOffline && !Manager.IsServer) {
            PauseManager.WriteOfflineLog("Only the host can start exploration mode");
            return;
        }
        
        isInExplorationMap = true;
        if (!inSteamLobby) privateLobby = true;
        mapsManager.inExplorationMap = true;

        StartCoroutine(EnterMap(mapName));
    }

    IEnumerator EnterMap(string name) {
        yield return new WaitForSeconds(0.2f);
        if (!inSteamLobby) { 
            CreateLobbyDirect();
            SetLobbyType(ELobbyType.k_ELobbyTypePrivate);
            SetAllowMidMatchJoining(false);
        }
        yield return new WaitUntil(() => SceneMotor.Instance != null);
        yield return new WaitForSeconds(0.3f);
        SetLobbyType(ELobbyType.k_ELobbyTypePrivate);
        SceneMotor.Instance.EnterScene(name);
    }

    public bool startAutomatically;

    [Space]
    public string lobbyName;
    public ELobbyType lobbyType;

    public void SetLobbyType(TMP_Dropdown dropdown) { SetLobbyType(dropdown.value); }
    public void SetLobbyType(ELobbyType newLobbyType) {
        lobbyType = newLobbyType;
        UpdateLobbyType();
    }
    public void SetLobbyType(int value) {
        switch (value) {
            case (0) : lobbyType = ELobbyType.k_ELobbyTypePublic; break;
            case (1) : lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly; break;
            case (2) : lobbyType = ELobbyType.k_ELobbyTypePrivate; break;
        }
        UpdateLobbyType();
    }
    public ELobbyType GetLobbyType(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case (0) :return ELobbyType.k_ELobbyTypePublic;break;
            case (1) :return ELobbyType.k_ELobbyTypeFriendsOnly;break;
            case (2) :return ELobbyType.k_ELobbyTypePrivate;break;
        }
        return ELobbyType.k_ELobbyTypePublic;
    }

    public void UpdateLobbyType() {
        if (!inSteamLobby) return;
        SteamMatchmaking.SetLobbyType(new CSteamID(CurrentLobbyID), lobbyType);
        int value = lobbyType switch {
            ELobbyType.k_ELobbyTypePublic => 0,
            ELobbyType.k_ELobbyTypeFriendsOnly => 1,
            ELobbyType.k_ELobbyTypePrivate => 2,
            _ => 0
        };
        LobbyTypeDropdown.SetValueWithoutNotify(value);
    }

    public void SetGamemode(TMP_Dropdown dropdown) { SetGamemode(dropdown.value); }
    public void SetGamemode(int value) {
        
        playingTeams = value != 0;
        if (InstanceFinder.NetworkManager.IsServer) {
            if (GameManager.Instance != null) {
                GameManager.Instance.playingTeams = playingTeams;
                ScoreManager.Instance.ResetTeams();
                if (playingTeams) { ScoreManager.Instance.Default2v2(); }
            }
            if (inSteamLobby) { SetGamemodeString(); }
        }
    }

    public void SetGamemodeString() {
        if (maxPlayers == 2) { SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "gamemode", "1v1"); }
        else {
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "gamemode", (playingTeams ? "Teams" : "FFA"));
        }
    }

    public void SetStartAutomatically(Toggle toggle){
        startAutomatically = toggle.isOn;

        if (startAutomatically) AutomaticStart();
        else StopCoroutine(WaitForAllToConnectCoroutine);
    }
    public void AutomaticStart() {
        WaitForAllToConnectCoroutine = WaitForAllToConnect();
        StartCoroutine(WaitForAllToConnectCoroutine);
    }
    private IEnumerator WaitForAllToConnectCoroutine;
    [HideInInspector] public bool AllReady;
    IEnumerator WaitForAllToConnect() {
        // TODO: add support for more than 2 players here
        // Sirius edit : it already works ( I THINK) ! The AllReady boolean works for any number of players. 
        // And let's keep the minimum amount to 2 to start a game (even if it's a 4 players lobby)
        yield return new WaitUntil(() => AllReady && players.Count >= 2);
        if (Manager.IsServer) PauseManager.StartGameSteam();
    }
    
    public void SetHUDActive(bool active) {
        foreach (MenuHUDTween hud in lobby3D) {
            if (active) hud.SetEnabled();
            else hud.SetDisabled();
        }
    }
    
    // Callbacks
    private void OnLobbyCreated(LobbyCreated_t callback) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            PauseManager.WriteOfflineLog($"Failed to create lobby: {Enum.GetName(typeof(EResult), callback.m_eResult)}");
            return;
        }
        Debug.Log("Lobby Created");

        lastActivityTime = Time.time;

        CurrentLobbyID = callback.m_ulSteamIDLobby;

        inSteamLobby = true;

        if (!privateLobby) {
            LobbyTypeDropdown.value = LobbyTypeDropdownBeforeLobby.value;
            SetLobbyType(LobbyTypeDropdown);
            SetGamemode(GamemodeDropdown);
        }

        SetMaxPlayers(MaxPlayersDropdown);

        DestroyInviteCards();
        
        LobbyWindow.SetActive(true);

        HostButton.SetActive(false);
        StopButton.SetActive(true);
        SetHUDActive(true);

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "ownDlc0", (ownDlc0 ? "True" : "False"));
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "allowMidMatchJoining", _allowMidMatchJoining ? "True" : "False");
        SetGamemodeString();
        if (!privateLobby) {
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", (lobbyName == "" ? SteamFriends.GetPersonaName().ToString() + "'s lobby" : lobbyName));
        }
        else {
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", "private");
            SteamMatchmaking.SetLobbyType(new CSteamID(callback.m_ulSteamIDLobby), ELobbyType.k_ELobbyTypePrivate);
        }
        
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
        lobbyIdText.SetLobbyIDText(callback.m_ulSteamIDLobby.ToString());

        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        if (!_fishySteamworks.StartConnection(true)) {
            Debug.LogError("Failed to start FishySteamworks connection");
            LeaveLobby();
            return;
        }

        GameObject _sceneMotor = Instantiate(_sceneMotorPrefab, transform.position, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(_sceneMotor);

        if (startAutomatically) AutomaticStart();
    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback) {
        Debug.Log("JoinRequest");
        if (inSteamLobby) LeaveLobby();
        SteamMatchmaking.RequestLobbyData(callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    public void UpdatePlayerCountDisplay() { 
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "playerCount", (players.Count).ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "maxPlayers", maxPlayers.ToString());
    }
  
    public static readonly HashSet<ulong> bannedPlayers = new HashSet<ulong>();
    public void AddLobbyBan(ulong player) {
        if (!Manager.IsServer) return;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), player.ToString(), "banned");
        bannedPlayers.Add(player);
    }

    private bool showDev;
    
    string HostAddress;
    private void OnLobbyEntered(LobbyEnter_t callback) {
        RichPresenceManager.Instance.UpdateStatusFromGameState();
        
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess) {
            PauseManager.WriteOfflineLog($"Failed to join lobby: {Enum.GetName(typeof(EChatRoomEnterResponse), callback.m_EChatRoomEnterResponse)}");
            LeaveLobby();
            return;
        }
        Debug.Log("Lobby Entered");

        lastActivityTime = Time.time;
        
        if (lobbyManager.Lobby != (CSteamID)callback.m_ulSteamIDLobby) {
            lobbyManager.Lobby = (CSteamID)callback.m_ulSteamIDLobby;
            Debug.Log("steam overlay invite join");
        }
        
        string version = SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "version");
        if (version != $"{GameVersion}{AssemblyScanner.MatchMakingKey}") {
            PauseManager.WriteOfflineLog("Version mismatch, make sure you and the host are on the same version and have the same mods installed.");
            LeaveLobby();
            return;
        }
        
        // I cannot think of a better way to do this so yeah its dumb but so is steamworks
        string bannedString = SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, SteamUser.GetSteamID().ToString());
        if (bannedString == "banned") {
            PauseManager.WriteOfflineLog("You are banned from this lobby.");
            LeaveLobby();
            return;
        }
        
        DestroyInviteCards();

        /*
        Debug.Log(SteamMatchmaking.GetNumLobbyMembers((CSteamID) callback.m_ulSteamIDLobby));
        if (SteamMatchmaking.GetNumLobbyMembers((CSteamID) callback.m_ulSteamIDLobby) > 2) {
            PauseManager.WriteOfflineLog("Lobby is full");
            LeaveLobby();
            return;
        }*/

        if (!MenuController.Instance.playMenu.activeSelf && !isInExplorationMap) {
            MenuController.Instance.OpenGame();
        }
        
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyMemberData((CSteamID)CurrentLobbyID, "ShowDev", showDev ? "true" : "false");

        PauseManager.ShowInviteViewport(false, invitePopupViewport);

        LobbyWindow.SetActive(true);
        LobbiesBrowser.SetActive(false);
        SetHUDActive(true);

        if (!Manager.IsServer) {
            HostButton.SetActive(true);
            StopButton.SetActive(false);
        }

        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
        LobbyNameText.text = FilterSystem.FilterString(LobbyNameText.text);
        
        lobbyIdText.SetLobbyIDText(callback.m_ulSteamIDLobby.ToString());

        HostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress");
        _fishySteamworks.SetClientAddress(HostAddress);
        if (!_fishySteamworks.StartConnection(false)) {
            Debug.LogError("Failed to start FishySteamworks connection");
            LeaveLobby();
            return;
        }

        inSteamLobby = true;
    }
    private void OnInviteReceived(LobbyInvite_t result) {
        string steamNameOfLobbyOwner = SteamFriends.GetFriendPersonaName((CSteamID)result.m_ulSteamIDUser);
        var ins = Instantiate(lobbyInviteInstance, invitePopupViewport.position, Quaternion.identity, invitePopupViewport.transform);
        if (ins == null) Debug.LogError("Failed to invite to lobby???");
        
        LobbyInviteInstance inviteCard = ins.GetComponent<LobbyInviteInstance>();
        inviteCard.lobbyName = steamNameOfLobbyOwner;
        inviteCard.lobbyID = result.m_ulSteamIDLobby;
        PauseManager.ShowInviteViewport(true, invitePopupViewport);
        /*
            var ins = Instantiate(lobbyInviteInstance, invitePopupViewport.position, Quaternion.identity, invitePopupViewport.transform);
            currentInviteCard = ins;
            InvitedLobbyName = SteamMatchmaking.GetLobbyData((CSteamID)result.m_ulSteamIDLobby, "name");
            currentInviteCard.GetComponent<LobbyInviteInstance>().lobbyName = InvitedLobbyName;
            inviteInfo = false;
            pauseManager.ShowInviteViewport(true, invitePopupViewport);
         */
    }
    bool inviteInfo;
    void OnGetLobbyData(LobbyDataUpdate_t result) {
        CSteamID lobbyID = (CSteamID)result.m_ulSteamIDLobby;

        if (inSteamLobby && result.m_ulSteamIDLobby == CurrentLobbyID && result.m_ulSteamIDMember != result.m_ulSteamIDLobby) {
            if (PlayerListItem.DevloperIds.ContainsKey(result.m_ulSteamIDMember)) {
                if (LobbyController.Instance.Steamidtolistiem.TryGetValue(result.m_ulSteamIDMember, out PlayerListItems playerListItems)) {
                    bool active = SteamMatchmaking.GetLobbyMemberData(lobbyID, (CSteamID)result.m_ulSteamIDMember, "ShowDev") == "true";
                    if (playerListItems.PlayerListItem == null || playerListItems.PlayerListItemTab == null) { return; }
                    playerListItems.PlayerListItem.SetDev(active);
                    playerListItems.PlayerListItemTab.SetDev(active);
                }   
            }
        }

        if (blockedLobbies.Contains(result.m_ulSteamIDLobby)) { 
            Debug.Log($"Blocked lobby {result.m_ulSteamIDLobby}, name: {SteamMatchmaking.GetLobbyData(lobbyID, "name")}");
            return;
        }
        
        if (SteamMatchmaking.GetLobbyData(lobbyID, SteamUser.GetSteamID().ToString()) == "banned") { return; }
        
        int lobbyMaxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
        int lobbyPlayerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        if (lobbyMaxPlayers > 4 || lobbyPlayerCount < 1) { return; }
        
        LobbiesListManager.Instance.DisplayLobbyWithData(lobbyID);
        
        if (findingQuickMatchLobby && !inSteamLobby) {
            if (QuickMatchFilterString != "All") {
                string gameMode = SteamMatchmaking.GetLobbyData(lobbyID, "gamemode");
                if (QuickMatchFilterString != gameMode) { return; }
            }
            
            findingQuickMatchLobby = false;
            JoinLobby(lobbyID);
        }
    }
    private void OnLobbyKicked(LobbyKicked_t callback){ LeaveLobby(); }
    
    // This is to check who's left the lobby
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback) {
        EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
        if ((stateChange & EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0) {
            Debug.Log("Player Left");
            CSteamID playerLeft = (CSteamID)callback.m_ulSteamIDUserChanged;
            string playerName = SteamFriends.GetFriendPersonaName(playerLeft);
            if (HostAddress == playerLeft.ToString()) {
                PauseManager.WriteOfflineLog($"The Host ({playerName}) left the lobby");
                LeaveLobby();
            } else {
                PauseManager.WriteOfflineLog(playerName + " left the lobby");
                GameManager.Instance.SteamIdLeftHandlerShit(playerLeft);
            }

            if (ClientInstance.Instance.IsServer && !PauseManager.inMainMenu) {
                int[] activeTeamCount = GameManager.Instance.GetConnectedTeams();
                if (activeTeamCount.Length <= 1) {
                    LeaveMatch();
                }
            }
        }
        RichPresenceManager.Instance.UpdateStatusFromGameState();
    }

    private void OnApplicationQuit() { if (inSteamLobby) { LeaveMatch(); } }

    public void JoinLobby(CSteamID lobbyID) {
        if (inSteamLobby) LeaveLobby();
        StartCoroutine(JoinLobbyWithDelay(lobbyID));
    }
    IEnumerator JoinLobbyWithDelay(CSteamID lobbyID) {
        LobbiesBrowser.SetActive(false);
        yield return new WaitUntil(() => Manager.IsOffline);
        yield return new WaitForSeconds(0.3f);
        lobbyManager.Join(lobbyID);
    }
    public void JoinLobbyAuth(CSteamID lobbyID) {
        JoinLobby(lobbyID);
    }

    
    public void LeaveSteamLobby(bool reloadMenu = true) {
        if (startAutomatically && WaitForAllToConnectCoroutine != null) StopCoroutine(WaitForAllToConnectCoroutine);
        _fishySteamworks.Shutdown();
        if (reloadMenu) { StartCoroutine(ReloadNetworkManagerCoroutine()); }

        if (CurrentLobbyID != 0) {
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
            CurrentLobbyID = 0;
        }
    }
    
    IEnumerator ReloadNetworkManagerCoroutine()
    {
        yield return new WaitUntil(() => Manager.IsOffline);
        yield return new WaitUntil(() => !inSteamLobby);
        yield return new WaitForSeconds(0.2f);
        ReloadNetworkManager();
    }

    public void RunQuickMatch() {
        CooldownTimer = 5f;
        
        if (inSteamLobby) { 
            LeaveLobby();
            findingQuickMatchLobby = false;
            MatchmakingBanner.SetActive(false);
        }
        else if (findingQuickMatchLobby) { CancelQuickMatch(); }
        else {
            MatchmakingBanner.SetActive(true);
            findingQuickMatchLobby = true;
            GetLobbiesList();
        }
    }

    public void CancelQuickMatch() {
        findingQuickMatchLobby = false;
        if (inSteamLobby) LeaveLobby();
        MatchmakingBanner.SetActive(false);
    }
    
    public void JoinLobbyWithText(TMP_InputField text) {
        string vIn = text.text;
        ulong vOut = Convert.ToUInt64(vIn);
        
        lobbyManager.Join((CSteamID)(vOut));
        LobbiesBrowser.SetActive(false);
    }

    public void SetLobbyName(TMP_InputField text) {
        lobbyName = FilterSystem.FilterString(text.text);
    }

    public void LeaveLobby() {
        bannedPlayers.Clear();
        PauseManager.serverStarted = false;
        MatchmakingBanner.SetActive(false);

        if (LobbyChatUILogic.Instance != null) LobbyChatUILogic.Instance.ClearChat();

        PauseManager.WriteOfflineLog("You left the lobby");

        SaveLoadSystem.Instance.Save();

        _fishySteamworks.StopConnection(Manager.IsServer);
        if (!PauseManager.inMainMenu) { UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); }
        LobbyWindow.SetActive(false);

        lobbyIdText.SetLobbyIDText("Not In a Lobby");

        HostButton.SetActive(true);
        StopButton.SetActive(false);
        MapSelectionWindow.transform.localScale = Vector3.zero;
        MapSelectionWindow3D.SetDisabled();
        ReadyTextScript.SetState(true);
        SetHUDActive(false);

        inSteamLobby = false;
        findingQuickMatchLobby = false;

        privateLobby = false;
        isInExplorationMap = false;

        LeaveSteamLobby(false);
    }

    void ReloadNetworkManager()
    {
        GameObject networkManagerPrefab = GameObject.Find("NetworkManager");
        if (networkManagerPrefab) { Destroy(networkManagerPrefab); } 
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

        //Instantiate(NetworkManagerPrefab, Vector3.zero, Quaternion.identity);
    }

    public void LeaveMatch() {
        if (players.Count > 2 && !InstanceFinder.NetworkManager.IsServer) {
            LeaveLobby();
            return;
        }
        
        if (SceneMotor.Instance != null) {
            if (Manager.IsServer) SceneMotor.Instance.SetSceneIdToZero();
            if (SceneMotor.Instance.testMap) {
                SceneMotor.Instance.testMap = false;
                if (privateLobby){
                    LeaveLobby();
                    return;
                }
            }
        }
        if (PauseManager.inMainMenu) return;

        SaveLoadSystem.Instance.Save();

        if (LobbyChatUILogic.Instance != null) LobbyChatUILogic.Instance.ClearChat();

        if (!Manager.IsOffline) PauseManager.WriteLog($"{LobbyController.Instance.LocalPlayerController.PlayerNameTag} returned to main menu");

        if (SceneMotor.Instance != null) {
            //SceneMotor.Instance.RoundsAlgorithm();
            SceneMotor.Instance.LeaveMatchForAll();
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        privateLobby = false;
        isInExplorationMap = false;
    }
    
    public void KickSelf() {
        LobbyController.Instance.LocalPlayerController.KickSelf();
        PauseManager.WriteOfflineLog("Lobby is full");
    }
    
    public void DestroyInviteCards() {
        foreach (Transform child in invitePopupViewport) { Destroy(child.gameObject); }
    }

    public void GetLobbiesList() {
        if (lobbyIDs.Count > 0) { lobbyIDs.Clear(); }
        if (LobbiesListManager.Instance.listOfLobbies.Count > 0) { LobbiesListManager.Instance.DestroyLobbies(); }
        lobbyManager.Search(100);
    }

    // TODO: add support for friends only search
    void OnGetLobbyList(LobbyMatchList_t result) {
        
        // These should already be cleared as the only time we search for lobbies is the GetLobbiesList function
        if (lobbyIDs.Count > 0) lobbyIDs.Clear();
        if (LobbiesListManager.Instance.listOfLobbies.Count > 0) { LobbiesListManager.Instance.DestroyLobbies(); }
        
        // Add all the lobbies to the list
        for (int i = 0; i < result.m_nLobbiesMatching; i++) {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    public void OpenDLCStorePage() { SteamFriends.ActivateGameOverlayToStore((AppId_t)3153370, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None); }
    public void OpenSupporterDLCStorePage() { SteamFriends.ActivateGameOverlayToStore((AppId_t)3868930, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None); }

    //[SerializeField] private FriendInviteDropDown friendInviteScript;
    
    // Steam things
    public void OpenFriendChat(string inputField) {
        if (!uint.TryParse(inputField, out uint friendID)) { return; }
        
        UserData user = UserData.Get(friendID);
        if (!user.IsValid) { return; }
        
        SteamFriends.ActivateGameOverlayToUser("chat", (CSteamID)friendID);
    }

    public void JoinFriendLobby(string inputField) {
        if (!uint.TryParse(inputField, out uint friendID)) { return; }
        
        UserData user = UserData.Get(friendID);
        if (!user.IsValid) { return; }
        
        user.GetGamePlayed(out FriendGameInfo gameInfo);
        JoinLobby(gameInfo.Lobby);
    }

    public void InviteFriendToLobby(string inputField) {
        if (!uint.TryParse(inputField, out uint friendID)) { return; }
        
        UserData user = UserData.Get(friendID);
        if (!user.IsValid) { return; }
        
        PauseManager.WriteOfflineLog("Invited " + user.Name + " to lobby");
        SteamFriends.InviteUserToGame((CSteamID)friendID, CurrentLobbyID.ToString());
        //friendInviteScript.Invited.Invoke(user);
    }

    public void AddSteamFriend(UserData user) {
        if (!user.IsValid) { return; }
        SteamFriends.ActivateGameOverlayToUser("friendadd", (CSteamID)user);
    }

    public bool CanJoinFriend(string inputField) {
        if (!uint.TryParse(inputField, out var friendID)) { return false; }

        UserData user = UserData.Get(friendID);
        if (!user.IsValid) { return false; }

        if (friendID == localSteamUser.FriendId) { return false; }

        bool inGame = user.GetGamePlayed(out FriendGameInfo gameInfo);
        bool inlobby = (ulong)gameInfo.Lobby.SteamId != 0;
        return inGame && inlobby;
    }

    public bool CanInviteFriend(string inputField) {
        if (!uint.TryParse(inputField, out var friendID)) { return false; }
        
        UserData user = UserData.Get(friendID);
        if (!user.IsValid) { return false; }
        if (friendID == localSteamUser.FriendId) { return false; }
        if (players.Count <= 1) { return inSteamLobby && Manager.IsServer; }
        if (players[1].GetComponent<ClientInstance>().PlayerSteamID == user.SteamId) { return false; }
        return inSteamLobby && Manager.IsServer;
    }

    public ulong GetSteamIdFromLobbyIndex(int index) { return (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), index); }
    
    public bool ValidateSteamIDisInLobby(ulong steamId) {
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(CurrentLobbyID));
        for (int i = 0; i < memberCount; i++) {
            if ((ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), i) == steamId) { return true; }
        }
        return false;
    }
    
    public int GetLobbyIndexFromSteamId(ulong steamId) {
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(CurrentLobbyID));
        for (int i = 0; i < memberCount; i++) {
            if ((ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), i) == steamId) { return i; }
        }
        return int.MaxValue; // not found
    }

    public void OpenSteamInviteUI() { SteamFriends.ActivateGameOverlayInviteDialog((CSteamID)CurrentLobbyID); }
}
