using FishNet.Object;
using UnityEngine;

/// <summary>
/// Logic for the WeaponDropper (Invisible weapon spawning GameObject)
/// </summary>
public class WeaponDropper : Spawner {
	public GameObject[] itemsToSpawn;
	private bool ReadyToSpawn = false;
	[SerializeField] private Vector3 ejectDirection = Vector3.zero;

	private void Awake() { WaitTillTaken = false; }
	private void OnEnable() { PauseManager.OnRoundStarted += StartNewRound; }
	private void OnDisable() { PauseManager.OnRoundStarted -= StartNewRound; }
	private void StartNewRound() { ReadyToSpawn = true; }
	
	protected override void Update() {
		if (!ReadyToSpawn) { return; }
		base.Update();
	}

	public override void Spawn() {
		GameObject toSpawn;

		if (SpawnerManager.Instance.randomiseWeapons) {
			toSpawn = SpawnerManager.Instance.GetRandomSpawnableWeapon();
		}
		else {
			toSpawn = itemsToSpawn[Random.Range(0, itemsToSpawn.Length)];
			if (SpawnerManager.Instance.swapGuns) {
				if (SpawnerManager.Instance.Swaps.TryGetValue(toSpawn.name, out string name)) {
					if (string.IsNullOrEmpty(name)) { return; }
					if (SpawnerManager.NameToWeaponDict.TryGetValue(name, out GameObject gameObject)) { toSpawn = gameObject; }
				}
			}
		}
		if (toSpawn == null) { return; }

		Vector3 spawnPos = transform.position + Vector3.up / 2;
		GameObject spawned = Instantiate(toSpawn, spawnPos, Quaternion.identity);
		ServerManager.Spawn(spawned);
		spawned.GetComponent<ItemBehaviour>().DispenserDrop(ejectDirection);
	}
}