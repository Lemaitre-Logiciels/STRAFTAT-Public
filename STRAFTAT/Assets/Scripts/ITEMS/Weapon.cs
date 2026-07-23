using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class Weapon : NetworkBehaviour
{

	[Header("Weapon Stats")]
	[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public int currentAmmo = 32;
	public float timeBetweenFire = 0.2f;
	public float damage = 1;
	public float headMultiplier = 2;
	public float movementFactor = 1; // heaviness
	public int maxWallJumps = 1;
	public float jumpFactor = 1;
	public float wallJumpFactor = 1;
	[ToggleGroup("fireSlowDown")] public bool fireSlowDown;
	[ToggleGroup("fireSlowDown")] public float fireSlowDownFactor = 0.8f;
	[ToggleGroup("fireSlowDown")] public float fireSlowDownDuration = 0.15f;
	
	[BoxGroup("Accuracy", centerLabel: false)]  public float minSpread = 0;
	[BoxGroup("Accuracy", centerLabel: false)]  public float maxSpread = 0.2f;
	[BoxGroup("Accuracy", centerLabel: false)]  public float standingAccuracy = 1f;
	[BoxGroup("Accuracy", centerLabel: false)]  public float walkAccuracy = 0.75f;
	[BoxGroup("Accuracy", centerLabel: false)]  public float sprintAccuracy = 0.5f;

	[ToggleGroup("ScopeAimWeapon")] public bool ScopeAimWeapon;
	[ToggleGroup("ScopeAimWeapon")] public float notAimingAccuracy = 0.5f;


	[BoxGroup("Weapon Properties", centerLabel: false)] public bool requireBothHands;
	[BoxGroup("Weapon Properties", centerLabel: false)] public bool needsAmmo = true;
	[BoxGroup("Weapon Properties", centerLabel: false)] public bool onePressShoot;
	[BoxGroup("Weapon Properties", centerLabel: false)] public bool changePitchOnShoot;
	[BoxGroup("Weapon Properties", centerLabel: false)] [SerializeField] private bool inHandDespawn;
	[BoxGroup("Weapon Properties", centerLabel: false)] [ToggleGroup("burstGun", groupTitle: "Use Burst")] public bool burstGun;
	[BoxGroup("Weapon Properties", centerLabel: false)] [ToggleGroup("burstGun", groupTitle: "Use Burst")] public bool aimBurstGun;
	[BoxGroup("Weapon Properties", centerLabel: false)] [ToggleGroup("burstGun", groupTitle: "Use Burst")] public int bulletsAmount;
	[BoxGroup("Weapon Properties", centerLabel: false)] [ToggleGroup("burstGun", groupTitle: "Use Burst")] public float timeBetweenBullets;
	[BoxGroup("Weapon Properties", centerLabel: false)] [ToggleGroup("burstGun", groupTitle: "Use Burst")] public float additionalPrecision=1;


	[Header("SFX")]
	[HideInInspector] public AudioSource audio;
	public AudioClip fireClip;
	public AudioClip nobulletClip;
	public AudioClip headHitClip;
	public AudioClip bodyHitClip;
	public AudioClip deathClip;


	[Header("VFX")]
	public GameObject muzzleFlash;
	public float lightIntensity = 5;
	public GameObject headImpact;
	public GameObject bodyImpact;
	public GameObject genericBodyImpact;
	public bool playGenericBodyImpactOnBody;
	public GameObject bloodSplatter;
	public GameObject genericImpact;
	public GameObject bulletHole;
	public LineRenderer bulletTrailLocal;
	public GameObject hitMarker;
	[HideInInspector] public GameObject marker;

	// This attribute has a title specified for the group. The title only needs to be applied to a single attribute for a group.

	[ToggleGroup("SurfacesImpact")]
	public bool SurfacesImpact;

	[ToggleGroup("SurfacesImpact")] public GameObject concreteHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject sandHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject dirtHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject metalHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject tauleHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject waterHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject woodHitImpact;
	[ToggleGroup("SurfacesImpact")] public GameObject softbodyHitImpact;

	[ToggleGroup("SurfacesVFX")]
	public bool SurfacesVFX;

	[ToggleGroup("SurfacesVFX")] public GameObject sandHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject dirtHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject metalHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject tauleHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject waterHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject woodHitFx;
	[ToggleGroup("SurfacesVFX")] public GameObject softbodyHitFx;

	[ToggleGroup("EjectVFX")] public bool EjectVFX;
	[ToggleGroup("EjectVFX")] public GameObject ejectCaseVfx;
	[SerializeField] private GameObject reloadEjectVfx;
	private Transform ejectCasePoint;


	[Header("Layers")]
	public LayerMask defaultLayer = default;
	public LayerMask playerLayer = default;
	public LayerMask supLayer = default;

	[BoxGroup("Camera Animation", centerLabel: false)] public float duration;
	[BoxGroup("Camera Animation", centerLabel: false)] public int vibrato;

	[TabGroup("Normal Shake")]  public Vector3 strength;
	[TabGroup("Normal Shake")]  public float randomness;
	[TabGroup("Normal Shake")]  public bool fadeOut;
	[TabGroup("Normal Shake")]  public ShakeRandomnessMode randomnessMode;
	[TabGroup("Normal Shake")]  public Ease shakeEase;

	[TabGroup("Revolver Shake")]
	public bool revolverShake;

	[TabGroup("Revolver Shake")] [ToggleGroup("akAnim")]
	public bool akAnim = false;
	[TabGroup("Revolver Shake")] [ToggleGroup("akAnim")] [SerializeField] private float cameraLerpSpeed = 3;

	[TabGroup("Revolver Shake")] [SerializeField] private Vector3 recoil;
	[TabGroup("Revolver Shake")] [SerializeField] private float elasticity;

	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private bool instantPush;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private bool holdback;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private bool instantComebackOnFire;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private bool horizontalAnimation;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private bool supplementAnimator;
	public Animator animator;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private Vector3 animationPunch;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private Vector3 foreArmAnimationPunch;
	[BoxGroup("Weapon Animation", centerLabel: false)] public float foreArmAnimationSpeed = 10;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private float animationDuration;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private int animationVibrato;
	[BoxGroup("Weapon Animation", centerLabel: false)] [SerializeField] private float animationElasticity;


	[Header("State")]
	public bool inRightHand;
	public bool inLeftHand;

	private PlayerControls playerControls;
	[HideInInspector] public InputAction fire1, fire2, reload, jump;
	[HideInInspector] public ItemBehaviour behaviour;
	[HideInInspector] public FirstPersonController playerController;
	[HideInInspector] public GameObject rootObject;
	[HideInInspector] public GameObject lastPlayerHolder;
	[HideInInspector] public CameraShakeConstrains camAnimScript;
	private TextMeshProUGUI ammoDisplay;
	[HideInInspector] public Transform shootPoint;
	[HideInInspector] public Transform muzzleFlashPoint;
	[HideInInspector] public Camera cam;
	[HideInInspector] public Tween tween;
	[HideInInspector] public bool isClicked;
	[HideInInspector] public Transform fpArms;
	[HideInInspector] public PlayerValues playerValues;
	[HideInInspector] public bool shot;
	[HideInInspector] public bool heldOnce;
	[HideInInspector] public int noAmmoClicks;
	[HideInInspector] public Transform elbowPivot;
	float caseSoundTimer;

	[ToggleGroup("reloadWeapon", groupTitle: "Reload Weapon")]
	public bool reloadWeapon;
	[HideInInspector] public bool isReloading;
	[ToggleGroup("reloadWeapon")] public int ammoCharge;

	[SerializeField] private int ejectCaseIndex;

	[HideInInspector] public float chargedBullets;
	public float ragdollEjectForce = 50;

	private PauseManager pauseManager;

	private Vector3 initialLocalPos;

	[HideInInspector] public bool cantTakeSafeBool;

	void Awake()
	{
		playerControls = new PlayerControls();
		behaviour = GetComponent<ItemBehaviour>();
		audio = GetComponent<AudioSource>();

		if (GetComponent<Animator>() != null) animator = GetComponent<Animator>();

		if (GetComponentInChildren<ShootPointObject>()) shootPoint = GetComponentInChildren<ShootPointObject>().transform;
		if (GetComponentInChildren<FlashPointObject>()) muzzleFlashPoint = GetComponentInChildren<FlashPointObject>().transform;
		if (GetComponentInChildren<EjectCasePointObject>()) ejectCasePoint = GetComponentInChildren<EjectCasePointObject>().transform;

		pauseManager = PauseManager.Instance;
		

		if (GetComponentInChildren<ElbowPivotPoint>() != null) elbowPivot = GetComponentInChildren<ElbowPivotPoint>().transform;

		//SpawnEffect(spawnEffect);
	}
	
	private void OnDisable() { transform.DOKill(); }

	[HideInInspector] public bool invertFire;
	
	public void WeaponUpdate()
	{
		rootObject = behaviour.rootObject;
		lastPlayerHolder = behaviour.lastPlayerHolder;
		playerController = behaviour.playerController;
		cam = behaviour.cam;

		caseSoundTimer -= Time.deltaTime;

		if (currentAmmo <= 0) cantTakeSafeBool = true;

		//Despawning the item when no more ammo
		if (currentAmmo <= 0 && currentAmmo > -100 && gameObject.layer == 7)
		{
			Invoke("DespawnObject", 0.65f);
			
			currentAmmo = -103;
		}
		else if (heldOnce && lastPlayerHolder == null)
		{
			heldOnce = false;
			gameObject.SetActive(false);
			DespawnObjectServer();
		}
		else if (inHandDespawn && currentAmmo <= 0 && currentAmmo > -100 && gameObject.layer != 7)
		{
			if (inRightHand)
				behaviour.playerPickup.RightHandDrop();
			else if (inLeftHand)
				behaviour.playerPickup.LeftHandDrop();
				
			DespawnObjectServer();
			gameObject.SetActive(false);
			currentAmmo = -103;
		}

		if (gameObject.layer == 7) return;

		fpArms.GetComponent<FPArms>().heavy = behaviour.heavy;
		fpArms.GetComponent<FPArms>().vertical = behaviour.vertical;

		if (inRightHand) {
			playerController.movementFactor = movementFactor;
			playerController.jumpFactor = jumpFactor;
			playerController.maxWallJumps = maxWallJumps;
			playerController.wallJumpFactor = wallJumpFactor;
		}

		if (playerController) {
			fire1 = playerController.fire1;
			fire2 = playerController.fire2;
			reload = playerController.reload;
			jump = playerController.jump;
		}

		if (IsOwner)
		{
			if (inRightHand) {
				if (behaviour.playerPickup.objInHand != gameObject) {
					DespawnObject();
					gameObject.layer = 0;
					behaviour.playerPickup.HandsReconstruct();
				}
			}

			if (inLeftHand) {
				if (behaviour.playerPickup.objInLeftHand != gameObject) {
					DespawnObject();
					gameObject.layer = 0;
					behaviour.playerPickup.HandsReconstruct();
				}
			}
		}


		//If not your weapon, change the layer so you can't see it through walls
		if (rootObject.layer != 6) {
			SetLayerAllChildren(transform, 9);
		}

		if (elbowPivot) elbowPivot.transform.localRotation = Quaternion.Slerp(elbowPivot.transform.localRotation, Quaternion.identity, foreArmAnimationSpeed * Time.deltaTime);

		if (IsOwner) pauseManager.ChangeAmmoText((needsAmmo ? Mathf.Clamp(currentAmmo, 0, currentAmmo).ToString() : "∞"), (reloadWeapon ? chargedBullets.ToString() + " / "  : ""), inRightHand);


		//Sound system (2D for me, 3D for enemy)
		if (IsOwner)
		{
			GetComponent<AudioSource>().spatialBlend = 0;
		}
		else GetComponent<AudioSource>().spatialBlend = 1;

		if (gameObject.layer == 8)
		{
			if (IsOwner)
			{
				invertFire = behaviour.playerPickup.hasObjectInHand && behaviour.playerPickup.hasObjectInLeftHand && Settings.Instance.inverseFireBinding;
			}
		}
	}

	private void DespawnObject()
	{
		if (gameObject.layer == 8 || gameObject.layer == 9) {
			currentAmmo = -50;
			return;
		}

		SpawnEffect(behaviour.depopVFX);
		DespawnObjectServer();
	}

	[ServerRpc (RequireOwnership = false)]
	private void DespawnObjectServer()
	{
		transform.DOKill();
		base.Despawn();
	}

	private void SpawnEffect(GameObject fx)
	{
		Instantiate(fx, transform.position, Quaternion.identity);
		gameObject.SetActive(false);
	}

	


	public void CameraAnimation()
	{
		transform.DOKill();

		camAnimScript.baseSpeed = cameraLerpSpeed;


		if (tween == null) {
			tween = cam.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
		}
		else {
			tween = cam.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode).SetEase(shakeEase);
		}
	}

	void OnShoot()
	{
		if (fireSlowDown) playerController.SetSpeed(fireSlowDownFactor, fireSlowDownDuration);
		if (EjectVFX) 
		{
			var ejectCase = Instantiate(ejectCaseVfx, ejectCasePoint.position, Quaternion.identity);
			ejectCase.transform.forward = -ejectCasePoint.right;

			if (caseSoundTimer > 0) {
				float playsound = Random.Range(0, 100);
				ejectCase.GetComponent<EjectedCaseScript>().shouldPlaySound = (playsound > 60 ? true : false);
			}
			else ejectCase.GetComponent<EjectedCaseScript>().shouldPlaySound = true;

			ejectCase.GetComponent<EjectedCaseScript>().ejectCaseIndex = ejectCaseIndex;
		}

		caseSoundTimer = 0.3f;
	}

	public void OnReload()
	{
		if (reloadEjectVfx) 
		{
			var ejectCase = Instantiate(reloadEjectVfx, ejectCasePoint.position, Quaternion.identity);
			ejectCase.transform.forward = -ejectCasePoint.right;

			if (caseSoundTimer > 0) {
				float playsound = Random.Range(0, 100);
				ejectCase.GetComponent<EjectedCaseScript>().shouldPlaySound = (playsound > 60 ? true : false);
			}
			else ejectCase.GetComponent<EjectedCaseScript>().shouldPlaySound = true;

			ejectCase.GetComponent<EjectedCaseScript>().ejectCaseIndex = ejectCaseIndex;
		}

		caseSoundTimer = 0.3f;
	}


	public void CameraRevolverAnimation()
	{
		transform.DOKill();

		camAnimScript.baseSpeed = cameraLerpSpeed;

		if (!akAnim)
		{
			if (tween == null) {
				tween = cam.transform.DOPunchRotation(-recoil, duration, vibrato, elasticity).SetEase(shakeEase);
			}
			else {
				tween = cam.transform.DOPunchRotation(-recoil, duration, vibrato, elasticity).SetEase(shakeEase);
			}
		}
		else
		{
			if (tween == null) {
				tween = cam.transform.DOLocalRotate(cam.transform.localEulerAngles-recoil, duration).SetEase(shakeEase);
			}
			else {
				tween = cam.transform.DOLocalRotate(cam.transform.localEulerAngles-recoil, duration).SetEase(shakeEase);
			}
		}
		
	}

	public void AltCameraRevolverAnimation()
	{
		transform.DOKill();

		camAnimScript.baseSpeed = cameraLerpSpeed;

		if (!akAnim)
		{
			if (tween == null) {
				tween = cam.transform.DOPunchRotation(-recoil*3, duration, vibrato, elasticity).SetEase(shakeEase);
			}
			else {
				tween = cam.transform.DOPunchRotation(-recoil*3, duration, vibrato, elasticity).SetEase(shakeEase);
			}
		}
		else
		{
			if (tween == null) {
				tween = cam.transform.DOLocalRotate(cam.transform.localEulerAngles-recoil*3, duration).SetEase(shakeEase);
			}
			else {
				tween = cam.transform.DOLocalRotate(cam.transform.localEulerAngles-recoil*3, duration).SetEase(shakeEase);
			}
		}
		
	}


	public void WeaponAnimation()
	{
		OnShoot();
		if (supplementAnimator)
			animator.SetTrigger("Shoot");

		if (instantComebackOnFire)
		{
			transform.DOKill();
			behaviour.InstantComeBackOnFire(); 
		}

		if (holdback)
		{
			transform.DOLocalMove(transform.localPosition-animationPunch, 0);
			transform.DOLocalMove(transform.localPosition-animationPunch-transform.forward*0.01f, animationDuration);
			//StartCoroutine(ComebackFromHoldbackAnimation());
		}     
		else if (instantPush)
		{
			if (horizontalAnimation) 
			{
				if (!playerController.isAiming && elbowPivot) elbowPivot.transform.DOLocalRotate(-foreArmAnimationPunch, animationDuration);
				transform.DOLocalMove(transform.localPosition-animationPunch, animationDuration);
			}
			else if (requireBothHands) {
				fpArms.DOLocalRotate(fpArms.localEulerAngles-animationPunch, animationDuration);
			}
			else
			{
				if (!playerController.isAiming && elbowPivot) elbowPivot.transform.DOLocalRotate(-foreArmAnimationPunch, animationDuration);
				transform.DOLocalRotate(-animationPunch, animationDuration);
			} 
		}
		else
		{
			if (horizontalAnimation) 
			{
				if (!playerController.isAiming && elbowPivot) elbowPivot.transform.DOPunchRotation(-foreArmAnimationPunch, animationDuration, animationVibrato, animationElasticity);
				transform.DOPunchPosition(-animationPunch, animationDuration, animationVibrato, animationElasticity);
			}
			else if (requireBothHands) 
			{
				fpArms.DOPunchRotation(-animationPunch, animationDuration, animationVibrato, animationElasticity);
			}
			else 
			{
				if (!playerController.isAiming && elbowPivot) elbowPivot.transform.DOPunchRotation(-foreArmAnimationPunch, animationDuration, animationVibrato, animationElasticity);
				transform.DOPunchRotation(-animationPunch, animationDuration, animationVibrato, animationElasticity);
			}
		}
		
	}

	IEnumerator ComebackFromHoldbackAnimation()
	{
		yield return new WaitForSeconds(animationDuration);


	}

	public void KillShockWave()
	{
		playerController.lensDistortion.intensity.value = playerController.killShockWaveStrength;
		Settings.Instance.IncreaseKillsAmount();
		playerController.colorGrading.saturation.value = -100;
	}

	public void TriggerEnvironment(GameObject obj, Vector3 hitPoint, Vector3 direction, Vector3 hitNormal)
	{
		if (obj.CompareTag("Mine"))
		{
			if (obj.transform.root.GetComponent<ProximityMine>().canExplode)
				obj.transform.root.GetComponent<ProximityMine>().ChangeState();
		}
		if (obj.CompareTag("Claymore"))
		{
			if (obj.transform.root.GetComponent<Claymore>().canExplode)
				obj.transform.root.GetComponent<Claymore>().ChangeState();
		}


		if (obj.CompareTag("Hat"))
		{
			obj.transform.SetParent(null);
			if (!obj.GetComponent<Rigidbody>()) obj.AddComponent<Rigidbody>();
			var tempRb = obj.GetComponent<Rigidbody>();
			tempRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			tempRb.drag = 0;
			tempRb.interpolation = RigidbodyInterpolation.Interpolate;
			tempRb.AddForce(direction * 10, ForceMode.Impulse);
			var randomInt = Random.Range(-1, 1);
			tempRb.AddTorque(cam.transform.forward * 40 + transform.right * 40, ForceMode.Impulse);
		}

		if (obj.layer == LayerMask.NameToLayer("Ragdoll"))
		{
			var bodies = obj.transform.root.GetComponentsInChildren<Rigidbody>();
			Instantiate(bodyImpact, hitPoint, Quaternion.LookRotation(hitNormal));
			Instantiate(bloodSplatter, hitPoint, Quaternion.LookRotation(hitNormal));
			foreach(var body in bodies)
			{
				body.AddExplosionForce(ragdollEjectForce, hitPoint - direction, 100, 1f, ForceMode.Impulse);
			}
		}

		if (obj.CompareTag("Grenade"))
		{
			CmdExplodeGrenade(obj);
		}

		if (obj.CompareTag("Pig"))
		{
			CmdKillPig(obj);
		}

		if (obj.CompareTag("DetachableObject"))
		{
			CmdDetachObject(obj, direction, hitNormal);
		}

		if (obj.CompareTag("NoSound"))
		{
			if (obj.GetComponent<Pot>() != null)
			{
				Settings.Instance.potsBroken ++;
				CmdBreakPot(obj);
			}

			if (obj.GetComponent<PropDamage>() != null)
			{
				CmdDamageProp(obj);
			}

			if (obj.GetComponent<Pigeon>() != null)
			{
				CmdKillBird(obj);
			}
		}
	}

	public void BreakGlassServer(Vector3 hitPoint, Vector3 direction, GameObject obj)
	{
		Settings.Instance.windowsBroken ++;
		CmdBreakGlassServer(hitPoint, direction, obj);
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	public void CmdBreakGlassServer(Vector3 hitPoint, Vector3 direction, GameObject obj)
	{
		BreakGlassObservers(hitPoint, direction, obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	private void BreakGlassObservers(Vector3 hitPoint, Vector3 direction, GameObject obj)
	{
		if (obj == null) return;
		if (obj.GetComponent<ShatterableGlass>() != null) obj.GetComponent<ShatterableGlass>().Shatter3D(hitPoint, direction);
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdExplodeGrenade(GameObject obj)
	{
		ExplodeGrenadeObservers(obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void ExplodeGrenadeObservers(GameObject obj)
	{
		obj.transform.root.GetComponent<HandGrenade>().explosionTimer = 0;
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdKillPig(GameObject obj)
	{
		KillPigObservers(obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void KillPigObservers(GameObject obj)
	{
		obj.GetComponent<Pig>().FallFar();
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdDetachObject(GameObject obj, Vector3 direction, Vector3 hitNormal)
	{
		DetachObjectObservers(obj, direction, hitNormal);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void DetachObjectObservers(GameObject obj, Vector3 direction, Vector3 hitNormal)
	{
		obj.GetComponent<DetachableObject>().Detach(hitNormal, direction);
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdKillBird(GameObject obj)
	{
		KillBirdObservers(obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void KillBirdObservers(GameObject obj)
	{
		obj.GetComponent<Pigeon>().Die();
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdBreakPot(GameObject obj)
	{
		BreakPotObservers(obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void BreakPotObservers(GameObject obj)
	{
		obj.GetComponent<Pot>().Die();
	}

	[ServerRpc (RequireOwnership = false, RunLocally = true)]
	void CmdDamageProp(GameObject obj)
	{
		CmdDamagePropObservers(obj);
	}

	[ObserversRpc (RunLocally = true, ExcludeOwner = true)]
	void CmdDamagePropObservers(GameObject obj)
	{
		obj.GetComponent<PropDamage>().Damage();
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("Teleport"))
		{
			transform.position = col.GetComponent<Teleporter>().teleportPoint.position;
		}

		if (col.CompareTag("Killz"))
		{
			Invoke("DespawnObject", 0.65f);
		}
	}


	public void SetLayerAllChildren(Transform root, int layer)
	{
		var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (var child in children)
		{
			child.gameObject.layer = layer;
		}
	}

	public bool StartsWithVowel(string name) {
		name=name.ToLower();
		if (name.StartsWith("a") || name.StartsWith("e") || name.StartsWith("i") || name.StartsWith("o") || name.StartsWith("u") || name.StartsWith("y")) return true;
		else return false;
	}

    
    public bool FriendlyFireCheck(PlayerHealth enemyHealth) {
        if (GameManager.Instance.FriendlyFireEnabled) { return false; }
        int teamID = ScoreManager.Instance.GetTeamId(enemyHealth.playerValues.playerClient.PlayerId);
        int attackerTeamID = ScoreManager.Instance.GetTeamId(playerValues.playerClient.PlayerId);
        return teamID == attackerTeamID;
    }

}