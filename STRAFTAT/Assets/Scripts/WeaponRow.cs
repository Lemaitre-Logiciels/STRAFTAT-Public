using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponRow : MonoBehaviour
{
	public string WeaponName;
	public string DisplayName;
	public TMP_InputField inputField;
	public Slider slider;
	public Toggle toggle;
	public TMP_Text weightedSpawnChance;
	public bool suppressEvents = false;

	private void Awake()
	{
		// Limit slider values to 0 - 100
		slider.minValue = 0;
		slider.maxValue = 100;
	}

	private void Start()
	{
		inputField.text = "100";
		slider.value = 100;
		// Add listeners
		inputField.onValueChanged.AddListener(OnInputFieldChanged);
		slider.onValueChanged.AddListener(OnSliderChanged);
		toggle.onValueChanged.AddListener(OnToggleChanged);
	}

	public void Init(string weaponName, string displayName)
	{
		WeaponName = weaponName;
		DisplayName = displayName;
	}

	private void OnInputFieldChanged(string value)
	{
		// Remove non-numeric characters
		if (int.TryParse(value, out int intValue))
		{
			intValue = Mathf.Clamp(intValue, 0, 100);
			inputField.text = intValue.ToString();
			slider.value = intValue;
		}
		else if (value == "")
		{
			slider.value = 0;
		}
		else
		{
			inputField.text = slider.value.ToString("0");
		}
		WeaponData data = SpawnerManager.weaponInfo[WeaponName];
		data.SpawnChance = (uint)intValue;
		SpawnerManager.weaponInfo[WeaponName] = data;

		if (!suppressEvents)
		{
			WeaponRandomizationMenu.UpdateAllSpawnProbabilities();
		}
	}

	private void OnSliderChanged(float value)
	{
		inputField.text = ((int)value).ToString();
	}

	private void OnToggleChanged(bool value)
	{
		inputField.interactable = value;
		slider.interactable = value;

		WeaponData data = SpawnerManager.weaponInfo[WeaponName];
		data.IsSpawnable = value;
		SpawnerManager.weaponInfo[WeaponName] = data;

		if (!suppressEvents)
		{
			WeaponRandomizationMenu.UpdateAllSpawnProbabilities();
		}
	}

	public float GetSpawnProbabilityPercentage()
	{
		uint totalSpawnWeight = WeaponRandomizationMenu.CachedTotalSpawnableWeight;
		if (totalSpawnWeight == 0)
		{
			return 0f;
		}
		if (!SpawnerManager.weaponInfo.TryGetValue(WeaponName, out WeaponData weapon) || !weapon.IsSpawnable)
		{
			return 0f;
		}

		return ((float)weapon.SpawnChance / totalSpawnWeight) * 100f;
	}

	public void UpdateRowDisplay()
	{
		suppressEvents = true;
		float chance = GetSpawnProbabilityPercentage();
		weightedSpawnChance.text = chance.ToString("F1") + "%";
		inputField.text = SpawnerManager.weaponInfo[WeaponName].SpawnChance.ToString();
		toggle.isOn = SpawnerManager.weaponInfo[WeaponName].IsSpawnable;
		suppressEvents = false;
	}
}