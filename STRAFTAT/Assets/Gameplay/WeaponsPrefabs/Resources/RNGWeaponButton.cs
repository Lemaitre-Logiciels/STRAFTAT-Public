using UnityEngine;
using UnityEngine.UI;

public class RNGWeaponButton : MonoBehaviour {
	public static Toggle toggle;
	public static ButtonSizeTween buttonSizeTween;
	// Disable the Randomize Weapon Checkbox if the player does not own the DLC required.
	private void Awake() {
		toggle = GetComponent<Toggle>();
		buttonSizeTween = GetComponent<ButtonSizeTween>();
	}

	public void Update() {
		bool shouldBeInteractable = SteamLobby.ownDlc0 && Settings.Instance.gamesPlayed >= 10;
		if (toggle.interactable != shouldBeInteractable) {
			toggle.interactable = shouldBeInteractable; 
		}
	}

	public void Toggle(bool state) {
		if (SpawnerManager.Instance && SpawnerManager.Instance.IsServer) { SpawnerManager.Instance.randomiseWeapons = state; }
	}
		
}
