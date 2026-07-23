using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WeaponRandomizationMenu : MonoBehaviour
{
	public static uint CachedTotalSpawnableWeight = 0;

	[SerializeField] private VerticalLayoutGroup weaponList;
	[SerializeField] private GameObject weaponRowPrefab;
	[SerializeField] private GameObject menuButton;
	[SerializeField] private Canvas menuCanvas;
	[SerializeField] private TMP_Text presetName;
	[SerializeField] private TMP_InputField weaponNameInputField;

	private static bool toggleState = false;
	private static bool sliderMaxToggle = true;
	private static List<WeaponRow> weaponRows = new List<WeaponRow>();

	private void Awake() {
		SpawnerManager.PopulateAllWeapons();
		menuCanvas.enabled = false;
	}

	private void Start()
	{
		PopulateMenu();
		weaponNameInputField.onValueChanged.AddListener(FilterListByWeaponName);
	}

	private void Update() {
		if (!SpawnerManager.Instance) return;
		
		/*
		bool shouldShowMenuButton = SteamLobby.ownDlc0 && SpawnerManager.Instance.randomiseWeapons;
		if (shouldShowMenuButton && !menuButton.activeSelf) { menuButton.SetActive(true); }
		else if (!shouldShowMenuButton && menuButton.activeSelf) { menuButton.SetActive(false); }
		*/
	}

	private static void UpdateCachedTotalWeight()
	{
		CachedTotalSpawnableWeight = (uint)SpawnerManager.weaponInfo.Values
		.Where(data => data.IsSpawnable)
		.Sum(data => data.SpawnChance);
	}

	public void ToggleMenu()
	{
		menuCanvas.enabled = !menuCanvas.enabled;
	}

	/// <summary>
	/// Creates new <see cref="WeaponRow"/>'s for each weapon that can be controlled.
	/// </summary>
	private void PopulateMenu()
	{
		foreach (GameObject weapon in SpawnerManager.AllWeapons)
		{
			ItemBehaviour itemBehaviour = weapon.GetComponent<ItemBehaviour>();
			GameObject newRow = Instantiate(weaponRowPrefab, weaponList.transform);
			WeaponRow row = newRow.GetComponent<WeaponRow>();
			string weaponDisplayName = itemBehaviour.weaponName.ToUpper(new CultureInfo("en-US", false));
			row.Init(weapon.name, weaponDisplayName);
			RegisterRow(row);

			TextMeshProUGUI nameText = newRow.transform.Find("NameLabel").GetComponent<TextMeshProUGUI>();
			Toggle toggle = newRow.transform.Find("EnableToggle").GetComponent<Toggle>();
			Slider slider = newRow.transform.Find("SpawnChanceSlider").GetComponent<Slider>();
			TMP_InputField inputField = newRow.transform.Find("PercentInput").GetComponent<TMP_InputField>();

			nameText.text = weaponDisplayName;
			toggle.isOn = true;
			slider.value = 100;

			WeaponData weaponData = new(weapon.name, (uint)slider.value, toggle.isOn);
			SpawnerManager.weaponInfo.Add(weapon.name, weaponData);
		}
		UpdateAllSpawnProbabilities();
	}

	private void RegisterRow(WeaponRow row)
	{
		if (!weaponRows.Contains(row))
		{
			weaponRows.Add(row);
		}
	}

	/// <summary>
	/// Recalculates the spawn probabilities for all <see cref="WeaponRow"/>s in the menu.
	/// </summary>
	public static void UpdateAllSpawnProbabilities()
	{
		UpdateCachedTotalWeight();
		foreach (WeaponRow row in weaponRows)
		{
			row.UpdateRowDisplay();
		}
		SpawnerManager.UpdateSpawnableWeapons();
	}

	/// <summary>
	/// Sets all toggle values for <see cref="WeaponRow"/> in weaponList to be either True or False
	/// </summary>
	public void ToggleAllWeapons()
	{
		foreach (var row in weaponRows)
		{
			if (row != null)
			{
				row.suppressEvents = true;
				row.toggle.isOn = toggleState;
				row.suppressEvents = false;
			}
		}
		toggleState = !toggleState;
		UpdateAllSpawnProbabilities();
	}

	/// <summary>
	/// Adjusts all weapon row slider values to be either the minimum or maximum value.
	/// </summary>
	public void SetAllSlidersToMinOrMax()
	{
		foreach (var row in weaponRows)
		{
			if (row != null)
			{
				row.suppressEvents = true;
				row.slider.value = sliderMaxToggle ? 100 : 0;
				row.suppressEvents = false;
			}
		}
		sliderMaxToggle = !sliderMaxToggle;
		UpdateAllSpawnProbabilities();
	}

	/// <summary>
	/// Completely randomizes the spawn chance and enabled status for each gun. Let's go gambling :D
	/// </summary>
	public static void RandomizeSettings()
	{
		foreach (var weapon in SpawnerManager.weaponInfo.ToList())
		{
			SpawnerManager.weaponInfo[weapon.Key] = new WeaponData
			{
				WeaponName = weapon.Key,
				SpawnChance = (uint)Random.Range(0, 101),
				IsSpawnable = Random.Range(0, 2) == 0
			};
		}
		UpdateAllSpawnProbabilities();
	}

	/// <summary>
	/// Filters the list of weapon rows by certain name
	/// </summary>
	public void FilterListByWeaponName(string weaponName)
	{
		if (weaponNameInputField == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(weaponName))
		{
			foreach (WeaponRow row in weaponRows)
			{
				if (row != null && !row.isActiveAndEnabled)
				{
					row.gameObject.SetActive(true);
				}
			}
			UpdateAllSpawnProbabilities();
			return;
		}

		HashSet<WeaponRow> matchesSet = new HashSet<WeaponRow>(
			weaponRows.Where(i =>
				i != null &&
				i.DisplayName.Contains(weaponName, StringComparison.CurrentCultureIgnoreCase))
		);

		foreach (WeaponRow row in weaponRows)
		{
			if (row == null) continue;

			bool shouldBeActive = matchesSet.Contains(row);
			if (row.gameObject.activeSelf != shouldBeActive)
			{
				row.gameObject.SetActive(shouldBeActive);
			}
		}
		UpdateAllSpawnProbabilities();
	}
}