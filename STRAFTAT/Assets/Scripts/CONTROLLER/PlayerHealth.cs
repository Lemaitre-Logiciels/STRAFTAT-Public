using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using TMPro;
using DG.Tweening;
using FishNet;
using FishNet.Component.Utility;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar] public float health = 10;
    public float fullHealth;
    public Vector3 spawn;
    [HideInInspector] public FirstPersonController controller;

    private float respawnTimer;
    private float sceneTimer;

    [SyncVar] public bool canMove = true;

    [SyncVar] public PlayerValues playerValues;

    [SyncVar] public Transform killer;
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool isKilled;
    [SyncVar] public bool isShot;
    [SyncVar] public Vector3 lastKillDirection;
    public bool shouldBounce;
    public bool shouldDropWeapon;
    public bool isDeadFromTargetRpc; // Variable to drop weapon on death
    public Vector3 bounceDirection;
    public float bounceForce;
    [SerializeField] private GameObject[] bodyParts;
    [SerializeField] private GameObject aboubiRagdoll;
    [SerializeField] private Vector3 killCamRagdollOffset;
    [SerializeField] private float directionOffset = 3;
    [SerializeField] private GameObject bloodFX;
    [SerializeField] private HealthTween tweenScript;

    [SerializeField] private CameraEffect camHit;
    private TMP_Text healthDisplay;
    public GameObject graphics;
    private GameObject mainCamObject;
    private KillCam killCamScript;
    private Transform playerCamera;

    float tempHealth;

    private bool spawnedRagdoll;
    [HideInInspector] public bool suicide;
    [HideInInspector] public bool fellVoid;

    public void Awake() {
        fullHealth = health;
        controller = GetComponent<FirstPersonController>();
        playerValues = GetComponent<PlayerValues>();
        healthDisplay = GameObject.FindGameObjectWithTag("HealthDisplay").GetComponent<TMP_Text>();
        mainCamObject = GameObject.Find("Main Camera");
        if (mainCamObject.GetComponent<KillCam>() != null) killCamScript = mainCamObject.GetComponent<KillCam>();
        playerCamera = controller.playerCamera.transform;
        killCamScript.ragdollCam = false;
        killCamScript.isDead = false;
        tempHealth = health;
        PingDisplay = InstanceFinder.NetworkManager.GetComponent<PingDisplay>();
    }

    PingDisplay PingDisplay;
    private float count;
    IEnumerator Start() {
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update() {
        if (!IsOwner) { return; }

        tweenScript.health = health;

        if (health <= 0) {
            if (mainCamObject != null) {
                KillCam();
                killCamScript.isDead = true;
                mainCamObject.GetComponent<Camera>().enabled = true;
                mainCamObject.GetComponent<AudioListener>().enabled = true;
            }
        }

        if (health <= 0 && health > -1000 && isKilled) {
            if (GameManager.Instance.roundWasWon != true) {
                //playerValues.playerClient.StartCoroutine(playerValues.playerClient.NextRound());
                GameManager.Instance.PlayerDied(playerValues.playerClient.PlayerId);
                if (fellVoid) PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{playerValues.playerClient.PlayerNameTag}</color></b> fell into the void");
                else if (suicide) PauseManager.Instance.WriteLog($"<b><color=#{PauseManager.Instance.selfNameLogColor}>{playerValues.playerClient.PlayerNameTag}</color></b> commited suicide");

                shouldDropWeapon = true;
                
                //GivePointsToPlayers();
            }
            DespawnObject();
            health = -2000;
            isKilled = false;
        }
        else if (health <= 0 && health > -1000)
        {
            StartCoroutine(SafeDeathFix());
            health = -2000;
        }

        //Minimal HUD Display values

        if (!controller.pauseManager.nonSteamworksTransport){
            controller.pauseManager.minimalistHealthText.text = "health : " + Mathf.Ceil((health/4) * 100).ToString();
            controller.pauseManager.minimalPingText.text = PingDisplay.ping.ToString() + " ms";
            controller.pauseManager.minimalFpsText.text = Mathf.Round(count).ToString() + " fps";
        }

        if (shouldBounce)
        {
            shouldBounce = false;
            AddForce(bounceDirection, bounceForce);
        }

        if (shouldDropWeapon)
        {
            shouldDropWeapon = false;
            if (GetComponent<PlayerPickup>().hasObjectInLeftHand) GetComponent<PlayerPickup>().LeftHandDrop();
            if (GetComponent<PlayerPickup>().hasObjectInHand) GetComponent<PlayerPickup>().RightHandDrop();
        }

        if (isDeadFromTargetRpc) gameObject.SetActive(false);

        if (health != tempHealth)
        {
            tempHealth = health;
            tweenScript.health = health;
            tweenScript.ChangeState();
        }

        //if (Input.GetKeyDown(KeyCode.F)) camHit.TakeHit();
    }

    public IEnumerator UnfreezePlayer(float time)
    {
        yield return new WaitForSeconds(time);

        UnfreezePlayerServer();
    }

    [ServerRpc]
    private void UnfreezePlayerServer()
    {
        controller.canMove = true;
    }

    IEnumerator SafeDeathFix()
    {
        yield return new WaitForSeconds(0.075f);

        if (!isShot && GameManager.Instance.roundWasWon != true) {

            //playerValues.playerClient.StartCoroutine(playerValues.playerClient.NextRound());
            GameManager.Instance.PlayerDied(playerValues.playerClient.PlayerId);
            PauseManager.Instance.WriteLog($"{playerValues.playerClient.PlayerNameTag} died");
            //GivePointsToPlayers();
            
        }
        DespawnObject();
            
        isKilled = false;
    }
    
    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void RemoveHealth(float damage)
    {
        health -= damage;
        HitFeedbackObservers();
    }

    [ObserversRpc]
    private void HitFeedbackObservers()
    {
        HitFeeback();
    }

    public void BumpPlayer(Vector3 dir, float force)
    {
        BumpPlayerObservers(dir, force);
    }

    private void BumpPlayerObservers(Vector3 dir, float force)
    {
        AddForce(dir, force);
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void SetKiller(Transform tempKiller)
    {
        killer = tempKiller;
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void ChangeKilledState(bool tempBool)
    {
        isKilled = tempBool;
    }

    [ObserversRpc]
    private void RemoveHealthObservers()
    {
        KillCam();
    }

    public void Dismemberment(string col)
    {
    }

    public void HitFeeback()
    {
        if (IsOwner) camHit.TakeHit();
    }

    public void Explode(bool explode, bool dismemberment, string memberName, Vector3 ejectForceDir, float force, Vector3 position)
    {   
        ExplodeServer(explode, dismemberment, memberName, ejectForceDir, force, position);
        killCamScript.ragdollCam = true;
    }

    [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void ExplodeServer(bool explode, bool dismemberment, string memberName, Vector3 ejectForceDir, float force, Vector3 position)
    {   
        ExplodeForAll(explode, dismemberment, memberName, ejectForceDir, force, position);
    }

    [ObserversRpc (RunLocally = true)]
    private void ExplodeForAll(bool explode, bool dismemberment, string memberName, Vector3 ejectForceDir, float force, Vector3 position)
    {
        if (spawnedRagdoll) return;

        spawnedRagdoll = true;

        if (explode) 
        {
            if (IsOwner) Settings.Instance.IncreaseDeathsAmount();
            for (int i = 0; i < bodyParts.Length; i++)
            {
                GameObject spawned = Instantiate(bodyParts[i], transform.position + Vector3.up, Quaternion.identity);
                spawned.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * 10, ForceMode.Impulse);
            }
        }
        else
        {
            if (IsOwner) {
                Settings.Instance.IncreaseDeathsAmount();
                SoundManager.Instance.PlaySoundWithPitch(PauseManager.Instance.deathAudioClip[(int)Mathf.RoundToInt(Random.Range(0, PauseManager.Instance.deathAudioClip.Length))], Random.Range(0.95f, 1.05f));
            }

            var ragdoll = Instantiate(aboubiRagdoll, transform.position, transform.rotation);
            
            foreach (var obj in ragdoll.GetComponent<RagdollDress>().meshesToChange)
            {
                obj.GetComponent<SkinnedMeshRenderer>().material = CosmeticsManager.Instance.mats[GetComponent<PlayerSetup>().mat];
            }
            if (GetComponent<PlayerSetup>().hat) Hat(GetComponent<PlayerSetup>().hat, new Vector3(ejectForceDir.x, 0, ejectForceDir.z).normalized);

            
            var bodies = ragdoll.transform.GetComponentsInChildren<Rigidbody>();
            foreach(var body in bodies)
            {
                body.AddExplosionForce(force*1.5f, position - ejectForceDir * 1.5f, 100f, 0f, ForceMode.Impulse);
            }
            killCamScript.ragdoll = ragdoll.transform;
            killCamScript.triggerLookAtBody = true;
            var caca = new Vector3(ejectForceDir.x, 0, ejectForceDir.z).normalized;
            killCamScript.firstPosition = transform.position + killCamRagdollOffset + caca*directionOffset + Vector3.right;
            Instantiate(bloodFX, position, Quaternion.LookRotation(ejectForceDir), FindRecursive("Hips", ragdoll.transform).transform);
            //if (memberName != Torso_1_Col FindRecursive(memberName)
        }
    }

    [ServerRpc (RequireOwnership=false)]
    public void DisablePlayerObjectWhenKilled() {
        DisablePlayerObjectForAll();
    }

    [ObserversRpc]
    private void DisablePlayerObjectForAll() {
        graphics.SetActive(false);
        controller.playerPickupScript.fpArms.gameObject.SetActive(false);
        GetComponent<CharacterController>().enabled = false;
    }

    void Hat(GameObject obj, Vector3 dir)
    {
        obj.transform.SetParent(null);
        if (!obj.GetComponent<Rigidbody>()) obj.AddComponent<Rigidbody>();
        var tempRb = obj.GetComponent<Rigidbody>();
        tempRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tempRb.drag = 0;
        tempRb.interpolation = RigidbodyInterpolation.Interpolate;
        tempRb.AddForce(dir * 1.5f, ForceMode.Impulse);
        var randomInt = Random.Range(-1, 1);
        tempRb.AddTorque(transform.forward * 5 + transform.right * 5, ForceMode.Impulse);
        Crosshair.Instance.hatObj = obj;
    }

    [ServerRpc (RunLocally = true)]
    public void DespawnObject()
    {
        DespawnObjectObservers();
    }

    [ObserversRpc (RunLocally = true, ExcludeOwner = true)]
    private void DespawnObjectObservers()
    {
        transform.gameObject.SetActive(false);
        Explode(false, false, "", -transform.forward, 30, transform.position + Vector3.up * 2 + Vector3.right);
    }

    /*[ServerRpc]
    private void GivePointsToPlayers()
    {

        foreach (var player in SteamLobby.Instance.players)
        {
            if (player.GetComponent<ClientInstance>() != playerValues.playerClient) {
                player.GetComponent<ClientInstance>().AddRoundScore();
                SendNextRoundToPlayer(player.Owner, player.GetComponent<ClientInstance>());
            }

        }
    }*/

    /*[TargetRpc]
    private void SendNextRoundToPlayer(NetworkConnection conn, ClientInstance client)
    {
        client.StartCoroutine(client.NextRound());
    }*/

    public void KillCam()
    {
        if (killer != null)
            killCamScript.enemy = killer;

        if (mainCamObject != null) mainCamObject.transform.position = transform.position + new Vector3(0, GetComponent<CharacterController>().height, 0);
    }

    void OnDisable()
    {
        KillCam();
        PauseManager.Instance.grabPopup.gameObject.SetActive(false);
    }

    public void AddForce(Vector3 force, float factor)
    {
        controller.customForceScript.AddForce(force, factor);
    }

    public void LocalScreenshake(float duration, float strength, int vibrato, float randomness, Ease shakeEase)
    {
        playerCamera.transform.DOKill();
        playerCamera.transform.DOShakeRotation(duration, strength, vibrato, randomness).SetEase(shakeEase);
    }

    GameObject FindRecursive(string name, Transform root)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        GameObject wantedObject = null;
        foreach (var child in children)
        {
            if (child.gameObject.name == name) 
            {
                wantedObject = child.gameObject;
                break;
            }
        }
        
        return wantedObject;
    }

     [ServerRpc (RunLocally = true, RequireOwnership = false)]
    public void TaserEnemy(PlayerHealth enemyHealth, float stunTime)
    {
        enemyHealth.controller.canMove = false;

        TaserEnemyTarget(enemyHealth.playerValues.playerClient.transform.GetComponent<NetworkObject>().Owner, enemyHealth, stunTime);

    }

    [TargetRpc]
    void TaserEnemyTarget(NetworkConnection conn, PlayerHealth enemyHealth, float stunTime)
    {
        
        enemyHealth.StartCoroutine(enemyHealth.UnfreezePlayer(stunTime));

        Debug.Log("Taser");
    }
}