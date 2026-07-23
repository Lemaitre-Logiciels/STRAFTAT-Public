using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HeathenEngineering.SteamworksIntegration.UI;

public class PauseManager : MonoBehaviour
{
    public bool pause;
    public bool gamepad;
    public bool chatting;
    public bool startRound;
    public bool rebinding;
    public bool serverStarted;
    public bool inMainMenu;
    public bool inVictoryMenu;
    public bool otherPauseBools;
    public bool gameStarted;
    [SerializeField] private bool canChat;
    [Space]
    [SerializeField] private FriendList friendListScript;
    [Space]
    
    [HideInInspector] public bool steamPlaying;

    private GameObject ChatBox;
    public GameObject sequenceDisplayGameObject;

    public static PauseManager Instance;
    [SerializeField] private InputAction menu, gamepadAny, keyboardAny;
    private PlayerControls _playerInput;

    [SerializeField] private GameObject pauseMenu;
    public GameObject tabScreen;
    [SerializeField] private GameObject firstInterface;
    [SerializeField] private GameObject secondInterface;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject resumeButton3D;
    [SerializeField] private GameObject optionsMenu;
    
    public GameObject minimalistUi;
    public TextMeshProUGUI minimalistHealthText;
    public TextMeshProUGUI minimalFpsText;
    public TextMeshProUGUI minimalPingText;
    [SerializeField] private MoveUIObject firstInterface3D;
    [SerializeField] private GameObject serverDownPopup;
    [SerializeField] private GameObject onePlayerLeftPopup;
    
    [SerializeField] private GameObject infoPopup;
    private List<string> TextToShow = new List<string>();
    public void ShowInfoPopup(string text) {
        if (TextToShow.Count > 0 || infoPopup.activeSelf) { TextToShow.Add(text); return; }
        infoPopup.SetActive(true);
        infoPopup.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    public void ShowNextInfoPopup() {
        if (TextToShow.Count == 0) { infoPopup.SetActive(false); return; }
        infoPopup.GetComponentInChildren<TextMeshProUGUI>().text = TextToShow[0];
        TextToShow.RemoveAt(0);
    }

    public TextMeshProUGUI rightGunAmmo;
    public TextMeshProUGUI leftGunAmmo;

    public TextMeshProUGUI rightGunAmmoReload;
    public TextMeshProUGUI leftGunAmmoReload;

    public TextMeshProUGUI grabPopup;
    public TextMeshProUGUI interactPopup;

    [SerializeField] private Transform posRightDown;
    [SerializeField] private Transform posRightUp;
    [SerializeField] private Transform posLeftDown;
    [SerializeField] private Transform posLeftUp;

    public AudioClip matchChatClip;
    public AudioClip genericMenuClip;
    public AudioClip pressMenuClip;
    public AudioClip releaseMenuClip;
    public AudioClip closeMenuClip;
    public AudioClip[] deathAudioClip;
    public bool nonSteamworksTransport;
    [Space]
    public string selfNameLogColor;
    public string enemyNameLogColor;

    public delegate void StartRoundAction();
    public static event StartRoundAction OnRoundStarted;

    public void InvokeRoundStarted() {
        BetweenRounds = false;
        OnRoundStarted?.Invoke();
    }

    public static bool BetweenRounds = false;
    
    public delegate void BeforeSpawnAction();
    public static event BeforeSpawnAction OnBeforeSpawn;

    public void InvokeBeforeSpawn() {
        BetweenRounds = true;
        OnBeforeSpawn?.Invoke();
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }

        if (optionsMenu != null) {
            optionsMenu.SetActive(true);
            optionsMenu.transform.localScale = Vector3.zero;
        }

        ChatBox = GameObject.Find("ChatInputField2");

        nonSteamworksTransport = (InstanceFinder.TransportManager.Transport != InstanceFinder.NetworkManager.gameObject.GetComponent<FishySteamworks.FishySteamworks>());

        _playerInput = InputManager.inputActions;
        
    }

    private void OnEnable()
    {
        menu.Enable();
        menu.performed += Menu;

        gamepadAny = _playerInput.Player.AnyGamepad;
        gamepadAny.Enable();
        gamepadAny.performed += ChangeControlSchemeGamepad;

        keyboardAny = _playerInput.Player.Any;
        keyboardAny.Enable();
        keyboardAny.performed += ChangeControlSchemeKeyboard;
    }
    private void OnDisable()
    {
        menu.Disable();
        menu.performed -= Menu;

        gamepadAny.Disable();
        gamepadAny.performed -= ChangeControlSchemeGamepad;

        keyboardAny.Disable();
        keyboardAny.performed -= ChangeControlSchemeKeyboard;
    }

    public void WriteLog(string text) {
        if (!InstanceFinder.NetworkManager.IsOffline) {
            MatchLogs.Instance.WriteLog(text);
        }
    }

    public void WriteOfflineLog(string text)
    {
        MatchLogsOffline.Instance.WriteLog(text);
    }

    public void CopyLobbyID()
    {
        GUIUtility.systemCopyBuffer = SteamLobby.Instance.CurrentLobbyID.ToString();
        WriteOfflineLog("Successfully copied lobby id");
    }

    [SerializeField] private TextMeshProUGUI interactPromptText;
    [SerializeField] private TextMeshProUGUI interactPromptTextGamepad;
    public string InteractPromptLetter;

    // Update is called once per frame
    void Update()
    {
        //InteractPromptLetter = (gamepad ? interactPromptTextGamepad.text : interactPromptText.text);

        //Updated to check for null before trying to access text values
        if (gamepad && interactPromptTextGamepad)
        {
            InteractPromptLetter = interactPromptTextGamepad.text;
        }
        else if (interactPromptText)
        {
            InteractPromptLetter = interactPromptText.text;
        }
        else
        {
            //No gamepad or kb prompt texts
            InteractPromptLetter = "?";
        }
        
        HandleServerState();
        HandleServerStateWhenOnePlayerIsLeft();
        HandleInputDetection();

        if (onEndRoundScreen) deadText.text = "";

        //check it exists first
        if (ChatBox) {
            chatting = ChatBox.activeSelf && canChat;
            if (ClientInstance.Instance && ClientInstance.Instance.IsTyping != chatting) { ClientInstance.Instance.SetTyping(chatting); }
        }

        if (!inMainMenu && SceneManager.GetActiveScene().name == "MainMenu") // Return to menu event
        {
            gameStarted = false;
            Debug.Log("Returned to main menu");
            SceneMotor.Instance.testMap = false;
            friendListScript.UpdateDisplay();
            StoppingMapCoroutine();
            ProgressManager.Instance.RunAllPopups();
            Screen.fullScreen = Settings.Instance.isFullscreen;
            if (RoundManager.Instance != null) RoundManager.Instance.StopCoroutine();
            Settings.Instance.UpdateElo();
            MapsManager.Instance.inExplorationMap = false;

            enemyHealthText.text = "";
            deadText.text = "";
            //StopCoroutine(StartRoundDelayCoroutine());
        }

        inMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        if (inMainMenu) {
            if (Application.targetFrameRate != 90) { Application.targetFrameRate = 90; }
        }
        else { 
            if (Application.targetFrameRate != Settings.Instance.targetFps) { Application.targetFrameRate = Settings.Instance.targetFps; }
        }
        
        inVictoryMenu = SceneManager.GetActiveScene().name == "VictoryScene";
        
        if (serverDownPopup) otherPauseBools = serverDownPopup.activeSelf || tabScreen.activeSelf || onePlayerLeftPopup.activeSelf;

        if (SceneManager.GetActiveScene().name == "MainMenu") {
            if (!mainMenu.activeSelf && !optionsMenu.activeSelf) mainMenu.SetActive(true);
            else if (optionsMenu.activeSelf) mainMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            steamPlaying = true;
        }

        //check it exists
        if (tabScreen)
        {
            if (!inMainMenu) tabScreen.SetActive(Input.GetKey(KeyCode.Tab));
            else tabScreen.SetActive(false);
        }

        VoiceChat();

        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        if (!nonSteamworksTransport) pause = pauseMenu.activeSelf;

        mainMenu.SetActive(false);
        Crosshair.Instance.image.enabled = !pauseMenu.activeSelf;

        if (pause == true || otherPauseBools)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (gamepad)  {
            if (EventSystem.current.currentSelectedGameObject == null) {
                if (FindObjectsOfType<Button>().Length > 0) EventSystem.current.SetSelectedGameObject(FindObjectsOfType<Button>()[0].gameObject);
            }
            else if (!EventSystem.current.currentSelectedGameObject.activeInHierarchy) {
                if (FindObjectsOfType<Button>().Length > 0) 
                {
                    EventSystem.current.SetSelectedGameObject(FindObjectsOfType<Button>()[0].gameObject);
                    //Debug.Log(EventSystem.current.currentSelectedGameObject.activeSelf);
                }
            }
        }
    }

    void HandleInputDetection()
    {/*
        Debug.Log(Keyboard.current.anyKey.wasPressedThisFrame);

        if (Gamepad.current != null)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                gamepad = false;
            }
        }
        else gamepad = false;*/
    }

    void ChangeControlSchemeGamepad(InputAction.CallbackContext ctx)
    {
        if (Gamepad.current != null)
        {
            if (MenuController.Instance != null)
            {
                if (MenuController.Instance.startMenu.activeSelf) 
                {
                    MenuController.Instance.OpenGame();
                }
            } 

            gamepad = true;
            if (EventSystem.current.currentSelectedGameObject == null) {
                if (FindObjectsOfType<Button>().Length > 0) EventSystem.current.SetSelectedGameObject(FindObjectsOfType<Button>()[0].gameObject);
            }
            else if (!EventSystem.current.currentSelectedGameObject.activeSelf) {
                if (FindObjectsOfType<Button>().Length > 0) EventSystem.current.SetSelectedGameObject(FindObjectsOfType<Button>()[0].gameObject);
            }
        }
    }

    void ChangeControlSchemeKeyboard(InputAction.CallbackContext ctx)
    {
        if (MenuController.Instance != null)
        {
            if (MenuController.Instance.startMenu.activeSelf) 
            {
                MenuController.Instance.OpenGame();
            }
        } 

        gamepad = false;
    }

    public void ChangeSelectedItem(GameObject obj)
    {
        if (!gamepad) return;

        EventSystem.current.firstSelectedGameObject = obj;
        EventSystem.current.SetSelectedGameObject(obj);
    }

    private void Menu(InputAction.CallbackContext ctx)
    {
        if (nonSteamworksTransport) {
            pause = !pause;
            return;
        }
        
        if (optionsMenu.activeSelf) 
            optionsMenu.SetActive(false);
            

        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        pauseMenu.SetActive(!pauseMenu.activeSelf);
        if (pauseMenu.activeSelf) ChangeSelectedItem(resumeButton3D);
        firstInterface3D.ChooseState(true);
        firstInterface.SetActive(true);
        secondInterface.SetActive(false);

        if (pause) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartGameSteam()
    {
        gameStarted = true;
        SceneMotor.Instance.ServerStartGameScene();
    }

    public void SetActiveOpposite(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    public void SetActiveOppositeScale(Transform obj)
    {
        obj.localScale = (obj.localScale.x == 0 ? Vector3.one : Vector3.zero);
    }

    public void QuitGame()
    {
        //System.Diagnostics.Process.GetCurrentProcess().Kill();
        Application.Quit();
    }

    public void MoveAmmoDisplay(bool up, bool right)
    {
        if (right)
            rightGunAmmo.transform.DOMove((up ? new Vector3(rightGunAmmo.transform.position.x, posRightUp.position.y, rightGunAmmo.transform.position.z) : new Vector3(rightGunAmmo.transform.position.x, posRightDown.position.y, rightGunAmmo.transform.position.z)), 0.3f).SetEase(Ease.OutSine);
        else 
            leftGunAmmo.transform.DOMove((up ? new Vector3(leftGunAmmo.transform.position.x, posLeftUp.position.y, leftGunAmmo.transform.position.z) : new Vector3(leftGunAmmo.transform.position.x, posLeftDown.position.y, leftGunAmmo.transform.position.z)), 0.3f).SetEase(Ease.OutSine);
    }

    public void ChangeAmmoText(string text, string reloadText, bool right)
    {
        if (right) {
            rightGunAmmo.text = reloadText + text;
        }
        else {
            leftGunAmmo.text = reloadText + text;
        }

    }

    public void PlayMenuClip(AudioClip clip)
    {
        SoundManager.Instance.PlaySound(clip);
    }

    private float serverTimer = -2;

    private void HandleServerState()
    {
        if (InstanceFinder.ClientManager && !InstanceFinder.ClientManager.Started && serverStarted) 
        {
            serverStarted = false;
            serverTimer = -2;
            DisplayServerDownPopup();
        }
    }

    private void HandleServerStateWhenOnePlayerIsLeft()
    {
        if (inMainMenu) return;
        if (SceneMotor.Instance != null)
            if (SceneMotor.Instance.testMap) return;

        //No lobby in cases where map scene is ran directly
        if (!SteamLobby.Instance)
            return;

        // Rework to support more than 2 players
        if (InstanceFinder.NetworkManager.IsServer && SteamLobby.Instance.players.Count == 1 && !Application.isEditor && serverStarted) 
        {
            serverStarted = false;
            SteamLobby.Instance.LeaveMatch();
            WriteOfflineLog("Player 2 left your lobby.");
            //onePlayerLeftPopup.SetActive(true);
        }
    }

    [Space]
    [SerializeField] private TextMeshProUGUI startRoundText;
    [SerializeField] private TextMeshProUGUI startRoundTextTitle;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private AudioClip startMatchClip;
    [SerializeField] private GameObject crosshair;
    [Space]

    [Header("Start Round Screens")]

    [SerializeField] private TextMeshProUGUI setText;
    [SerializeField] private TextMeshProUGUI mapText;
    [Space]
    [SerializeField] private Transform container;
    [SerializeField] private Transform startRoundBackDrop;
    [SerializeField] private Transform startRoundImageOne;
    [SerializeField] private Transform startRoundImageTwo;
    [SerializeField] private Transform startRoundImageThree;
    [Space]
    [SerializeField] private Transform startRoundImageOneRestPos;
    [SerializeField] private Transform startRoundImageOneActivePos;
    [Space]
    [SerializeField] private Transform startRoundImageTwoRestPos;
    [SerializeField] private Transform startRoundImageTwoActivePos;
    [Space]
    [SerializeField] private Transform startRoundImageThreeRestPos;
    [SerializeField] private Transform startRoundImageThreeActivePos;
    public bool onStartRoundScreen;
    public bool onEndRoundScreen;

    private Coroutine RoundDelayCoroutine;
    
    private void StoppingMapCoroutine() {
        if (RoundDelayCoroutine == null) return;
        StopCoroutine(RoundDelayCoroutine);

        startRoundBackDrop.GetComponent<Image>().DOColor(new Color(startRoundBackDrop.GetComponent<Image>().color.r, startRoundBackDrop.GetComponent<Image>().color.g, startRoundBackDrop.GetComponent<Image>().color.b, 0f), 0);

        startRoundImageOne.DOMove(startRoundImageOneRestPos.position, 0).SetEase(Ease.OutSine);
        startRoundImageTwo.DOMove(startRoundImageTwoRestPos.position, 0).SetEase(Ease.OutSine);
        startRoundImageThree.DOMove(startRoundImageThreeRestPos.position, 0).SetEase(Ease.OutSine);

        startRoundText.text = "";
        startRoundTextTitle.text = "";
        
        container.localScale = Vector3.zero;
        StartCoroutine(ChangeBoolStartRound());
        canShow = true;
        startRound = false;

        RoundDelayCoroutine = null;
    }

    public void StartRoundDelay(float timeTillMovementStarts) {
        if (!canShow || inMainMenu || inVictoryMenu) {
            Debug.Log($"Cannot start round delay animation, canShow: {canShow}, inMainMenu: {inMainMenu}, inVictoryMenu: {inVictoryMenu}");
            return; 
        }
        RoundDelayCoroutine = StartCoroutine(StartRoundDelayCoroutine(timeTillMovementStarts));
    }

    bool canShow = true;

    IEnumerator StartRoundDelayCoroutine(float timeTillMovementStarts) {
        yield return null; // Wait for the next frame wihtotu calling WaitForSeconds(0f) :P

        canShow = false;
        onStartRoundScreen = false;
        
        float getReadyGoTime = 1.2f;
        float firstRoundAnimTime = 1.8f;

        float totalAnimationTime = getReadyGoTime;
        

        int take = ScoreManager.Instance ? ScoreManager.Instance.TakeIndex : 0;
        int visualTake = take + 1;
        
        if (take == 0) { totalAnimationTime += firstRoundAnimTime; }
        
        float initialWait = timeTillMovementStarts - totalAnimationTime;

        if (initialWait < 0f) {
            Debug.Log("Not enough time to play the the full animation!!");
            initialWait = 0f;
        }

        yield return new WaitForSeconds(initialWait);

        if (take == 0) {
            container.localScale = Vector3.one;
            setText.text = "round " + SceneMotor.Instance.sceneIndex;
            mapText.text = SceneManager.GetActiveScene().name;
            startRoundBackDrop.GetComponent<Image>().color = new Color(startRoundBackDrop.GetComponent<Image>().color.r,
                startRoundBackDrop.GetComponent<Image>().color.g, startRoundBackDrop.GetComponent<Image>().color.b,
                0.92f);

            SoundManager.Instance.PlaySound(RoundManager.Instance.swooshClip[0]);
            startRoundImageOne.DOMove(startRoundImageOneActivePos.position, 0.4f).SetEase(Ease.OutSine);
            startRoundImageTwo.DOMove(startRoundImageTwoActivePos.position, 0.4f).SetEase(Ease.OutSine);
            startRoundImageThree.DOMove(startRoundImageThreeActivePos.position, 0.4f).SetEase(Ease.OutSine);

            yield return new WaitForSeconds(1.8f);

            startRoundBackDrop.GetComponent<Image>()
                .DOColor(
                    new Color(startRoundBackDrop.GetComponent<Image>().color.r,
                        startRoundBackDrop.GetComponent<Image>().color.g,
                        startRoundBackDrop.GetComponent<Image>().color.b, 0f), 0.4f);

            startRoundImageOne.DOMove(startRoundImageOneRestPos.position, 0.4f).SetEase(Ease.OutSine);
            startRoundImageTwo.DOMove(startRoundImageTwoRestPos.position, 0.4f).SetEase(Ease.OutSine);
            startRoundImageThree.DOMove(startRoundImageThreeRestPos.position, 0.4f).SetEase(Ease.OutSine);
            SoundManager.Instance.PlaySound(RoundManager.Instance.swooshClip[1]);
        }

        InvokeRoundStarted();

        yield return new WaitForSeconds(0.4f);
        startRoundTextTitle.text = "TAKE " + visualTake;
        SoundManager.Instance.PlaySound(startMatchClip);
        crosshair.SetActive(false);
        startRoundText.text = "GET";
        yield return new WaitForSeconds(0.4f);
        startRoundText.text = "READY";
        yield return new WaitForSeconds(0.4f);
        startRoundText.text = "GO!";
        crosshair.SetActive(true);
        onStartRoundScreen = true;
        yield return new WaitForSeconds(0.4f);
        startRoundText.text = "";
        startRoundTextTitle.text = "";
        container.localScale = Vector3.zero;
        StartCoroutine(ChangeBoolStartRound());
        canShow = true;
    }


    IEnumerator ChangeBoolStartRound()
    {
        yield return new WaitForSeconds(0.4f);
        onStartRoundScreen = false;
    }

    public void ShowEnemyHealth(float h, PlayerHealth ph)
    {
        StartCoroutine(ShowEnemyHealthCoroutine(h, ph));
    }

    IEnumerator ShowEnemyHealthCoroutine(float h, PlayerHealth ph)
    {
        yield return new WaitForSeconds(0.2f);
        if (ph.health <= 0){
            if (!GameManager.Instance.roundWasWon) enemyHealthText.text = "ENEMY HEALTH : " + (Mathf.Ceil((h/4)*100)).ToString() + " HP";
            if (!GameManager.Instance.roundWasWon) deadText.text = "DEAD";
        }
        
        yield return new WaitForSeconds(2.7f);
        enemyHealthText.text = "";
        deadText.text = "";
    }

    [SerializeField] private float time;

    public void SetActiveAfterSecond(GameObject obj)
    {
        StartCoroutine(SetActiveAfterSecondCoroutine(obj));
    }

    IEnumerator SetActiveAfterSecondCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(time);

        obj.SetActive(true);
    }

    [SerializeField] private float time2 = 1;

    public void SetActiveOppositeAfterSecond(GameObject obj)
    {
        StartCoroutine(SetActiveOppositeAfterSecondCoroutine(obj));
    }

    IEnumerator SetActiveOppositeAfterSecondCoroutine(GameObject obj)
    {
        

        obj.SetActive(!obj.activeSelf);
        yield return new WaitForSeconds(time2);
        obj.SetActive(!obj.activeSelf);
    }

    public void DisplayServerDownPopup()
    {
        serverDownPopup.SetActive(true);
    }

    [Space]
    [SerializeField] private GameObject voiceChatText;
    public bool isRecording;

    void VoiceChat()
    {
        if (chatting) return;

        //check it exists
        if (!voiceChatText) return;

        if (isRecording) 
        {
            voiceChatText.SetActive(true);
        }
        else {
            voiceChatText.SetActive(false);
        }
    }

    [Space]

    [SerializeField] private Transform hiddenInvitePos;
    [SerializeField] private Transform shownInvitePos;

    public void ShowInviteViewport(bool show, Transform viewport)
    {
        if (show) viewport.DOMove(shownInvitePos.position, 0.3f);
        else viewport.DOMove(hiddenInvitePos.position, 0.3f);
    }


    public void PlaySoundWithGamepad()
    {
        if (!gamepad) return;

        SoundManager.Instance.PlaySound(genericMenuClip);
    }


}


