using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet;
using System.Linq;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine.SceneManagement;
using TMPro;
using FishNet.Managing;

public class SceneMotor : NetworkBehaviour
{
    public static SceneMotor Instance;

    public bool firstToXWins;
    [Space] 
    [SerializeField]
    public readonly List<string> PlayListMaps = new();
    public readonly Queue<string> PlayListMapsQueue = new();
    public readonly HashSet<string> PlayedMaps = new();
    
    public List<string> mapsPlayed = new();
    
    [SerializeField] private string victoryScene = "VictoryScene";
    [SyncVar] public int sceneIndex;
    string currentSceneName;
    public void IncrementScene() { sceneIndex++; }
    
    [SerializeField] public GameObject _loaderCanvas;
    [SerializeField] public TextMeshProUGUI sceneText;

    private TMP_Dropdown roundAmountDropdown;
    [SyncVar] public int roundAmount;
    bool canLoadIntoScene;

    [Space]
    [SerializeField] private GameObject explorationText;

    void Awake() {
        Instance = this;
    }


    private void OnEnable() {
        InstanceFinder.SceneManager.OnLoadStart += OnLoadSceneStart;
        InstanceFinder.SceneManager.OnLoadEnd += OnLoadSceneEnd;
    }

    private void Start()
    {
        StartCoroutine(CanLoadIntoSceneEnable());

        if (SteamLobby.Instance.isInExplorationMap) return;
        MapSelection.Instance.InitiateMaps();
        roundAmountDropdown = GameObject.Find("RoundAmount").GetComponent<TMP_Dropdown>();

        // 4 is actually 6 rounds, because the dropdown starts at 0
        // I think 4 rounds for 1v1 needs testing and is probably better for casual and new players (too many ragequits in first to 6)
        // roundAmountDropdown.value = SteamLobby.Instance.maxPlayers == 2 ? 4 : 2;

        OnRoundAmountChange();

        roundAmountDropdown.onValueChanged.AddListener(delegate { OnRoundAmountChange(); });

    }

    IEnumerator CanLoadIntoSceneEnable() {
        yield return new WaitForSeconds(0.1f);
        canLoadIntoScene = true;
    }


    public void OnRoundAmountChange()
    {
        roundAmountDropdown = GameObject.Find("RoundAmount").GetComponent<TMP_Dropdown>();
        if (InstanceFinder.NetworkManager.IsServer) CmdChangeRoundAmount();
    }
    
    public string GetNextMap() {
        string next = PlayListMapsQueue.Dequeue();
        PlayListMapsQueue.Enqueue(next);
        return next;
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void CmdChangeRoundAmount()
    {
        roundAmount = roundAmountDropdown.value + 2;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftControl) && Application.isEditor)
        {
            ChangeNetworkScene();
        }

        if (Instance._loaderCanvas.activeSelf)
            if (Camera.main == null)
                if (!inLoadingScreen) Instance._loaderCanvas.SetActive(false);

        //if (!InstanceFinder.NetworkManager.IsServer) testMap = false;
        if (explorationText.activeSelf != testMap) explorationText.SetActive(testMap);
    }

    public void ChangeNetworkScene()
    {
        MapsManager.Instance.inExplorationMap = false;
        if (firstToXWins) {
            foreach (NetworkObject player in SteamLobby.Instance.players) {
                int playerId = player.GetComponent<ClientInstance>().PlayerId;
                int teamId = ScoreManager.Instance.GetTeamId(playerId);
                if (ScoreManager.Instance.GetPoints(teamId) >= roundAmount) {
                    ServerEndGameScene();
                    return;
                }
            }
        } else if (sceneIndex == 0) {
            ServerEndGameScene();
            return;
        }
        
        PlayedMaps.Add(currentSceneName);
        currentSceneName = GetNextMap();
        
        // I don't know what the hell this does but I am to scared to remove it
        if (currentSceneName == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) {
            SceneLoadData sldEmpty = new SceneLoadData("EmptyScene");
            sldEmpty.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sldEmpty);
        }

        SceneLoadData sld = new SceneLoadData(currentSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        /*foreach (var conn in InstanceFinder.ServerManager.Clients.Values.ToArray())
        {
            InstanceFinder.SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName((Instance.sceneIndex == Instance.gameMaps.Length-1 ? Instance.gameMaps[0] : Instance.gameMaps[sceneIndex])));
        }*/

        Instance.ChangeSceneId();
    }

    public void ServerStartGameScene()
    {
        // Check if not all players are in the same team
        bool canStartTeams=false;
        int teamid=ScoreManager.Instance.GetTeamId(ClientInstance.playerInstances[0].PlayerId);
        for (int i=0; i<4;i++) {
            if (ClientInstance.playerInstances.ContainsKey(i)) {
                if (ScoreManager.Instance.GetTeamId(ClientInstance.playerInstances[i].PlayerId) != teamid) {
                    canStartTeams = true;
                    break;
                }
            }
        }

        if (Instance.PlayListMaps.Count == 0)
        {
            PauseManager.Instance.WriteOfflineLog("No Maps selected !");
        }
        else if (canStartTeams == false && GameManager.Instance.playingTeams && ClientInstance.playerInstances.Count > 1) {
            PauseManager.Instance.WriteLog("All Players are in the same Team !");
        }
        else {
            currentSceneName = GetNextMap();
            
            PlayedMaps.Clear();
            PlayedMaps.Add(currentSceneName);
            
            GameManager.Instance.StartGame();
            
            SceneLoadData sld = new SceneLoadData(currentSceneName);
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);

            Instance.ChangeSceneId();
        }
    }

    public void ServerRestartGameScene() {
        currentSceneName = GetNextMap();
            
        PlayedMaps.Clear();
        PlayedMaps.Add(currentSceneName);
        
        GameManager.Instance.StartGame();
        
        SceneLoadData sld = new SceneLoadData(currentSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        Instance.ResetSceneId();
    }

    public void ServerEndGameScene()
    {
        
        SceneLoadData sld = new SceneLoadData(victoryScene);
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        Instance.ChangeSceneId();
    }

    public void ServerLeaveGameScene()
    {
        SceneLoadData sld = new SceneLoadData("MainMenu");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    public void ClientLeaveGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeSceneId() {
        Instance.IncrementScene();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetSceneId(NetworkConnection conn = null)
    {
        if (conn == null || !conn.IsHost) { return; }
        
        Instance.sceneIndex = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSceneIdToZero(NetworkConnection conn = null)
    {
        if (conn == null || !conn.IsHost) { return; }
        
        Instance.sceneIndex = 0;
    }

    [ServerRpc (RequireOwnership = false)]
    void CloseScene(string sceneToClose, NetworkConnection conn = null)
    {
        if (conn == null || !conn.IsHost) { return; }

        CloseScenesObserver(sceneToClose);
    }

    [ObserversRpc]
    void CloseScenesObserver(string sceneToClose)
    {

        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneToClose);
        
    }

    public void Shuffle(List<string> texts) {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Count; t++ ) {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Count);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    [ServerRpc (RunLocally = true)]
    public void ReturnMenuServer()
    {
        ReturnMenuObservers();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    void ReturnMenuObservers()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    [ServerRpc (RequireOwnership = false)]
    public void LeaveMatchForAll(NetworkConnection conn = null)
    {
        if ((conn == null || !conn.IsHost) && SteamLobby.Instance.players.Count > 2) { return; }
        
        SceneLoadData sld = new SceneLoadData("MainMenu");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        //RoundsAlgorithm();
        SetSceneIdToZero();
    }

    public bool inLoadingScreen;

    public void OnLoadSceneStart(SceneLoadStartEventArgs args)
    {
        if (!canLoadIntoScene) return;
        inLoadingScreen = true;
        Instance._loaderCanvas.SetActive(true);
        //Instance.sceneText.text = "entering " + Instance.gameMaps[Instance.sceneIndex] + " ...";
    }

    public void ShowLoadingScreen()
    {
        Instance._loaderCanvas.SetActive(true);
        //Instance.sceneText.text = "entering " + Instance.gameMaps[Instance.sceneIndex] + " ...";
    }

    public void OnLoadSceneEnd(SceneLoadEndEventArgs args)
    {
        Instance.StartCoroutine(Instance.DelayLoad());
    }

    IEnumerator DelayLoad()
    {
        yield return new WaitForSeconds(0.25f);
        inLoadingScreen = false;
        Instance._loaderCanvas.SetActive(false);
    }

    public IEnumerator StartGameClients()
    {
        yield return new WaitForSeconds(1.2f);

        Instance._loaderCanvas.SetActive(false);
    }

    public bool testMap;

    [ServerRpc (RequireOwnership = false)]
    public void EnterScene(string sceneName, NetworkConnection conn = null)
    {
        if (conn == null || !conn.IsHost) { return; }
        
        EnterSceneForAll();

        if (SteamLobby.Instance.players.Count > 1) {
            SceneLoadData sld2 = new SceneLoadData(sceneName);
            sld2.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld2);
            return;
        }

        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainMenu");

        SceneLookupData slud = new SceneLookupData()
        { 
            Name = sceneName 
        }; 

        SceneLoadData sld = new SceneLoadData(sceneName); 
        sld.PreferredActiveScene = slud;
        sld.ReplaceScenes = ReplaceOption.All;
        
        InstanceFinder.SceneManager.LoadConnectionScenes(LobbyController.Instance.LocalPlayerController.Owner, sld);
    }

    [ObserversRpc]
    private void EnterSceneForAll()
    {
        testMap = true;
    }

}
