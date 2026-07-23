using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPresetManager : MonoBehaviour
{
	[SerializeField] private Transform buttonContainer;
	[SerializeField] private Button buttonPrefab;
	[SerializeField] public TMP_InputField presetNameField;
	[SerializeField] [Required] public WeaponPresetUtility weaponPresetUtil;
	public static WeaponPresetManager Instance { get; private set; }
	
	private void Awake() {
		if (Instance != null) { return; }
		Instance = this;
	}
	private void Start() {
		PopulatePresetList();
	}
	
	// It may be good to refactor this to avoid destroying and instantiating buttons every time but for now this will not be a very big issue
	public void PopulatePresetList()
	{
		// Clear existing buttons
		foreach (Transform child in buttonContainer)
			Destroy(child.gameObject);

		Dictionary<string, WeaponPreset> presets = weaponPresetUtil.Presets;
		foreach (string presetName in presets.Keys) {
			// Instantiate the button
			Button loadButton = Instantiate(buttonPrefab, buttonContainer);
			TMP_Text label = loadButton.GetComponentInChildren<TMP_Text>();
			label.text = presetName;

			// Hook up Load functionality
			loadButton.onClick.AddListener(() => {
				weaponPresetUtil.LoadPreset(presetName);
			});

			// Find the delete button and hook up the delete functionality
			Transform deleteBtnTransform = loadButton.transform.Find("DeleteButton");
			Button deleteBtn = deleteBtnTransform.GetComponent<Button>();
			deleteBtn.onClick.AddListener(() => {
				if (weaponPresetUtil.Presets.Remove(presetName)) { PopulatePresetList(); }
			});
		}
	}
}
