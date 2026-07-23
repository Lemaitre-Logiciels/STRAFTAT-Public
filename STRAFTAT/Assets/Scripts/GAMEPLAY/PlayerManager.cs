using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using UnityEngine;
using Random=UnityEngine.Random;
using FishNet.Managing.Scened;
using UnityEngine.Serialization;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class PlayerManager : NetworkBehaviour
{
    [Tooltip("Character prefab to spawn.")]
    [FormerlySerializedAs("_characterPrefab")] [SerializeField] private GameObject characterPrefab;
    
    private GameObject SpawnedObject;
    private SpawnPoint[] CurrentSpawnPoints = Array.Empty<SpawnPoint>();
    private SpawnPoint[] SpawnPoint1v1 = Array.Empty<SpawnPoint>();
    private SpawnPoint[] SpawnPoint4v4 = Array.Empty<SpawnPoint>();

    private ClientInstance ClientScript;
    [HideInInspector] public FirstPersonController player;

    private MapsManager mapsManager;
    
    private bool StartTrigger = true;
    
    private void Awake() {
        ClientScript = GetComponent<ClientInstance>();
        PopulateSpawnPoints();
    }
    private void OnEnable() {
        InstanceFinder.SceneManager.OnLoadStart += OnLoadSceneStart;
        InstanceFinder.SceneManager.OnLoadEnd += OnLoadSceneEnd;
    }
    void Start() {
        mapsManager = MapsManager.Instance;
    }
    void Update() {
        if (!IsOwner) { return; }

        if (SpawnedObject && PauseManager.Instance.inMainMenu) {
            SpawnedObject.SetActive(false);
            DespawnServer(SpawnedObject);
        }
        
        if (waitForRoundStartCoroutine != null && PauseManager.Instance.inMainMenu) {
            StopCoroutine(waitForRoundStartCoroutine);
            GameManager.Instance.hasSetStartTime = false;
            waitForRoundStartCoroutine = null;
        }
    }

    public Coroutine waitForRoundStartCoroutine;

    public void WaitForRoundStartCoroutineStart() {
        // Prevent multiple coroutines from being started
        if (waitForRoundStartCoroutine != null) {
            Debug.LogWarning("WaitForRoundStartCoroutine already running, stopping it before starting a new one.");
            StopCoroutine(waitForRoundStartCoroutine);
        }
        waitForRoundStartCoroutine = StartCoroutine(WaitForRoundStart());
    }

    // fail safe so it will never fully break
    private const float ConnectionCheckTimeout = 7.5f;
    float CheckTimer = 0.0f; // Is reset to 0 when a new player readies up
    
    // The worst thing that happens if any of these while loops run forever is that the player can't move, which is fine because they'll get killed :P
    public IEnumerator WaitForRoundStart() { 
        // Skip the delay when in exploration mode or victory menu
        if (ClientScript.nonSteamworksTransport || SceneMotor.Instance.testMap || PauseManager.Instance.inVictoryMenu || PauseManager.Instance.inMainMenu) {
            SetPlayerMove(true);
            PauseManager.Instance.InvokeRoundStarted();
            Debug.Log($"Skipping round start... nonSteamworksTransport: {ClientScript.nonSteamworksTransport}, testMap: {mapsManager.inExplorationMap}, inVictoryMenu: {PauseManager.Instance.inVictoryMenu}, inMainMenu: {PauseManager.Instance.inMainMenu}");
            waitForRoundStartCoroutine = null; // Reset the coroutine reference
            yield break; // No need to wait for round start, just continue
        }

        SetPlayerMove(false);
        
        yield return new WaitForSeconds(0.5f); // This is needed so that the player will be in the scene and can stabilize the network connection 
        
        IncrementReadyPlayers(); // Runs first to ensure the player is in the alivePlayers list

        if (IsServer) {
            CheckTimer = -10.0f; // Give more time for the first client to connect
            
            while (!GameManager.Instance.ConnectionsToStartGame.SetEquals(GameManager.Instance.NetworkConnectionsReady)) {
                CheckTimer += Time.deltaTime;
                if (CheckTimer >= ConnectionCheckTimeout) {
                    Debug.LogWarning("Connection check timeout reached, proceeding with the round start.");
                    break;
                }
                yield return null;
                GameManager.Instance.ConnectionsToStartGame.RemoveWhere(c => !c.IsActive);
                GameManager.Instance.NetworkConnectionsReady.RemoveWhere(c => !GameManager.Instance.ConnectionsToStartGame.Contains(c));
            }
        
            GameManager.Instance.ConnectionsToStartGame.Clear(); // Clear the connections needed to start game list for the next round
            GameManager.Instance.NetworkConnectionsReady.Clear(); // Clear the ready players list for the next round
            
            float timeTillMovementStarts = ScoreManager.Instance.TakeIndex == 0 ? 3.0f : 1.2f;
            // add buffer time to take away from if loading takes too long
            // Effectively this can be seen as *hey we support 0.25 seconds of ping before we start having issues with the animation* :P
            // Possibly we could rework this to check the highest ping of the players and adjust the time accordingly, but this is fine for now
            timeTillMovementStarts += 0.25f;
            
            // This is an observerRPC that runs locally and sets the start tick for all clients
            // I picked this over a syncvar because there's a chance a client is really far behind
            // and the syncvar would update and then get reset before the client can read it
            GameManager.Instance.SetStartTime(timeTillMovementStarts);
        } else {
            // Wait for the server to set the start tick
            while (!GameManager.Instance.hasSetStartTime) { yield return null; }
        }
        GameManager.Instance.hasSetStartTime = false; // Reset the flag for the next round
        
        PauseManager.Instance.startRound = true; // Needed for some FPSController logic
        PauseManager.Instance.StartRoundDelay(GameManager.Instance.timeTillStart); // Play animation
        
        double elapsedTime = 0f;
        while (elapsedTime < GameManager.Instance.timeTillStart) {
            elapsedTime += Time.deltaTime;
            yield return null;
        } // Wait until the start time is reached

        // You have to set these two bools to allow the player to move :P
        SetPlayerMove(true);
        PauseManager.Instance.startRound = false;
        
        // This enables the tazer effect to show up
        player.setupScript.EnableTaserEffectServer();
        
        // Debug Purposes
        DateTime now = DateTime.Now;
        string formatted = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Debug.Log("CAN MOVE at Current Time: " + formatted);
        
        waitForRoundStartCoroutine = null; // Reset the coroutine reference
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementReadyPlayers(NetworkConnection conn = null) {
        GameManager.Instance.NetworkConnectionsReady.Add(conn);
        CheckTimer = 0.0f;
    }
    
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SetPlayerMove(bool state) { player.canMove = state; }

    public void OnLoadSceneStart(SceneLoadStartEventArgs args) {
        if (SpawnedObject) { Despawn(SpawnedObject); }
    }
    
    public void PopulateSpawnPoints() {
        GameObject SpawnPoint1v1GameObject = GameObject.FindGameObjectWithTag("Spawnpoints");
        SpawnPoint1v1 = SpawnPoint1v1GameObject ? SpawnPoint1v1GameObject.GetComponentsInChildren<SpawnPoint>() : Array.Empty<SpawnPoint>();
        
        GameObject SpawnPoint4v4GameObject = GameObject.FindGameObjectWithTag("Spawnpoints4Player");
        SpawnPoint4v4 = SpawnPoint4v4GameObject ? SpawnPoint4v4GameObject.GetComponentsInChildren<SpawnPoint>() : Array.Empty<SpawnPoint>();
        
        SetActiveSpawnPoints();
    }
    
    public void SetActiveSpawnPoints() {
        if (ClientScript.nonSteamworksTransport) {
            CurrentSpawnPoints = SpawnPoint1v1;
            return;
        }

        if (SteamLobby.Instance.players.Count > 2) {
            if (SpawnPoint4v4.Length != 0) {
                CurrentSpawnPoints = SpawnPoint4v4;
                return;
            }
            Debug.LogError("No spawn points found for 4+ players! Falling back to 1v1 spawn points.");
        }
        
        CurrentSpawnPoints = SpawnPoint1v1;
    }

    public void OnLoadSceneEnd(SceneLoadEndEventArgs args) {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu" && !PauseManager.Instance.inMainMenu) {
            ClientScript.ServerSetPlayerReadyState(false);
            Debug.Log("main menu player manager");
        }

        if (SpawnedObject != null) { return; }
        PopulateSpawnPoints();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "EmptyScene") {
            PauseManager.Instance.InvokeBeforeSpawn();
            NewSceneSpawn();
        }
    }

    // ------------------ Spawn stuff for the UNITY mode ------------------
    public void TryRespawn() {
        if (UnitySceneManager.GetActiveScene().name == "MainMenu") { return; }
        if (!IsOwner) { return; }

        // Debug Moving Platforms
        // Is this needed?
        AnimResetOnRound[] platforms = FindObjectsOfType<AnimResetOnRound>();
        foreach (AnimResetOnRound plat in platforms) { plat.StartNewRound(); }
        
        CmdRespawn();
        PauseManager.Instance.startRound = false;
    }
    private Transform ReturnSpawnPoint() {
        if (CurrentSpawnPoints.Length == 0) { return null; }
        
        Shuffle(CurrentSpawnPoints); // Shuffle cuz thats what the old code did, dunno why it didn't just pick a random one -_-
        foreach (SpawnPoint spawnPoint in CurrentSpawnPoints) {
            if (!spawnPoint.gameObject.activeInHierarchy) { continue; } // Skip inactive spawn points, even tho the actual spawn code doesn't care about this??
            
            // Changed this to NonAlloc to avoid allocating a new array every time we check for a spawn point, we only need the size of the array.
            int size = Physics.OverlapSphereNonAlloc(spawnPoint.transform.position, spawnPoint.Radius, null, 5);
            if (size == 0) { return spawnPoint.transform; }
        }

        //If here no valid spawn points found. Pick first one.
        return CurrentSpawnPoints[0].transform;
    }
    private static void Shuffle(SpawnPoint[] spawnPoints) {
        for (int t = 0; t < spawnPoints.Length; t++ ) {
            SpawnPoint tmp = spawnPoints[t];
            int r = Random.Range(t, spawnPoints.Length);
            spawnPoints[t] = spawnPoints[r];
            spawnPoints[r] = tmp;
        }
    }
    [ServerRpc]
    private void CmdRespawn() {
        if (SpawnedObject != null) {
            UnsubscribeFromInput();
            Despawn(SpawnedObject);
        }

        Transform spawn = ReturnSpawnPoint();
        if (spawn) {
            SpawnPlayer(CosmeticsManager.Instance.currentsuitIndex, CosmeticsManager.Instance.currentcigIndex, spawn.position, Quaternion.Euler(0f, spawn.eulerAngles.y, 0f));
        }
        else {
            Debug.LogError("All spawns are occupied.");
            // Hey there should probably be some code here to handle this lol
        }
    }
    
    // ------------------ Spawn stuff for new scenes ------------------
    public void NewSceneSpawn() {
        if (!IsOwner) { return; }
        
        SaveLoadSystem.Instance.Save(); // Questionable place to save?
        
        if (UnitySceneManager.GetActiveScene().name == "MainMenu") { return; }
        
        StartTrigger = true;
        if (SceneMotor.Instance) {
            if (!SceneMotor.Instance.testMap) Settings.Instance.IncreaseRoundsPlayed();
        }
        
        StartCoroutine(SceneMotor.Instance.StartGameClients());
        CmdNewSceneSpawn(CosmeticsManager.Instance.currentsuitIndex, CosmeticsManager.Instance.currentcigIndex);
    }
    [ServerRpc]
    private void CmdNewSceneSpawn(int suitIndex, int cigIndex) {
        if (PauseManager.Instance && PauseManager.Instance.inMainMenu) { return; }
        
        if (SpawnedObject) {
            UnsubscribeFromInput();
            SpawnedObject.SetActive(false);
            Despawn(SpawnedObject);
        }
        
        SpawnPlayer(suitIndex, cigIndex);
    }
    
    // ------------------ Spawn stuff for new rounds ------------------
    public void RoundSpawn() {
        if (UnitySceneManager.GetActiveScene().name == "MainMenu") { return; }
        if (!IsOwner) return;
        StartTrigger = true;
        CmdNewRoundSpawn(CosmeticsManager.Instance.currentsuitIndex, CosmeticsManager.Instance.currentcigIndex);
    }
    [ServerRpc]
    private void CmdNewRoundSpawn(int suitIndex, int cigIndex) {
        if (SpawnedObject != null) {
            UnsubscribeFromInput();
            SpawnedObject.SetActive(false);
            Despawn(SpawnedObject);
        }
        
        SpawnPlayer(suitIndex, cigIndex);
    }

    private void SpawnPlayer(int suitIndex, int cigIndex) {
        SetActiveSpawnPoints();
        
        SpawnPoint spawnPoint;
        if (SteamLobby.Instance.players.Count <= 2) {
            int offset = ClientScript.PlayerId == 0 ? 0 : 1; // Host is always 0, so we can use that to offset the spawn point index
            spawnPoint = CurrentSpawnPoints[(ScoreManager.Instance.TakeIndex + offset) % CurrentSpawnPoints.Length];
        }
        else {
            int[] connectedTeams = GameManager.Instance.GetConnectedTeams();
            if (connectedTeams.Length == 2 && ScoreManager.Instance.GetPlayerIdsForTeam(connectedTeams[0]).Count == 2 && ScoreManager.Instance.GetPlayerIdsForTeam(connectedTeams[1]).Count == 2) {
                // Kevs 2v2 logic, for better 2v2 gameplay
                /*
                # Take One
                `player one of team one`, gets the `first` spawn
                `player one of team two`, gets the `second` spawn
                `player two of team one`,  gets the `third` spawn
                `player two of team two`, gets the `fourth` spawn

                # Take Two
                `player one of team one`, gets the `second` spawn
                `player one of team two`, gets the `third` spawn
                `player two of team one`,  gets the `fourth` spawn
                `player two of team two`, gets the `first` spawn

                # Take Three
                `player one of team one`, gets the `third` spawn
                `player one of team two`, gets the `fourth` spawn
                `player two of team one`,  gets the `first` spawn
                `player two of team two`, gets the `second` spawn

                # Take Four
                `player one of team one`, gets the `fourth` spawn
                `player one of team two`, gets the `first` spawn
                `player two of team one`,  gets the `second` spawn
                `player two of team two`, gets the `third` spawn

                # Take Five
                `player one of team one`, gets the `first` spawn
                `player one of team two`, gets the `second` spawn
                `player two of team one`,  gets the `third` spawn
                `player two of team two`, gets the `fourth` spawn
                */
                int teamID = ScoreManager.Instance.GetTeamId(ClientScript.PlayerId);
                Array.Sort(connectedTeams);
                int teamIndex = Array.IndexOf(connectedTeams, teamID);
                List<int> teamPlayers = ScoreManager.Instance.TeamIdToPlayerIds[teamID];
                int indexOfPlayer = teamPlayers.IndexOf(ClientScript.PlayerId);
                int spawnPointIndex = (ScoreManager.Instance.TakeIndex + teamIndex + (indexOfPlayer * 2)) % CurrentSpawnPoints.Length;
                spawnPoint = CurrentSpawnPoints[spawnPointIndex];
            } else {
                spawnPoint = CurrentSpawnPoints[(ScoreManager.Instance.TakeIndex + ClientScript.PlayerId) % CurrentSpawnPoints.Length];
            }
        }

        SpawnPlayer(suitIndex, cigIndex, spawnPoint.transform.position, Quaternion.Euler(0f, spawnPoint.transform.eulerAngles.y, 0f));
    }
    private void SpawnPlayer(int suitIndex, int cigIndex, Vector3 position, Quaternion rotation) {
        //BeforeSpawn(ClientScript.Owner);

        if (GameManager.Instance != null) {
            GameManager.Instance.alivePlayers.Add(ClientScript.PlayerId); // Add the player to the alive players list, so they can be
        }
        
        GameObject playerGameObject = Instantiate(characterPrefab, position, rotation);
        SpawnedObject = playerGameObject;
        player = playerGameObject.GetComponent<FirstPersonController>();
        playerGameObject.GetComponentInChildren<VisualInfo>().name.text = ClientScript.PlayerName;
        playerGameObject.GetComponent<PlayerValues>().playerClient = ClientScript;
        PlayerSetup playerSetup = playerGameObject.GetComponent<PlayerSetup>();
        playerSetup.mat = suitIndex;
        playerSetup.cig = cigIndex;
        Spawn(playerGameObject, Owner);
    }
    
    private void UnsubscribeFromInput() {
        player.move.Disable();
        player.moveUp.Disable();
        player.jump.Disable();
        player.run.Disable();
        player.lookY.Disable();
        player.lookX.Disable();
        player.crouch.Disable();
        player.leanLeft.Disable();
        player.leanRight.Disable();

        player.jump.performed -= player.Jump;
        player.crouch.performed -= player.Slide;
        player.crouch.started -= player.SetCrouch;
        player.crouch.canceled -= player.SetCrouch;
        player.crouch.canceled -= player.SlideEnd;
    }
    
    [ServerRpc]
    private void DespawnServer(GameObject obj) { if (obj) { Despawn(obj); } }
}
