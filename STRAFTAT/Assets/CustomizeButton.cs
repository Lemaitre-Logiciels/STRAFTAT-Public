using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeButton : MonoBehaviour {
    [SerializeField] private TMP_Text text;
    [SerializeField] private WeaponRandomizationMenu weaponRandomizationMenu;
    [SerializeField] private WeaponRemapper weaponRemapper;
    [SerializeField] private ButtonSizeTween _buttonSizeTween;
    [SerializeField] private Button button;
    
    public void Update() {
        if (!SpawnerManager.Instance) { return; }
        
        if (!SteamLobby.ownDlc0) {
            text.text = "DLC Exclusive";
            _buttonSizeTween.customText = "DLC Exclusive";
            button.interactable = false;
            return;
        }
        text.text = SpawnerManager.Instance.randomiseWeapons ? "Randomizer Settings" : "Swapper Settings";
        _buttonSizeTween.customText = SpawnerManager.Instance.randomiseWeapons ? "Randomizer" : "Swapper";
        button.interactable = true;
    }

    public void PressedButton() {
        if (!SpawnerManager.Instance) { return; }
        if (SpawnerManager.Instance.randomiseWeapons) { weaponRandomizationMenu.ToggleMenu(); }
        else { weaponRemapper.gameObject.SetActive(true); }
    }
}
