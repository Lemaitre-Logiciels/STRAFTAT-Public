using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnerManager : NetworkBehaviour {
	public static GameObject[] AllWeapons;
	public static readonly Dictionary<string, GameObject> NameToWeaponDict = new Dictionary<string, GameObject>();
	public static readonly Dictionary<string, int> NameToIndexDict = new Dictionary<string, int>();
	[SyncVar] public bool randomiseWeapons = false;
	[SyncObject] public readonly SyncList<WeaponData> spawnChances = new SyncList<WeaponData>();
	[SyncVar] public uint totalSpawnWeight = 0;
	
	[SyncVar] public bool swapGuns = false;
	public Dictionary<string, string> Swaps = new Dictionary<string, string>();
	
	[ObserversRpc(BufferLast = true, RunLocally = true)]
	public void SyncRandomSettings(Dictionary<string, string> newSwaps) { Swaps = newSwaps; }
    
    static string _currentMap = "";
    private static void TryBakeSyncForMap(string mapName) {
        if (Instance == null || !InstanceFinder.NetworkManager.IsServer) { return; }
        if (_currentMap == mapName) { return; }
        if (mapName is "MainMenu") { 
	        _currentMap = "";
	        return; 
        }
        if (mapName is "EmptyScene" or "MovedObjectsHolder") { return; }
        _currentMap = mapName;
        
        if (WeaponRemapper.SelectedPreset != null) {
            Instance.swapGuns = true;
            Dictionary<string, string> swaps = WeaponRemapper.SelectedPreset.WeaponMap(_currentMap);
            Instance.SyncRandomSettings(swaps);
        }
        else { Instance.swapGuns = false; }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { TryBakeSyncForMap(scene.name); }

	private const string WeaponsPath = "RandomWeapons";

	public static void PopulateAllWeapons() {
		if (AllWeapons != null) { return; }
		AllWeapons = Resources.LoadAll<GameObject>(WeaponsPath);
		NameToWeaponDict.Clear();
		NameToIndexDict.Clear();
		for (var index = 0; index < AllWeapons.Length; index++) {
			GameObject weapon = AllWeapons[index];
			NameToWeaponDict[weapon.name] = weapon;
			NameToIndexDict[weapon.name] = index;
		}
	}

	public static SpawnerManager Instance;

	private void Awake()
	{
		if (Instance == null) { Instance = this; }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
	}
	
	public void SyncRandomSettings(IEnumerable<WeaponData> weaponData, uint totalWeight)
	{
		spawnChances.Clear();
		foreach (WeaponData data in weaponData) {
			if (!data.IsSpawnable) { continue; }
			spawnChances.Add(data);
		}
		totalSpawnWeight = totalWeight;
	}

	public GameObject GetRandomSpawnableWeapon()
	{
		if (spawnChances == null || spawnChances.Count == 0)
		{
			UpdateSpawnableWeapons();
		}

		if (spawnChances.Count == 0)
		{
			Debug.LogWarning("No spawnable weapons available.");
			return null;
		}

		uint randomValue = (uint)Random.Range(0, totalSpawnWeight);
		uint cumulativeWeight = 0;

		foreach (WeaponData data in spawnChances)
		{
			cumulativeWeight += data.SpawnChance;
			if (randomValue < cumulativeWeight)
			{
				return NameToWeaponDict[data.WeaponName];
			}
		}

		Debug.LogError("Failed to find a weapon for the random value: " + randomValue);
		return null;
	}
	
	public static Dictionary<string, WeaponData> weaponInfo = new();
	public static void UpdateSpawnableWeapons() {
		if (!InstanceFinder.NetworkManager.IsServer) { return; }

		uint totalWeight = 0;
		foreach (WeaponData data in weaponInfo.Values) {
			if (data.IsSpawnable) { totalWeight += data.SpawnChance; }
		}
		Instance.SyncRandomSettings(SpawnerManager.weaponInfo.Values, totalWeight);
	}
}