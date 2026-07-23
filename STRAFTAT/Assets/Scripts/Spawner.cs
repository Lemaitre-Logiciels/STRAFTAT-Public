using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The base class for all things spawner related.
/// </summary>
public abstract class Spawner : NetworkBehaviour {
	[FormerlySerializedAs("countdown")]
	[Range(0, 60), SerializeField] protected float weaponRespawnTimeInSeconds = 3f;

	protected float CountdownTimer = 0;
	protected bool WaitTillTaken = true;
	protected ItemBehaviour ItemBehaviour;

	protected virtual void Start() { CountdownTimer = weaponRespawnTimeInSeconds; }

	protected virtual void Update() {
		if (!IsServer) { return; }

		// if waiting till the gun is taken do nothing untill it is
		if (WaitTillTaken && ItemBehaviour && !ItemBehaviour.isTaken) {
			CountdownTimer = weaponRespawnTimeInSeconds;
			return;
		}

		if (CountdownTimer > 0f) {
			CountdownTimer -= Time.deltaTime;
			return;
		}
		Spawn();
		CountdownTimer = weaponRespawnTimeInSeconds;
	}

	public abstract void Spawn();
}

[System.Serializable]
public struct WeaponData
{
	public string WeaponName;
	public uint SpawnChance;
	public bool IsSpawnable;

	public WeaponData(string weaponName, uint spawnChance, bool isSpawnable)
	{
		this.WeaponName = weaponName;
		this.SpawnChance = spawnChance;
		this.IsSpawnable = isSpawnable;
	}
}