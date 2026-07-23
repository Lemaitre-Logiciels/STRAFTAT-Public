using FishNet;
using FishNet.Managing.Logging;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using InputSystem = UnityEngine.InputSystem;
using UnityEngine;
using FishNet.Component.Spawning;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Steamworks;
using FishNet.Managing;
using FishNet.Managing.Transporting;
using FishySteamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ComputerysModdingUtilities;
using LambdaTheDev.NetworkAudioSync;
using HeathenEngineering.SteamworksIntegration;
using UnityEngine.Serialization;

public class ClientInstance : NetworkBehaviour
{
    public static ClientInstance Instance;
    public static Dictionary<int, ClientInstance> playerInstances = new Dictionary<int, ClientInstance>();

    public bool textMuted = false;
    
    public PlayerManager PlayerSpawner;
    public bool alreadySpawned;

    public int ConnectionID;
    // Negitive value means not set
    [FormerlySerializedAs("PlayerIdNumber")] public int PlayerId = -1;
    
    public ulong PlayerSteamID;
    public string PlayerName;
    
    private static readonly Regex PlayerNameTagRegex = new Regex(@"\{PLAYER_NAME\}:\{(\d+)\}", RegexOptions.Compiled);
    public string PlayerNameTag => $"{{PLAYER_NAME}}:{{{PlayerId}}}";
    public static string ReplaceAllPlayerNameTags(string inputString) {
        return PlayerNameTagRegex.Replace(inputString, match => {
            string playerIdString = match.Groups[1].Value;
            if (int.TryParse(playerIdString, out int playerId)) {
                if (playerInstances.TryGetValue(playerId, out ClientInstance instance)) {
                    return instance.PlayerName;
                }
            }

            return match.Value;
        });
    }
    
    
    public bool Ready;
    
    /*
    [SyncVar] public int roundScore;
    [SyncVar] public int gameScore;
    public int roundIndex;
    public int roundIndexInfo;
    */

    private NetworkManager networkManager;
    private TransportManager transportManager;
    private PauseManager pauseManager;

    [HideInInspector] public bool nonSteamworksTransport;

    private InputSystem.InputAction record;
    private PlayerControls playerControls;

    [SerializeField] private AudioClip joinSfx;
    [SerializeField] private AudioClip leaveSfx;

    public override void OnStartClient() {
        base.OnStartClient();

        bool isUsingSteamworks = transportManager.Transport == networkManager.gameObject.GetComponent<FishySteamworks.FishySteamworks>();
        
        if (IsServer) { SetSyncValues(GetComponent<NetworkObject>()); }
        
        if (IsOwner) {
            playerControls = InputManager.inputActions;
            record = playerControls.Player.VoiceChat;
            record.Enable();

            Instance = this;
            PlayerSpawner = GetComponent<PlayerManager>();
            pauseManager = PauseManager.Instance;

            if (!SteamLobby.Instance.isInExplorationMap) SoundManager.Instance.PlaySound(joinSfx);

            
            if (isUsingSteamworks){
                LobbyController.Instance.LocalPlayerController = this;
                if (!IsServer) PauseManager.Instance.WriteLog($"{SteamFriends.GetPersonaName()} joined the lobby");
            } else {
                PlayerSpawner.PopulateSpawnPoints();
                PlayerSpawner.TryRespawn();
                return;
            }

            if (IsServer) {
                GameManager.Instance.playingTeams = SteamLobby.Instance.playingTeams;
                if (SteamLobby.Instance.playingTeams) { ScoreManager.Instance.Default2v2(); }
            }
            
            ReportModsToServer(AssemblyScanner.AllModNames);
            
        } else if (isUsingSteamworks){

            //SteamLobby.Instance.players.Add(GetComponent<NetworkObject>());
            if (InstanceFinder.TimeManager.ClientUptime > 0.5f) SoundManager.Instance.PlaySound(joinSfx);

            //StartCoroutine(LobbyController.Instance.clientPreview.GetComponent<AboubiPreviewLobby>().RunIntoLobby());

            if (SteamLobby.Instance.isInExplorationMap && InstanceFinder.NetworkManager.IsServer && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu") {
                SceneMotor.Instance.LeaveMatchForAll();
                MenuController.Instance.OpenGame();
                SceneMotor.Instance.testMap = false;
            }

            StartCoroutine(AddPlayerToHistory(this));
            
        }

        alreadySpawned = true;
        
    }
    
    [ServerRpc]
    private void ReportModsToServer(string[] modNames) => ReceiveModsFromServer(modNames);
    
    [ObserversRpc(ExcludeOwner = true)]
    private void ReceiveModsFromServer(string[] modNames) {
        if (modNames.Length == 0) {
            Debug.Log($"Player {PlayerName} ({PlayerSteamID}) has no mods.");
            return;
        }
        
        string modList = string.Join(", ", modNames);
        Debug.Log($"Player {PlayerName} ({PlayerSteamID}) has the following mods: {modList}");
    }

    IEnumerator AddPlayerToHistory(ClientInstance self){
        yield return new WaitForSeconds(5);
        if (self) { Settings.Instance.AddPlayerToHistory(self); }
    }

    
    IEnumerator StopPlayerRunning(int id) {
        yield return new WaitForSeconds(1);
        MenuAnimationServer(0, id);
    }

    private void SetSyncValues(NetworkObject newPlayer) {
        NetworkConnection conn = newPlayer.Owner;
        string steamIDString = conn.GetAddress();
        if (!ulong.TryParse(steamIDString, out ulong steamID)) {
            Debug.LogError("Failed to parse SteamID from connection address?!?!?, somthing is wrong, or I made a bad guess about how any of this undocumented shit works.");
            return;
        }
        Debug.Log($"A Player with the steamid: {steamID} joined in ({SteamFriends.GetFriendPersonaName(new CSteamID(steamID))}");
        
        int id = 0;
        
        // Dynamically choose player id
        ClientInstance[] clientInstances = FindObjectsOfType<ClientInstance>(); // There is probably a better way to do this, but this is fine for now
        
        int[] playerIds = clientInstances.Select(x => x.PlayerId).ToArray();

        int length = playerIds.Length;
        for (int i = 0; i < length; i++) {
            if (playerIds.Contains(i)) { continue; }
            id = i;
            break;
        }
        
        AddNewPlayer(conn, newPlayer, id, steamID);
        UpdateOnClients(SteamLobby.Instance.maxPlayers);
        
        RunIntoLobby(id);
        InitiateClient(conn, id);
    }

    [ObserversRpc]
    private void UpdateOnClients(int maxPlayers) {
        if (SteamLobby.Instance.players.Count > maxPlayers) return;

        SteamLobby.Instance.maxPlayers = maxPlayers;
        
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    [ContextMenu("UpdateOnClients")]
    public void UpdateOnClients(){
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    [ObserversRpc(BufferLast=true)]
    private void AddNewPlayer(NetworkConnection owner, NetworkObject newPlayer, int id, ulong steamid) {
        if (!SteamLobby.Instance.ValidateSteamIDisInLobby(steamid)) {
            Debug.LogError($"Player with SteamID {steamid} is not in the lobby! This should never happen, but if it does somebody cheating probly.");
            PlayerSteamID = 0;
        }
        else { PlayerSteamID = steamid; }
        
        PlayerId = id;
        
        ConnectionID = owner.ClientId;

        SteamLobby.Instance.players.Add(newPlayer);
        playerInstances.Add(id, this);

        PlayerName = FilterSystem.FilterString(SteamFriends.GetFriendPersonaName(new CSteamID(PlayerSteamID)));
        gameObject.name = PlayerName;
        
        if (!owner.IsLocalClient) { VoiceChatUI.Instance.CreateVoiceChatUIObject(this); }
        
        Debug.Log($"Added new player | SteamID: {PlayerSteamID} (reportedID: {steamid}), PlayerId: {PlayerId}, ConnectionID: {ConnectionID}, PlayerName: {PlayerName}");
    }

    [ObserversRpc]
    private void RunIntoLobby(int id) {
        AboubiPreviewLobby preview = LobbyController.Instance.previews[id];
        if (!preview) {
            Debug.LogError("Preview not found for player ID: " + id);
            return;
        }
        StartCoroutine(preview.RunIntoLobby());
        StartCoroutine(StopPlayerRunning(id));
    }

    [TargetRpc]
    private void InitiateClient(NetworkConnection conn, int id) {
        DressAboubi(CosmeticsManager.Instance.currenthat, CosmeticsManager.Instance.currentsuitIndex, CosmeticsManager.Instance.currentcigIndex, id);
        MenuAnimationServer(4, id);
    }

    [ServerRpc (RequireOwnership=false)] 
    public void UpdateServerMaxPlayers() {
        UpdateObserversMaxPlayers(SteamLobby.Instance.maxPlayers);
    }

    [ObserversRpc]
    private void UpdateObserversMaxPlayers(int maxPlayers) {
        SteamLobby.Instance.maxPlayers = maxPlayers;
    }

    public void ChangeReady()
    {
        ServerSetPlayerReady();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSetPlayerReady()
    {
        PlayerReadyUpdate(!Ready);
        PlayerReadyUpdated();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetPlayerReadyState(bool state)
    {
        PlayerReadyUpdate(state);
        PlayerReadyUpdated();
    }

    [ObserversRpc(BufferLast=true)]
    private void PlayerReadyUpdate(bool newValue)
    {
        Ready = newValue;
    }

    [ObserversRpc]
    private void PlayerReadyUpdated()
    {
        LobbyController.Instance.UpdatePlayerList();
    }

    void OnDisable()
    {
        if (!PauseManager.Instance.inMainMenu || !PauseManager.Instance.inVictoryMenu) GameManager.Instance.PlayerDied(PlayerId);
        SteamLobby.Instance.players.Remove(GetComponent<NetworkObject>());

        LobbyController.Instance.UpdatePlayerList();

        playerInstances.Remove(PlayerId);

        if (this != LobbyController.Instance.LocalPlayerController) return;

        if (!SteamLobby.Instance.isInExplorationMap) SoundManager.Instance.PlaySound(leaveSfx);
        
        SteamLobby.Instance.LeaveSteamLobby();
        
        playerInstances.Clear();
        
        record.Disable();
    }

    void Awake()
    {
        PlayerSpawner = GetComponent<PlayerManager>();
        networkManager = InstanceFinder.NetworkManager;
        transportManager = InstanceFinder.TransportManager;

        nonSteamworksTransport = transportManager.Transport != networkManager.gameObject.GetComponent<FishySteamworks.FishySteamworks>();
    }
    
    private void Update() {
        if (!IsOwner) { return; }

        VoiceChat();
        MenuAnimation();
        
        // What's this?
        if (nonSteamworksTransport ? Input.GetKeyDown(KeyCode.G) : Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha9) && Application.isEditor) {
            PlayerSpawner.TryRespawn();
        }
    }
    
    // Preview Aboubi in main menu

    [ServerRpc]
    public void DressAboubi(GameObject hat, int matIndex, int cigIndex, int id) {
        DressAboubiObservers(hat, matIndex, cigIndex, id);
    }

    [ObserversRpc (BufferLast = true)]
    public void DressAboubiObservers(GameObject hat, int matIndex, int cigIndex, int id) {
        AboubiPreviewLobby preview = LobbyController.Instance.previews[id];
        preview.parentObj = gameObject;
        preview.previewObject.SetActive(true);
        preview.ChangeDress(hat, CosmeticsManager.Instance.mats[matIndex], CosmeticsManager.Instance.cigs[cigIndex]);
    }

    // Menu Voice Chat

    [SerializeField] public VoiceStream vstream;
    [SerializeField] private VoiceRecorder vstreamRecorder;
    public AudioSource voiceChatSource;

    [ServerRpc]
    public void PlayVoiceChat(byte[] data)
    {
        PlayVoiceChatObservers(data);
    }

    [ObserversRpc]
    public void PlayVoiceChatObservers(byte[] data)
    {
        if (!IsOwner) vstream.PlayVoiceData(data);
    }

    void VoiceChat()
    {
        // Main Camera is a thing when you are in the kill cam.
        if (!pauseManager.inMainMenu && Camera.main ==null) {
            vstreamRecorder.IsRecording = false;
            return;
        }
        
        if (!Settings.Instance.enableVoiceChat || !vstreamRecorder) { return; }
        
        if (pauseManager.chatting) { return; }
        
        if (!Settings.Instance.enableVoiceChat) {
            vstreamRecorder.IsRecording = false;
            pauseManager.isRecording = false;
            return;
        }
        
        switch (Settings.Instance.voiceChatMode) {
            case VoiceChatMode.ToggleMute: {
                if (!record.WasPressedThisFrame()) { return; }
                vstreamRecorder.IsRecording = !vstreamRecorder.IsRecording;
                pauseManager.isRecording = vstreamRecorder.IsRecording;
                break;
            }
            case VoiceChatMode.OpenMic: {
                vstreamRecorder.IsRecording = true;
                pauseManager.isRecording = true;
                break;
            }
            case VoiceChatMode.PushToTalk: {
                if (record.ReadValue<float>() > 0.1f) {
                    vstreamRecorder.IsRecording = true;
                    pauseManager.isRecording = true;
                } else {
                    vstreamRecorder.IsRecording = false;
                    pauseManager.isRecording = false;
                }
                break;
            }
        }
        
        if (IsTalking != vstreamRecorder.IsRecording) { SetTalking(vstreamRecorder.IsRecording); }
    }

    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool IsTalking = false;
    [ServerRpc(RunLocally = true)]
    public void SetTalking(bool isTalking) {
        IsTalking = isTalking;
    }
    
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool IsTyping = false;
    [ServerRpc(RunLocally = true)]
    public void SetTyping(bool isTyping) { IsTyping = isTyping; }

    void MenuAnimation() {
        for (int i = 0; i < 6; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                MenuAnimationServer(i, this.PlayerId);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void MenuAnimationServer(int index, int id)
    {
        MenuAnimationObservers(index, id);
    }

    [ObserversRpc]
    public void MenuAnimationObservers(int index, int id) {
        Animator anim = LobbyController.Instance.previews[id].GetComponentInChildren<Animator>(true);

        switch (index) {
            case 0: {
                anim.SetBool("Crouch", false);
                anim.SetBool("Slide", false);
                anim.SetFloat("MovementSpeed", 0);
                anim.SetFloat("crouchMove", 0);
                break;
            }
            case 1: {
                anim.SetBool("Crouch", true);
                anim.SetBool("Slide", false);
                anim.SetFloat("MovementSpeed", 0);
                anim.SetFloat("crouchMove", 0);
                break;
            }
            case 2: {
                anim.SetBool("Crouch", false);
                anim.SetBool("Slide", true);
                anim.SetFloat("MovementSpeed", 0);
                anim.SetFloat("crouchMove", 0);
                break;
            }
            case 3: {
                anim.SetBool("Crouch", false);
                anim.SetBool("Slide", false);
                anim.SetFloat("MovementSpeed", 0.5f);
                anim.SetFloat("crouchMove", 0);
                break;
            }
            case 4: {
                anim.SetBool("Crouch", false);
                anim.SetBool("Slide", false);
                anim.SetFloat("MovementSpeed", 1);
                anim.SetFloat("crouchMove", 0);
                break;
            }
            case 5: {
                anim.SetBool("Crouch", true);
                anim.SetBool("Slide", false);
                anim.SetFloat("MovementSpeed", 0);
                anim.SetFloat("crouchMove", 1);
                break;
            }
        }
    }

    public void KickSelf() {
        if (InstanceFinder.NetworkManager.IsServer) { return; } // I don't think this can ever happen lol
        SteamLobby.Instance.LeaveLobby();
    }
}
