using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

public class KillCam : MonoBehaviour
{
    public InputAction move, run, interact, lookX, lookY;

    public Transform enemy;
    public Transform ragdoll;
    public Vector3 firstPosition;

    private bool killcam;
    public bool triggerLookAtBody;

    public bool ragdollCam;
    public bool isDead;

    private float timer;

    Vector3 deltaPosition;
    Vector3 tempPosition;

    GameObject[] players;
    int alivePlayerCount;

    int spectatePlayerId;

    TextMeshProUGUI switchCamText;
    TextMeshProUGUI playerNameText;
    TextMeshProUGUI playerHpText;

    private bool midMatchJoin;
    bool midMatchTrigger;

    bool freeCam;
    bool hideHud;
    bool hasPressedFreecam;
    
    Camera freeCamCamera;
    Vector2 mouseInput;

    private void Awake() {
        freeCamCamera = GetComponent<Camera>();
        freeCamCamera.fieldOfView = Settings.Instance.normalFovValue;
    }

    void Start() {
        switchCamText = GameObject.Find("SwitchCamText").GetComponent<TextMeshProUGUI>();
        playerNameText = GameObject.Find("PlayerNameText").GetComponent<TextMeshProUGUI>();
        playerHpText = GameObject.Find("PlayerHpText").GetComponent<TextMeshProUGUI>();
    }

    void OnEnable() {

        var playerControls = InputManager.inputActions;

        move = playerControls.Player.Move;
        move.Enable();

        interact = playerControls.Player.Interact;
        interact.Enable();

        run = playerControls.Player.Run;
        run.Enable();

        lookY = playerControls.Player.MouseY;
        lookY.Enable();

        lookX = playerControls.Player.MouseX;
        lookX.Enable();

        lookX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        lookY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();
    }
    // Update is called once per frame
    void Update()
    {
        if (PauseManager.Instance.nonSteamworksTransport) return;

        bool isUsingIndependentSensitivity = Settings.Instance.toggleTwoAxis;
        float x = 0;
        float y = 0;
        if (isUsingIndependentSensitivity) {
            x = mouseInput.x * Settings.Instance.horizontalSensitivity * 0.1f * (Settings.Instance.invertMouseX ? -1 : 1) * (PauseManager.Instance.gamepad ? 14 : 1);;
            y = mouseInput.y * Settings.Instance.verticalSensitivity * 0.1f * (Settings.Instance.invertMouseY ? -1 : 1) * (PauseManager.Instance.gamepad ? 14 : 1);
        }
        else {
            x = mouseInput.x * Settings.Instance.mouseSensitivity * 0.1f * (Settings.Instance.invertMouseX ? -1 : 1) * (PauseManager.Instance.gamepad ? 14 : 1);;
            y = mouseInput.y * Settings.Instance.mouseSensitivity * 0.1f * (Settings.Instance.invertMouseY ? -1 : 1) * (PauseManager.Instance.gamepad ? 14 : 1);
        }

        var isRunning = run.ReadValue<float>() > 0.1f;

        var movx = move.ReadValue<Vector2>().x;
        var movy = move.ReadValue<Vector2>().y;
        var movz = 0;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) movz = -1;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) movz = 1;

        timer -= Time.deltaTime;

        if (enemy) deltaPosition = enemy.position - tempPosition;

        if (!midMatchJoin && !midMatchTrigger) {
            
            if (GameObject.FindGameObjectsWithTag("PlayerGraphics").Length > 0) {
                if (LobbyController.Instance.LocalPlayerController.PlayerSpawner.player == null) {
                    ragdoll = GameObject.FindGameObjectsWithTag("PlayerGraphics")[0].transform;
                    Debug.Log("Mid match");
                    midMatchJoin = true;
                }
            }
        }

        if (midMatchJoin) {
            switchCamText.text = "You joined mid match. You will be spawned next round.";
            midMatchTrigger = true;

            if (LobbyController.Instance.LocalPlayerController.PlayerSpawner.player != null) midMatchJoin = false;
        }
        
        if (enemy == null)
        {
            freeCam = false;
            switchCamText.text = "";
            playerHpText.text = "";
            playerNameText.text = "";
            triggerLookAtBody = true;
        }
        else if (enemy.gameObject.activeSelf == false || (enemy.gameObject.name != "Graphics" && enemy.Find("Graphics") == null)) {
            players = GameObject.FindGameObjectsWithTag("PlayerGraphics");
            if (players.Length > 0) enemy = players[0].transform;
            else enemy = null;
        }

        if (freeCam && !PauseManager.Instance.pause) {
            if (transform.parent != null) transform.SetParent(null);
            timer = -1;

            transform.RotateAround(transform.position, Vector3.up, x);
            transform.RotateAround(transform.position, transform.right, -y);

            transform.Translate(transform.right * movx * (isRunning ? 30 : 15) * Time.deltaTime, Space.World);
            transform.Translate(transform.up * movz * (isRunning ? 30 : 15) * Time.deltaTime, Space.World);
            transform.Translate(transform.forward * movy * (isRunning ? 30 : 15) * Time.deltaTime, Space.World);

        }
        
        if (ragdoll)
        {
            if (interact.WasPerformedThisFrame() && !PauseManager.Instance.chatting && !freeCam) { freeCam = true; transform.DOKill(); hasPressedFreecam = true; }

            if (triggerLookAtBody && !freeCam)
            {
                players = GameObject.FindGameObjectsWithTag("PlayerGraphics");
                if (players.Length > 0) enemy = players[0].transform;
                timer = 3;
                var rag = ragdoll;
                ragdoll = FindRecursive("Hips", rag);
                triggerLookAtBody = false;
                
                
            }

            if (timer < 0)
            {
                if (Input.GetKeyDown(KeyCode.H)) hideHud = !hideHud;

                if (isDead) {
                    if (hideHud) {
                        switchCamText.text = "";
                        playerHpText.text = "";
                        playerNameText.text = "";
                    }
                    else if (freeCam) {
                        playerHpText.text = "Free Camera"; playerNameText.text = "";
                        switchCamText.text = $"Mouse Buttons to switch player. {PauseManager.Instance.InteractPromptLetter} to toggle free cam. H to hide HUD";
                    }
                    else {
                        switchCamText.text = $"Mouse Buttons to switch player. {PauseManager.Instance.InteractPromptLetter} to toggle free cam. H to hide HUD";
                        if (enemy.GetComponent<PlayerHealth>() != null)
                            playerHpText.text = $"Health : {Mathf.Ceil((enemy.GetComponent<PlayerHealth>().health/4) * 100)}";
                        else if (enemy.GetComponentInParent<PlayerHealth>() != null)
                            playerHpText.text = $"Health : {Mathf.Ceil((enemy.GetComponentInParent<PlayerHealth>().health/4) * 100)}";

                        if (enemy.GetComponent<PlayerValues>() != null)
                            playerNameText.text = $"Player : {enemy.GetComponent<PlayerValues>().playerClient.PlayerName}";
                        else if (enemy.GetComponentInParent<PlayerValues>() != null)
                            playerNameText.text = $"Player : {enemy.GetComponentInParent<PlayerValues>().playerClient.PlayerName}";
                    }
                }

                players = GameObject.FindGameObjectsWithTag("PlayerGraphics");                

                alivePlayerCount = players.Length;

                if (interact.WasPerformedThisFrame() && !PauseManager.Instance.chatting && freeCam && !hasPressedFreecam) { 
                    freeCam = false; 
                    if (spectatePlayerId==alivePlayerCount-1) spectatePlayerId=0;
                    else spectatePlayerId++;
                    spectatePlayerId = Mathf.Clamp(spectatePlayerId, 0, alivePlayerCount-1);
                    enemy = players[spectatePlayerId].transform;
                }

                if (Input.GetMouseButtonDown(0)) {
                    freeCam = false;
                    if (spectatePlayerId==alivePlayerCount-1) spectatePlayerId=0;
                    else spectatePlayerId++;
                    spectatePlayerId = Mathf.Clamp(spectatePlayerId, 0, alivePlayerCount-1);
                    enemy = players[spectatePlayerId].transform;
                }
                if (Input.GetMouseButtonDown(1)) {
                    freeCam = false;
                    if (spectatePlayerId==0) spectatePlayerId=alivePlayerCount-1;
                    else spectatePlayerId--;
                    spectatePlayerId = Mathf.Clamp(spectatePlayerId, 0, alivePlayerCount-1);
                    enemy = players[spectatePlayerId].transform;
                }
                
                
                if (!freeCam) {
                    transform.DOMove(enemy.position + new Vector3(0, 2.3f, 0) - enemy.forward * (alivePlayerCount > 1 ? 1 : -1) * 2.3f, (timer > -1 ? 1 : 0));
                    transform.DOLookAt(enemy.position + new Vector3(0, 1.7f, 0), (timer > -1 ? 0.4f : 0));
                }


                
            }
            else if (!freeCam) {
                transform.DOLookAt(ragdoll.position, 0);
                transform.position = firstPosition;
                transform.SetParent(null);
            }

            
            
        }

        

        if (Input.GetMouseButtonDown(0) && timer > 0 && timer < 2.7f)
        {
            timer = 0;
        }

        if (enemy && !freeCam) tempPosition = enemy.position;

        hasPressedFreecam = false;

    }

    Transform FindRecursive(string name, Transform root)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        Transform wantedObject = null;
        foreach (var child in children)
        {
            if (child.gameObject.name == name) 
            {
                wantedObject = child;
                break;
            }
        }
        
        return wantedObject;
    }

    void OnDisable(){
        move.Disable();
        interact.Disable();
        run.Disable();
        lookY.Disable();
        lookX.Disable();
        switchCamText.text = "";
        playerHpText.text = "";
        playerNameText.text = "";
    }
}
