using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.PostProcessing;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisableForEnemy;
    [SerializeField] GameObject[] gameObjectToDisableForEnemy;
    [SerializeField] Behaviour[] componentsToDisableForMe;
    [SerializeField] AudioSource[] audioMuteForMe;
    [SerializeField] GameObject[] gameObjectsToDisableForMe;
    [SerializeField] GameObject[] fpArms;
    [SerializeField] GameObject stunVFX;
    [SerializeField] GameObject colliderParent;
    [SerializeField] GameObject graphics;
    [SerializeField] GameObject suppression;
    [SerializeField] Camera[] cameras;
    [SerializeField] LayerMask lowMask;
    [SerializeField] LayerMask highMask;
    [SyncVar] public int mat;
    [SyncVar] public int cig;

    [Space]
    [SerializeField] private GameObject startLights;
    [SerializeField] private GameObject hudObject;

    public Camera sceneCamera;
    float timer = 1;
    bool mainCameraActive;

    IEnumerator GetPlayersName()
    {
        yield return new WaitForSeconds(0.5f);

        
    }

    public override void OnStartClient() {
        base.OnStartClient();
        
        if (!IsOwner) {
            for (int i=0; i < componentsToDisableForEnemy.Length; i++)
            {
                componentsToDisableForEnemy[i].enabled = false;
            }
            for (int i=0; i < gameObjectToDisableForEnemy.Length; i++)
            {
                gameObjectToDisableForEnemy[i].SetActive(false);
            }

            
            this.enabled = false;
        } 
        else {
            for (int i=0; i < gameObjectsToDisableForMe.Length; i++)
            {
                gameObjectsToDisableForMe[i].gameObject.SetActive(false);
            }
            for (int i=0; i < componentsToDisableForMe.Length; i++)
            {
                componentsToDisableForMe[i].enabled = false;
            }
            for (int i=0; i < audioMuteForMe.Length; i++)
            {
                audioMuteForMe[i].mute = true;
            }

            for (int i=0; i < cameras.Length; i++)
            {
                cameras[i].enabled = true;
            }

            if (PauseManager.Instance.minimalistUi != null) HideHUD(Settings.Instance.minimalistUi);

            if (Settings.Instance.qualitySetting < 2)
            {
                if (Settings.Instance.qualitySetting == 1){
                    cameras[0].cullingMask = lowMask;
                    cameras[1].enabled = false;
                }
                if (Settings.Instance.qualitySetting == 0){
                    cameras[0].cullingMask = lowMask;
                    cameras[1].enabled = false;
                    cameras[2].enabled = false;
                    HideHUD(true);
                }
            }
            else{
                cameras[0].cullingMask = highMask;
            }

            if (Settings.Instance.motionBlur) ChangeMotionBlur(true);

            CmdChangeDress(gameObject, CosmeticsManager.Instance.currenthat, transform.forward);
            hatToWearPosition.gameObject.SetActive(false);

            gameObject.layer = 6;
            SetGameLayerRecursive(graphics, 16);

            foreach (var collider in suppression.GetComponentsInChildren<Collider>()) {
                collider.enabled = false;
            }

            if (GameObject.Find("Main Camera") != null) {
                GameObject.Find("Main Camera").GetComponent<KillCam>().ragdoll = null;
                GameObject.Find("Main Camera").GetComponent<KillCam>().enemy = null;
            }

            FirstPersonController player = GetComponent<FirstPersonController>();
            Settings.Instance.localPlayer = player;
            Crosshair.Instance.player = player;
            PauseManager.Instance.ChangeAmmoText("---", "", true);
            PauseManager.Instance.ChangeAmmoText("---", "", false);

            PlayerManager playerManager = GetComponent<PlayerValues>().playerClient.GetComponent<PlayerManager>();
            playerManager.player = player;
            playerManager.WaitForRoundStartCoroutineStart();

            if (SceneMotor.Instance != null)
                if (SceneMotor.Instance.testMap && GetComponentInChildren<HUDTween>() != null) GetComponentInChildren<HUDTween>().MoveUp();
        }

        graphics.transform.localPosition = new Vector3(0, -0.2f, 0);
    }

    public void ChangeMotionBlur(bool state)
    {
        if (GetComponent<FirstPersonController>().volume.profile.TryGetSettings(out MotionBlur blur))
        {
            blur.enabled.value = state;
        }
    }

    [ServerRpc]
    public void EnableTaserEffectServer() { EnableTaserEffectClient(); }

    [ObserversRpc(RunLocally = true)]
    private void EnableTaserEffectClient() { wasMoving = true; }
    

    [ServerRpc (RunLocally =  true)]
    public void ChangeSkinWidth(float value)
    {
        ChangeSkinWidthObservers(value );
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    public void ChangeSkinWidthObservers(float value)
    {
        graphics.transform.localPosition = new Vector3(0, -value, 0);
    }

    public void HideHUD(bool hide)
    {
        //if (Settings.Instance.qualitySetting == 0) return;
        hudObject.SetActive(!hide);
        PauseManager.Instance.minimalistUi.SetActive(hide);

        if (hudObject.activeSelf && !PauseManager.Instance.onStartRoundScreen && GetComponent<FirstPersonController>().canMove) GetComponentInChildren<HUDTween>().MoveUp();
    }

    void Awake()
    {
        for (int i=0; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }
        
    }

    FirstPersonController controller;
    [HideInInspector] public bool wasMoving;
    bool canMoveAgain;

    private void Start()
    {
        PauseManager.Instance.serverStarted = true;
        controller = GetComponent<FirstPersonController>();

        
    }

    bool dress;

    private void Update()
    {


        if (Camera.main != null)
        {
            sceneCamera = Camera.main;
            Camera.main.enabled = false;
            sceneCamera.GetComponent<AudioListener>().enabled = false;
        }

        if (Settings.Instance.localPlayer == null)
            Settings.Instance.localPlayer = GetComponent<FirstPersonController>();

        if (!controller.canMove && wasMoving)
        {
            Debug.Log(wasMoving);
            StunMatServer(1);
            wasMoving = false;
            canMoveAgain = true;
        }
        else if (canMoveAgain && controller.canMove)
        {
            StunMatServer(0);
            canMoveAgain = false;
            wasMoving = true;
        }

    }

    [ServerRpc]
    void CmdChangeDress(GameObject player, GameObject temphat, Vector3 direction)
    {
        ChangeDress(player,temphat, direction);
    }

    [ObserversRpc (BufferLast = true)]
    private void ChangeDress(GameObject player, GameObject temphat, Vector3 direction)
    {
        if (temphat != null){
            var currentHat = Instantiate(temphat, hatToWearPosition.position, Quaternion.identity, hatToWearPosition);
            currentHat.AddComponent<HatPosition>();
            currentHat.tag = "Hat";
            currentHat.layer = 18;
            hat = currentHat;
            currentHat.GetComponent<HatPosition>().reference = hatToWearPosition;
            currentHat.transform.forward = direction;
            currentHat.SetActive(true);
        } 

        var currentCig = Instantiate(CosmeticsManager.Instance.cigs[cig], hatToWearPosition.position, Quaternion.identity, hatToWearPosition);
        currentCig.AddComponent<HatPosition>();
        currentCig.GetComponent<HatPosition>().reference = hatToWearPosition;
        currentCig.SetActive(true);

        player.GetComponent<PlayerSetup>().normalMat = CosmeticsManager.Instance.mats[mat];
        foreach (var obj in meshesToChange)
        {
            obj.GetComponent<SkinnedMeshRenderer>().material = CosmeticsManager.Instance.mats[mat];
        }
        foreach (var obj in fpArmsSuits)
        {
            obj.GetComponent<SkinnedMeshRenderer>().material = CosmeticsManager.Instance.fparmsMats[mat];
        }

        if (!IsOwner && LobbyController.Instance.LocalPlayerController) {
            int teamId = ScoreManager.Instance.GetTeamId(GetComponent<PlayerValues>().playerClient.PlayerId);
            int localTeamId = ScoreManager.Instance.GetTeamId(LobbyController.Instance.LocalPlayerController.PlayerId);
            if (teamId == localTeamId) {
                foreach (GameObject obj in gameObjectsToDisableForMe) {
                    if (obj.name == "SM_Aboubi_Head00") {
                        Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                        newMat[0].SetFloat("_ASEOutlineWidth", 0.02f * OutlineHandler.OutlineWidthMultiplier);
                        newMat[0].SetColor("_ASEOutlineColor", OutlineHandler.TeamColor);
                        obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
                    } else {
                        Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                        newMat[0].SetFloat("_ASEOutlineWidth", 0.04f * OutlineHandler.OutlineWidthMultiplier);
                        newMat[0].SetColor("_ASEOutlineColor", OutlineHandler.TeamColor);
                        obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
                    }
                }
            } else if (GameManager.Instance.EnemyOutlinesEnabled) { // is an enemy
                foreach (GameObject obj in gameObjectsToDisableForMe) {
                    if (obj.name == "SM_Aboubi_Head00") {
                        Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                        newMat[0].SetFloat("_ASEOutlineWidth", 0.02f * OutlineHandler.OutlineWidthMultiplier);
                        newMat[0].SetColor("_ASEOutlineColor", OutlineHandler.EnemyColor);
                        obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
                    }
                    else {
                        Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                        newMat[0].SetFloat("_ASEOutlineWidth", 0.04f * OutlineHandler.OutlineWidthMultiplier);
                        newMat[0].SetColor("_ASEOutlineColor", OutlineHandler.EnemyColor);
                        obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
                    }
                }
            }
        }
        
    }
    public GameObject hat;

    public Material normalMat;
    [SerializeField] private Material normalHeadMat;
    [SerializeField] private Material stunMat;
    [SerializeField] private Material stunHeadMat;
 
    [ServerRpc (RunLocally = true)]
    private void StunMatServer(int i)
    {
        StunMatObservers(i);
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner =  true)]
    private void StunMatObservers(int i)
    {
        stunVFX.SetActive(i==1 ? true : false);
        foreach (var obj in fpArms)
        {
            Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
            newMat[0].SetFloat("Float2", i);
            obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
        }        

        foreach (var obj in gameObjectsToDisableForMe)
        {
            if (obj.name == "SM_Aboubi_Head00" || obj.name == "SK_Aboubi00_Hand_Left00" || obj.name == "SK_Aboubi00_Hand_Right00") {
                Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                newMat[0].SetFloat("Float2", i);
                obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
            }
            else {
                Material[] newMat = obj.GetComponent<SkinnedMeshRenderer>().materials;
                newMat[0].SetFloat("Float2", i);
                obj.GetComponent<SkinnedMeshRenderer>().materials = newMat;
            }
        }
    }

    public void StartLights()
    {
        StartCoroutine(StartingRound());
    }

    IEnumerator StartingRound()
    {
        startLights.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        startLights.SetActive(false);
    }


    private void OnDisable()
    {
        Settings.Instance.localPlayer = null;
        if (sceneCamera != null)
        {
            sceneCamera.enabled = true;
            sceneCamera.GetComponent<AudioListener>().enabled = true;
        }

        if (IsOwner) EnemyHealth();

        if (IsOwner) PauseManager.Instance.minimalistUi.SetActive(false);

        
        if (IsOwner) PauseManager.Instance.ShowEnemyHealth(enemyHealth, GetComponent<PlayerHealth>());

        PauseManager.Instance.MoveAmmoDisplay(false, true);
        PauseManager.Instance.MoveAmmoDisplay(false, false);
        PauseManager.Instance.ChangeAmmoText("---", "", true);
        PauseManager.Instance.ChangeAmmoText("---", "", false);
    }

    float enemyHealth;

    public void EnemyHealth()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length != 0)
        {
            if (sceneCamera != null) {
                if (sceneCamera.GetComponent<KillCam>().enemy != null) {
                    if (sceneCamera.GetComponent<KillCam>().enemy.GetComponent<PlayerHealth>() != null)
                        enemyHealth = sceneCamera.GetComponent<KillCam>().enemy.GetComponent<PlayerHealth>().health;

                    else if (sceneCamera.GetComponent<KillCam>().enemy.GetComponentInParent<PlayerHealth>() != null)
                        enemyHealth = sceneCamera.GetComponent<KillCam>().enemy.GetComponentInParent<PlayerHealth>().health;
                    else {
                        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
                        {
                            if (player.GetComponent<PlayerHealth>() != GetComponent<PlayerHealth>()) {
                                enemyHealth = player.GetComponent<PlayerHealth>().health;
                                break;
                            }
                        }
                    }
                }
                else {
                    foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
                    {
                        if (player.GetComponent<PlayerHealth>() != GetComponent<PlayerHealth>()) {
                            enemyHealth = player.GetComponent<PlayerHealth>().health;
                            break;
                        }
                    }
                }
            }
            else {
                foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (player.GetComponent<PlayerHealth>() != GetComponent<PlayerHealth>()) {
                        enemyHealth = player.GetComponent<PlayerHealth>().health;
                        break;
                    }
                }
            }
        }
        else enemyHealth = 0;
    }

    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;
 
            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);
         
        }
    }

    public GameObject[] meshesToChange;
    public GameObject[] fpArmsSuits;
    public Transform hatToWearPosition;

    
}