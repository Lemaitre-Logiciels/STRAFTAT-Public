using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WeaponPresetUtility : MonoBehaviour, ISaveable
{
	public string defaultPresetName = "DefaultPreset";

	public readonly Dictionary<string, WeaponPreset> Presets = new Dictionary<string, WeaponPreset>();
	
	[ContextMenu("Save Preset")]
	public void SaveCurrentPreset() {
		string presetName = defaultPresetName;
		if (!string.IsNullOrEmpty(WeaponPresetManager.Instance.presetNameField.text)) {
			presetName = WeaponPresetManager.Instance.presetNameField.text;
		}

		WeaponPreset preset = new WeaponPreset();
		foreach (WeaponData weaponData in SpawnerManager.weaponInfo.Values) {
			preset.weapons.Add(weaponData);
		}

		Presets[presetName] = preset;
	
		WeaponPresetManager.Instance.PopulatePresetList();
		SaveLoadSystem.Instance.Save();
	}

	[ContextMenu("Load Preset")]
	public void LoadPreset(string presetName) {
		if (!Presets.TryGetValue(presetName, out WeaponPreset preset)) {
			Debug.LogError($"Preset '{presetName}' not found.");
			return;
		} 
		
		foreach (WeaponData data in preset.weapons) {
			SpawnerManager.weaponInfo[data.WeaponName] = data;
		}

		SpawnerManager.UpdateSpawnableWeapons();
		WeaponRandomizationMenu.UpdateAllSpawnProbabilities();
	}

	public object SaveState() {
		JObject state = new();
		foreach (KeyValuePair<string, WeaponPreset> kvp in Presets) {
			state[kvp.Key] = JObject.FromObject(kvp.Value);
		}
		return state;
	}

	public void LoadState(JContainer state) {
		JObject stateObject = (JObject)state;
		Presets.Clear();
		foreach (KeyValuePair<string, JToken> kvp in stateObject) {
			string presetName = kvp.Key;
			WeaponPreset preset = kvp.Value.ToObject<WeaponPreset>();
			Presets[presetName] = preset;
		}

		WeaponPresetManager.Instance?.PopulatePresetList();
	}
}