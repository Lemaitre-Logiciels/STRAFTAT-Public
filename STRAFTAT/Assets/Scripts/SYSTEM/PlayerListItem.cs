using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using FishNet;
using TMPro;
using FishNet.Connection;
using HeathenEngineering.SteamworksIntegration;

public class PlayerListItem : MonoBehaviour { 
    public static readonly Dictionary<ulong, string> DevloperIds = new Dictionary<ulong, string>() {
        { 76561198299321868, "Developer"}, // Computery
        { 76561199176512752, "Developer" }, // Sirius
        { 76561198044115100, "Developer" }, // Leonard
        { 76561198025619453, "Mapper" }, // Dragonfly
        { 76561198152302843, "Mapper" }, // Jellyfish
        { 76561198099269397, "Mapper" }, // Alzuroth
        { 76561198154188879, "Mapper" } // SOnOva
    };

    public string PlayerName;
    public int ConnectionID;
    public int PlayerIdNumber;
    public ulong PlayerSteamID;
    public bool AvatarReceived;

    private ulong lobbyID;

    public TextMeshProUGUI PlayerNameText;
    public GameObject DevloperIndicator;
    public TMP_Text DevloperText;
    public RawImage PlayerIcon;

    public TextMeshProUGUI PlayerReadyText;
    public bool Ready;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    [SerializeField] private GameObject kickbutton;
    [SerializeField] private GameObject banbutton;
    [SerializeField] private GameObject cosmeticbutton;
    [SerializeField] private GameObject mutebutton;
    [SerializeField] private GameObject textmutebutton;
    [SerializeField] private GameObject addButton;
    [SerializeField] private TMP_Dropdown teamIdDropdown;
    [Space]
    [SerializeField] private Color readyColor;
    [SerializeField] private Color notreadyColor;

    PauseManager pauseManager;
    LobbyController lobbyController;
    GameManager gameManager;
    UserData thisUser;

    ulong localSteamId;

    public void ChangeReadyStatus()
    {
        if (Ready)
        {
            PlayerReadyText.text = "ready";
            PlayerReadyText.color = readyColor;
        }
        else{
            PlayerReadyText.text = "not ready";
            PlayerReadyText.color = notreadyColor;
        }
    }

    private void Start()
    {
        pauseManager = PauseManager.Instance;
        lobbyController = LobbyController.Instance;

        thisUser = UserData.Get(PlayerSteamID);
        localSteamId = (ulong)SteamUser.GetSteamID();

        lobbyID = SteamLobby.Instance.CurrentLobbyID;
        if (!InstanceFinder.NetworkManager.IsServer) {
            kickbutton.SetActive(false);
            banbutton.SetActive(false);
        }

        if (InstanceFinder.NetworkManager.IsServer && localSteamId == PlayerSteamID) {
            kickbutton.SetActive(false);
            banbutton.SetActive(false);
        }
        if (InstanceFinder.NetworkManager.IsServer && localSteamId != PlayerSteamID && SceneMotor.Instance != null) SceneMotor.Instance.CmdChangeRoundAmount();
        if (localSteamId != PlayerSteamID) teamIdDropdown.interactable=false;

        if (localSteamId == PlayerSteamID) {
            mutebutton.SetActive(false);
            textmutebutton.SetActive(false);
        }

        DevloperIndicator.SetActive(false);
        DevloperText.text = DevloperIds.GetValueOrDefault(PlayerSteamID, "not a dev");
        
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);

        EFriendRelationship relationship = SteamFriends.GetFriendRelationship((CSteamID)thisUser);
        Debug.Log($"PlayerListItem: Friend relationship: {relationship}");
        
        if (relationship == EFriendRelationship.k_EFriendRelationshipFriend || localSteamId == PlayerSteamID) addButton.SetActive(false);
        
        teamIdDropdown.value = ScoreManager.Instance.GetTeamId(PlayerIdNumber);
        teamIdDropdown.gameObject.name = $"TeamIdDropdownPlayer{PlayerIdNumber}";

        gameManager = GameManager.Instance;

        TrySetDev();
    }

    private void TrySetDev() {
        if (!DevloperIds.ContainsKey(PlayerSteamID)) { return; }
        SetDev(SteamMatchmaking.GetLobbyMemberData((CSteamID)LobbyController.Instance.CurrentLobbyID, (CSteamID)PlayerSteamID, "ShowDev") == "true");
    }

    public void SetDev(bool active) { DevloperIndicator.SetActive(active); }

    void Update()
    {
        //kickbutton.transform.localScale = pauseManager.inMainMenu ? Vector3.one : Vector3.zero;
        cosmeticbutton.transform.localScale = pauseManager.inMainMenu ? Vector3.one : Vector3.zero;
        bool isInTabMenu = transform.parent.gameObject.name == "-- TAB SCREEN --";

        if (PlayerIdNumber > 0) {
            if (isInTabMenu)
                transform.position = lobbyController.tabclientPosition[PlayerIdNumber-1].position;
            else
                transform.position = lobbyController.clientPosition[PlayerIdNumber-1].position;
        }

        if (gameManager) {
            teamIdDropdown.gameObject.SetActive(gameManager.playingTeams);
            if (InstanceFinder.NetworkManager.IsServer) teamIdDropdown.interactable = !isInTabMenu;
            else if (localSteamId == PlayerSteamID) teamIdDropdown.interactable = !isInTabMenu;
        }
    }
    
    void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayerSteamID);
        if (ImageID == -1) return;
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }

    public void CAcacas()
    {
        MenuController.Instance.ActivateMenu(MenuController.Instance.hatsMenu);
    }

    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;
        ChangeReadyStatus();
        if (!AvatarReceived) GetPlayerIcon();
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayerSteamID) //you
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else //another player
        {
            return;
        }
    }

    public void KickPlayer() {
        if (!GameManager.Instance) return;

        NetworkConnection desiredConnection = null;
        for (int i=0; i<SteamLobby.Instance.players.Count;i++) {
            if (SteamLobby.Instance.players[i].transform.GetComponent<ClientInstance>().ConnectionID == this.ConnectionID) {
                desiredConnection = SteamLobby.Instance.players[i].Owner;
                break;
            }
        }
        
        if (desiredConnection != null) {
            GameManager.Instance.CmdKickPlayer(desiredConnection, "You got kicked");
            PauseManager.Instance.WriteOfflineLog("Performed Kick");
        }
        else PauseManager.Instance.WriteOfflineLog("Kick Failed");
    }

    public void LobbyBan() {
        if (!GameManager.Instance) return;

        NetworkConnection desiredConnection = null;
        for (int i=0; i<SteamLobby.Instance.players.Count;i++) {
            if (SteamLobby.Instance.players[i].transform.GetComponent<ClientInstance>().ConnectionID == this.ConnectionID) {
                desiredConnection = SteamLobby.Instance.players[i].Owner;
                break;
            }
        }
        
        if (desiredConnection != null) {
            SteamLobby.Instance.AddLobbyBan(PlayerSteamID);
            GameManager.Instance.CmdKickPlayer(desiredConnection, "You've been banned from that lobby");
            PauseManager.Instance.WriteOfflineLog("Performed Lobby Ban");
        }
        else PauseManager.Instance.WriteOfflineLog("Lobby Ban Failed");
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarReceived = true;
        return texture;
    }

    [Space]
    [SerializeField] private Image muteImg;
    [SerializeField] private Sprite mute;
    [SerializeField] private Sprite notmute;

    public void SwitchMute()
    {
        var clients = GameObject.FindGameObjectsWithTag("ClientInstance");

        foreach (var audiosource in clients)
        {
            if (audiosource.GetComponent<ClientInstance>().PlayerSteamID == PlayerSteamID) audiosource.GetComponent<AudioSource>().mute = !audiosource.GetComponent<AudioSource>().mute;
        }

        muteImg.sprite = (muteImg.sprite == mute ? notmute : mute);
    }
    
    [Space]
    [SerializeField] private Image textMuteImg;
    [SerializeField] private Sprite textMute;
    [SerializeField] private Sprite textNotmute;
    
    public void SwitchTextMute() {
        // dont care how dumb this is
        GameObject[] clients = GameObject.FindGameObjectsWithTag("ClientInstance");
        foreach (GameObject client in clients) {
            ClientInstance instance = client.GetComponent<ClientInstance>();
            if (instance.PlayerSteamID != PlayerSteamID) { continue; }
            instance.textMuted = !instance.textMuted;
        }
        
        textMuteImg.sprite = (textMuteImg.sprite == textMute ? textNotmute : textMute);
    }
    
    void OnEnable()
    {
        if (lobbyID == 0) lobbyID = SteamLobby.Instance.CurrentLobbyID;
        if (lobbyID != SteamLobby.Instance.CurrentLobbyID) Destroy(gameObject);
        
    }

    public void OnDropdownTeamIdEnabled() {
        if (ScoreManager.Instance != null) {
            if (teamIdDropdown.value != ScoreManager.Instance.GetTeamId(PlayerIdNumber)) teamIdDropdown.value = ScoreManager.Instance.GetTeamId(PlayerIdNumber);
        }
    }

    public void AddFriend()
    {
        SteamLobby.Instance.AddSteamFriend(thisUser);
    }

    public void SetSelfTeamId(TMP_Dropdown dropdown) {
        if (ScoreManager.Instance == null) return;
        if (transform.parent.gameObject.name == "-- TAB SCREEN --") return;
        if (teamIdDropdown.value == ScoreManager.Instance.GetTeamId(PlayerIdNumber)) return;
        if (LobbyController.Instance.LocalPlayerController.PlayerId != PlayerIdNumber && !InstanceFinder.NetworkManager.IsServer) return;
         
        ScoreManager.Instance.SetTeamIdServer(PlayerIdNumber, dropdown.value);
    }

    
}
