using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Logic behind the ItemSpawner platform where most weapons spawn by default.
/// </summary>
public class ItemSpawner : Spawner {
	public GameObject itemToSpawn;

	private void Awake() { WaitTillTaken = true; }

    private void OnEnable() {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    private void Start() { StartNewRound(); }
    
	private void OnDisable() { PauseManager.OnBeforeSpawn -= StartNewRound; }
	public void StartNewRound() { CountdownTimer = 0; }

	public override void Spawn() {
		GameObject toSpawn = itemToSpawn;
		
		if (SpawnerManager.Instance) {
			if (SpawnerManager.Instance.randomiseWeapons) {
				toSpawn = SpawnerManager.Instance.GetRandomSpawnableWeapon();
			}
			else if (SpawnerManager.Instance.swapGuns) {
				if (SpawnerManager.Instance.Swaps.TryGetValue(toSpawn.name, out string name)) {
					if (string.IsNullOrEmpty(name)) { return; }
					if (SpawnerManager.NameToWeaponDict.TryGetValue(name, out GameObject gameObject)) { toSpawn = gameObject; }
				}
			}
		}
		if (toSpawn == null) { return; }
		
		Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
		Quaternion spawnRotation = Quaternion.LookRotation(transform.right);
		GameObject spawned = Instantiate(toSpawn, spawnPosition, spawnRotation, transform);
		ServerManager.Spawn(spawned);

		ItemBehaviour = spawned.GetComponent<ItemBehaviour>();
	}

#if UNITY_EDITOR

	//Editor only functionality
	private void OnValidate()
	{
		//do not run inside the prefab itself
		if (gameObject.scene.name == null)
			return;

		EditorApplication.delayCall += () =>
		{
			DestroyPreview();
			CreatePreview();
		};
	}

	//Removal of previous item preview if it exists
	private void DestroyPreview()
	{
		foreach (Transform t in transform)
		{
			if (transform == null)
				return;

			if (t.name == "EditorPreview")
			{
				DestroyImmediate(t.gameObject);
			}
		}
	}

	private void CreatePreview()
	{
		//do nothing if no item is set
		if (itemToSpawn == null)
			return;

		GameObject spawnedInstance = PrefabUtility.InstantiatePrefab(itemToSpawn) as GameObject;

		if (spawnedInstance != null)
		{
			//non saveable gameobject for the preview
			HideFlags hf = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave | HideFlags.NotEditable;
			spawnedInstance.hideFlags = hf;
			spawnedInstance.name = "EditorPreview";
			spawnedInstance.transform.parent = transform;
			spawnedInstance.transform.localPosition = Vector3.up * 0.5f;
			spawnedInstance.transform.localScale = spawnedInstance.transform.localScale * 5;
			spawnedInstance.transform.localRotation = Quaternion.identity;
		}
	}

	[ContextMenu("Align ItemSpawner With Ground")]
	private void SnapToGround()
	{
		//make it possible to undo after snap
		Undo.RecordObject(transform, "Align With Ground");

		//dont collide with our collider during raycast
		transform.GetComponentInChildren<Collider>().enabled = false;

		Ray ray = new Ray(transform.position, Vector3.down);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 50f))
		{
			//snap to ground
			transform.position = hit.point;
			//align to the surface normal
			transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
		}

		//re-enable the collider
		transform.GetComponentInChildren<Collider>().enabled = true;
	}

	[ContextMenu("Show Weapon Prefabs")]
	private void NavigateToWeaponsPrefabs()
	{
		//path of the weapon prefabs
		string folderPath = "Assets/Gameplay/WeaponsPrefabs/AAA12.prefab";
		Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
		Selection.activeObject = folder;
		EditorUtility.FocusProjectWindow();
		//keep this item spawner selected after navigated to folder
		Selection.activeObject = this;
	}

#endif
}
