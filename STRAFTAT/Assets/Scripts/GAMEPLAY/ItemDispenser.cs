using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

/// <summary>
/// Logic behind the ItemDispenser game object. (Slot machine)
/// </summary>
public class ItemDispenser : InteractEnvironment
{
	public GameObject[] itemsToSpawn;
	public Vector3 restPos;
	[SyncVar] public GameObject spawnedItem;
	[SerializeField] private Transform origin;
	[SerializeField] private Animator anim;
	[SerializeField] private AudioClip triggerClip;

	[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public float timer;
	private float localTimer;
	[SerializeField] private float countdown = 3;

	[Space]
	[SerializeField] private Light light;

	[SerializeField] private MeshRenderer screen;
	[SerializeField] private GameObject availableScreen;
	[SerializeField] private GameObject loadingScreen;
	[SerializeField] private Color availableLightColor;
	[SerializeField] private Color loadingLightColor;
	[SerializeField] private Material availableLightMat;
	[SerializeField] private Material loadingLightMat;

	private AudioSource audio;
	private GameObject item;

	private void Start()
	{
		audio = GetComponent<AudioSource>();
	}

	public override void OnFocus()
	{
		PauseManager.Instance.interactPopup.gameObject.SetActive(true);
		PauseManager.Instance.interactPopup.text = popupText.ToLower() + " [" + PauseManager.Instance.InteractPromptLetter.ToLower() + "]";
	}

	public override void OnInteract(Transform player)
	{
		if (timer > 0 || localTimer > 0) return;

		CmdTimer();
		StartCoroutine(SpawnItem());
	}

	/// <summary>
	/// Spawns a weapon based on a map specific weapon list or a Random weapon if <see cref="Spawner.IsRandomWeaponsEnabled"/> is True.
	/// </summary>
	/// <returns></returns>
	private IEnumerator SpawnItem()
	{
		yield return new WaitForSeconds(0.2f);

		if (SpawnerManager.Instance.randomiseWeapons)
		{
			item = SpawnerManager.Instance.GetRandomSpawnableWeapon();
		}
		else
		{
			//default behaviour, picking item from local pool
			item = itemsToSpawn[Random.Range(0, itemsToSpawn.Length)];
			if (SpawnerManager.Instance.swapGuns) {
				if (SpawnerManager.Instance.Swaps.TryGetValue(item.name, out string name)) {
					if (string.IsNullOrEmpty(name)) { yield break; }
					if (SpawnerManager.NameToWeaponDict.TryGetValue(name, out GameObject gameObject)) { item = gameObject; }
				}
			}
		}
		if (item == null) { yield break; }

		SpawnWeapon(item);
	}

	public override void OnLoseFocus()
	{
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		localTimer -= Time.deltaTime;
		if (((timer > 0 && timer < countdown - 0.3f) || (localTimer > 0 && localTimer < countdown - 0.3f)) && availableScreen.activeSelf)
		{
			availableScreen.SetActive(false);
			loadingScreen.SetActive(true);
			light.color = loadingLightColor;
			Material[] mats = new Material[1];
			mats[0] = loadingLightMat;
			screen.materials = mats;
		}
		else if ((timer <= 0 || localTimer <= 0) && loadingScreen.activeSelf)
		{
			availableScreen.SetActive(true);
			loadingScreen.SetActive(false);
			light.color = availableLightColor;
			Material[] mats = new Material[1];
			mats[0] = availableLightMat;
			screen.materials = mats;
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void CmdTimer()
	{
		timer = countdown;
		localTimer = countdown;
		PlaySound();
	}

	[ObserversRpc]
	private void PlaySound()
	{
		anim.SetTrigger("Trigger");
		audio.PlayOneShot(triggerClip);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnWeapon(GameObject item) {
		GameObject spawned = Instantiate(item, origin.position, Quaternion.identity);
		ServerManager.Spawn(spawned);
		spawnedItem = spawned;
		spawned.GetComponent<ItemBehaviour>().DispenserDrop(origin.forward);
		timer = countdown;
		localTimer = countdown;
	}
}