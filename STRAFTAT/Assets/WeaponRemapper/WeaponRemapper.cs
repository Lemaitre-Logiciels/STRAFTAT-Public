using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WeaponRemapPreset {
    public string Name = "";
    public List<WeaponRemapMap> Maps = new List<WeaponRemapMap>();

    public Dictionary<string, string> WeaponMap(string mapName) {
        Dictionary<string, string> strings = new Dictionary<string, string>();
        for (var index = Maps.Count - 1; index >= 0; index--) {
            WeaponRemapMap map = Maps[index];

            try {
                string regex = WildCardToRegular(map.MapString);
                if (!Regex.IsMatch(mapName, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) { continue; }
            }
            catch {
                continue;
            }

            foreach (WeaponRemap weaponRemap in map.WeaponRemaps) { strings[weaponRemap.Precursor] = weaponRemap.Result; }
        }
        return strings;
    }
    
    private static string WildCardToRegular(string value) { return $"^{Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*")}$"; }
}

[Serializable]
public class WeaponRemapMap {
    public string MapString = "";
    public List<WeaponRemap> WeaponRemaps = new List<WeaponRemap>();
}

[Serializable]
public class WeaponRemap {
    public string Precursor = ""; // do alchemy n shit
    public string Result = "";
}

public class WeaponRemapper : MonoBehaviour, ISaveable {
    [SerializeField] private WeaponRemapperPresetUI weaponRemapperPresetTemplate;
    [SerializeField] private RectTransform presetParent;
    
    [SerializeField] private WeaponRemapperMapUI weaponRemapperMapTemplate;
    [SerializeField] private RectTransform mapParent;
    
    [SerializeField] private WeaponRemapperSelector weaponRemapperSelectorTemplate;
    [SerializeField] private RectTransform selectorParent;

    [SerializeField] private TMP_Text selectedPresetText;
    [SerializeField] private TMP_Text selectedMapText;

    [HideInInspector] private List<WeaponRemapPreset> _weaponRemaps = new List<WeaponRemapPreset>();
    public static WeaponRemapPreset SelectedPreset;
    private WeaponRemapMap _selectedMap;
    
    public void Awake() {
        SpawnerManager.PopulateAllWeapons();
        
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
        data.Add(new TMP_Dropdown.OptionData("None"));
        foreach (GameObject gameobject in SpawnerManager.AllWeapons) {
            ItemBehaviour itemBehaviour = gameobject.GetComponent<ItemBehaviour>();
            string weaponDisplayName = itemBehaviour.weaponName.ToUpper(new CultureInfo("en-US", false));
            data.Add(new TMP_Dropdown.OptionData(weaponDisplayName));
        }
        weaponRemapperSelectorTemplate.oldWeaponDropDown.AddOptions(data);
        weaponRemapperSelectorTemplate.newWeaponDropDown.AddOptions(data);
    }

    public void Update() {
        if (SelectedPreset != null) { selectedPresetText.text = $"Selected Swap: \"{SelectedPreset.Name}\""; }
        else { selectedPresetText.text = $"Selected Swap: None"; }
        
        if (_selectedMap != null) { selectedMapText.text = $"Selected Map: \"{_selectedMap.MapString}\""; }
        else { selectedMapText.text = $"Selected Map: None"; }
    }

    public void AddNewPreset() {
        WeaponRemapPreset weaponRemapPreset = new WeaponRemapPreset();
        _weaponRemaps.Add(weaponRemapPreset);
        
        InitPreset(weaponRemapPreset);
    }
    public void InitPreset(WeaponRemapPreset weaponRemapPreset) {
        WeaponRemapperPresetUI weaponRemapperPresetUI = Instantiate(weaponRemapperPresetTemplate, presetParent);
        weaponRemapperPresetUI.inputField.text = weaponRemapPreset.Name;
        
        weaponRemapperPresetUI.deleteButton.onClick.AddListener(() => {
            _weaponRemaps.Remove(weaponRemapPreset);
            Destroy(weaponRemapperPresetUI.gameObject);
            if (SelectedPreset == weaponRemapPreset) {
                foreach(Transform child in selectorParent) { Destroy(child.gameObject); }
                foreach(Transform child in mapParent) { Destroy(child.gameObject); }
                SelectedPreset = null;
                _selectedMap = null;
            }
        });
        weaponRemapperPresetUI.inputField.onValueChanged.AddListener((string newName) => { weaponRemapPreset.Name = newName; });
        weaponRemapperPresetUI.inputField.onSelect.AddListener((string _) => {
            if (SelectedPreset == weaponRemapPreset) { return; }
            foreach(Transform child in selectorParent) { Destroy(child.gameObject); }
            foreach(Transform child in mapParent) { Destroy(child.gameObject); }
            
            SelectedPreset = weaponRemapPreset;
            _selectedMap = null;
            
            foreach (WeaponRemapMap weaponRemapMap in weaponRemapPreset.Maps) { InitMap(weaponRemapMap); }
        });
        
        weaponRemapperPresetUI.gameObject.SetActive(true);
    }
    
    public void AddNewMap() {
        if (SelectedPreset == null) {
            PauseManager.Instance.WriteOfflineLog("You need to select a preset first.");
            return;
        }

        WeaponRemapMap weaponRemapMap = new WeaponRemapMap();
        SelectedPreset.Maps.Add(weaponRemapMap);
        
        InitMap(weaponRemapMap);
    }
    private void InitMap(WeaponRemapMap weaponRemapMap) {
        WeaponRemapperMapUI weaponRemapperMapUI = Instantiate(weaponRemapperMapTemplate, mapParent);
        weaponRemapperMapUI.inputField.text = weaponRemapMap.MapString;
        
        weaponRemapperMapUI.deleteButton.onClick.AddListener(() => {
            SelectedPreset.Maps.Remove(weaponRemapMap);
            Destroy(weaponRemapperMapUI.gameObject);
            if (_selectedMap == weaponRemapMap) {
                foreach(Transform child in selectorParent) { Destroy(child.gameObject); }
                _selectedMap = null;
            }
        });
        weaponRemapperMapUI.upButton.onClick.AddListener(() => { MoveMap(weaponRemapMap, weaponRemapperMapUI.transform, -1); });
        weaponRemapperMapUI.downButton.onClick.AddListener(() => { MoveMap(weaponRemapMap, weaponRemapperMapUI.transform, 1); });
        weaponRemapperMapUI.inputField.onValueChanged.AddListener((string newMapString) => { weaponRemapMap.MapString = newMapString; });
        weaponRemapperMapUI.inputField.onSelect.AddListener((string _) => {
            if (_selectedMap == weaponRemapMap) { return; }
            
            foreach(Transform child in selectorParent) { Destroy(child.gameObject); }

            _selectedMap = weaponRemapMap;
            
            foreach (WeaponRemap weaponRemap in weaponRemapMap.WeaponRemaps) { InitOverride(weaponRemap); }
        });
        
        weaponRemapperMapUI.gameObject.SetActive(true);
    }
    
    private void MoveMap(WeaponRemapMap weaponRemapMap1, Transform mapTransform, int offset) {
        int oldIndex = mapTransform.GetSiblingIndex();
        int newIndex = oldIndex + offset;
        if (newIndex < 0 || newIndex >= SelectedPreset.Maps.Count) { return; }
        mapTransform.SetSiblingIndex(newIndex);
        SelectedPreset.Maps.RemoveAt(oldIndex);
        SelectedPreset.Maps.Insert(newIndex, weaponRemapMap1);
    }
    
    public void AddNewOverride() {
        if (_selectedMap == null) {
            PauseManager.Instance.WriteOfflineLog("You need to select a map first.");
            return;
        }
        
        WeaponRemap weaponRemap = new() { Precursor = "", Result = "" };

        _selectedMap.WeaponRemaps.Add(weaponRemap);
        InitOverride(weaponRemap);
    }
    
    private void InitOverride(WeaponRemap weaponRemap) {
        int precursorIndex = 0;
        int resultIndex = 0;
        if (!string.IsNullOrEmpty(weaponRemap.Precursor) && SpawnerManager.NameToIndexDict.TryGetValue(weaponRemap.Precursor, out int precursorIdx)) { precursorIndex = precursorIdx + 1; }
        if (!string.IsNullOrEmpty(weaponRemap.Result) && SpawnerManager.NameToIndexDict.TryGetValue(weaponRemap.Result, out int resultIdx)) { resultIndex = resultIdx + 1; }
        
        WeaponRemapperSelector weaponRemapperSelector = Instantiate(weaponRemapperSelectorTemplate, selectorParent);
        weaponRemapperSelector.oldWeaponDropDown.SetValueWithoutNotify(precursorIndex);
        weaponRemapperSelector.newWeaponDropDown.SetValueWithoutNotify(resultIndex);

        weaponRemapperSelector.oldWeaponDropDown.onValueChanged.AddListener((int selected) => {
            weaponRemap.Precursor = selected == 0 ? "" : SpawnerManager.AllWeapons[selected - 1].name;
            SaveLoadSystem.Instance.Save();
        });
        weaponRemapperSelector.newWeaponDropDown.onValueChanged.AddListener((int selected) => {
            weaponRemap.Result = selected == 0 ? "" : SpawnerManager.AllWeapons[selected - 1].name;
            SaveLoadSystem.Instance.Save();
        });
        weaponRemapperSelector.deleteButton.onClick.AddListener(() => {
            _selectedMap.WeaponRemaps.Remove(weaponRemap);
            Destroy(weaponRemapperSelector.gameObject);
        });
        
        weaponRemapperSelector.gameObject.SetActive(true);
    }

    public void TriggerSave() {
        SaveLoadSystem.Instance.Save();
    }

    public object SaveState() {
        return _weaponRemaps;
    }

    public void LoadState(JContainer state) {
        _weaponRemaps.Clear();
        foreach (Transform child in presetParent) { DestroyImmediate(child.gameObject); }
    
        List<WeaponRemapPreset> loaded = state.ToObject<List<WeaponRemapPreset>>();
        if (loaded != null) {
            foreach (WeaponRemapPreset preset in loaded) {
                if (preset == null) { continue; }
                _weaponRemaps.Add(preset);
                InitPreset(preset);
            }
        }
    }
    
    public void DeselectPreset() {
        foreach(Transform child in selectorParent) { Destroy(child.gameObject); }
        foreach(Transform child in mapParent) { Destroy(child.gameObject); }
            
        SelectedPreset = null;
        _selectedMap = null;
    }
    
    public void Export() {
        if (SelectedPreset == null) {
            PauseManager.Instance.WriteOfflineLog("Nothing is selected.");
            return;
        }
        JObject selectedPresetJObject = JObject.FromObject(SelectedPreset);
        JObject data = new JObject {
            { "Preset", selectedPresetJObject },
            { "type", "swap" }
        };
        
        string exportString = data.ToString(Newtonsoft.Json.Formatting.None);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(exportString);
        string compressedBase64String;
        using (MemoryStream outputStream = new MemoryStream()) {
            using (GZipStream gZipStream = new GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal)) { gZipStream.Write(bytes, 0, bytes.Length); }
            compressedBase64String = Convert.ToBase64String(outputStream.ToArray());
        }
        
        GUIUtility.systemCopyBuffer = compressedBase64String;
        PauseManager.Instance.WriteOfflineLog($"Exported {SelectedPreset.Name} to clipboard");
    }

    public void Import() {
        string clipboardText = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(clipboardText)) {
            PauseManager.Instance.WriteOfflineLog("Clipboard is empty");
            return;
        }

        string decompressedSwapString;
        try {
            byte[] compressedBytes = Convert.FromBase64String(clipboardText);
            using (MemoryStream inputStream = new MemoryStream(compressedBytes))
            using (GZipStream gZipStream =
                   new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress))
            using (StreamReader streamReader = new StreamReader(gZipStream)) {
                decompressedSwapString = streamReader.ReadToEnd();
            }
        }
        catch (Exception e) {
            Debug.LogError($"WeaponRemapper: Failed to decompress clipboard text: {e.Message}");
            PauseManager.Instance.WriteOfflineLog("Failed to decompress swap preset from the clipboard.");
            return;
        }

        JObject importedPlaylist;
        try {
            importedPlaylist = JObject.Parse(decompressedSwapString);
        }
        catch (Exception e) {
            Debug.LogError($"WeaponRemapper: Failed to parse clipboard text as JSON: {e.Message}");
            PauseManager.Instance.WriteOfflineLog("Failed to parse swap preset from the clipboard.");
            return;
        }

        if (!importedPlaylist.TryGetValue("type", out JToken value)) {
            Debug.LogError("WeaponRemapper: Clipboard JSON does not contain 'type' key.");
            PauseManager.Instance.WriteOfflineLog("Invalid swap preset format in clipboard.");
            return;
        }

        if (value.ToString() != "swap") {
            Debug.LogError("WeaponRemapper: Clipboard JSON 'type' is not 'swap'.");
            PauseManager.Instance.WriteOfflineLog("This is not a swap preset!");
            return;
        }

        if (!importedPlaylist.TryGetValue("Preset", out JToken dataToken)) {
            Debug.LogError("WeaponRemapper: Clipboard JSON does not contain the 'SelectedPreset' key.");
            PauseManager.Instance.WriteOfflineLog("Invalid swap preset format in clipboard, could not find data.");
            return;
        }

        WeaponRemapPreset selectedPresetString = dataToken.ToObject<WeaponRemapPreset>();
        _weaponRemaps.Add(selectedPresetString);
        InitPreset(selectedPresetString);
    }

    public GameObject helpScreen;
    public void OnEnable() { helpScreen.SetActive(false); }
    public void ToggleHelpScreen() { helpScreen.SetActive(!helpScreen.activeSelf); }
}
