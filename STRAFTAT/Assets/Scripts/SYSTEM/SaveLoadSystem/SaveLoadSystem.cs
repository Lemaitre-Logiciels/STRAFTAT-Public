using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;


public class SaveLoadSystem : MonoBehaviour
{
    static bool DisableSavingDueToSteam = false;
    private const string SettingsSaveId = "7ec1c0a0-f12f-4b9f-a6b6-38f4c4e6519b";
    
    // Save file upgrade system
    private delegate Dictionary<string, object> SaveFileUpgradeDelegate(string path);
    private static readonly Dictionary<string, SaveFileUpgradeDelegate> OldSaveFileUpgradeMap = new() { { "Version2", UpgradeV2Save }, };
    private static Dictionary<string, object> UpgradeV2Save(string path) {
        using (FileStream stream = File.Open(path, FileMode.Open)) {
            BinaryFormatter formatter = new();
            return (Dictionary<string, object>)formatter.Deserialize(stream);
        }
    }
    private bool UpgradeSaveFile() {
        string steamID = SteamUser.GetSteamID().ToString();

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.sav");
        foreach (string file in files) {
            
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            if (!fileNameWithoutExtension.Contains(steamID)) { continue; }
            string[] parts = fileNameWithoutExtension.Split(steamID);
            if (parts.Length < 2) { continue; }
            string version = parts[1];
            
            Debug.Log($"SaveLoadSystem: Upgrading save file from version {version}...");
            if (!OldSaveFileUpgradeMap.TryGetValue(version, out SaveFileUpgradeDelegate upgradeDelegate)) { continue; }
            try {
                Dictionary<string, object> state = upgradeDelegate(file);
                SaveFile(state);
                return true;
            }
            catch (Exception ex) {
                Debug.LogError($"Error upgrading save file from version {version}: {ex.Message}");
            }
        }
        return false;
    }
    
    private static string SavePath => Path.Combine(Application.persistentDataPath, $"{SteamUser.GetSteamID()}Version3.sav");
    private static string PreviousSavePath => $"{SavePath}.prev";
    public static SaveLoadSystem Instance;
    
    void Awake() {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        if (!SteamManager.Initialized) {
            Debug.LogError("SaveLoadSystem: SteamManager not initialized, cannot save or load file.");
            DisableSavingDueToSteam = true;
        }
    }

    private void Start() {
        if (DisableSavingDueToSteam) { PauseManager.Instance.ShowInfoPopup("Steam is not initialized, please restart the game!"); }
    }

    [ContextMenu("Save")]
    public void Save() {
        if (DisableSavingDueToSteam) {
            Debug.LogError("SaveLoadSystem: SteamManager was not initialized on Awake, cannot save file.");
            return;
        }
        
        Dictionary<string, object> state = LoadFile();
        if (SaveState(state)) {
            SaveFile(state);
        }
    }

    [ContextMenu("Load")]
    public void Load() {
        Dictionary<string, object> state;
        if (DisableSavingDueToSteam) {
            Debug.LogError("SaveLoadSystem: SteamManager was not initialized on Awake, loading blank file.");
            state = new Dictionary<string, object>();
        } else {
            state = LoadFile();
        }
        
        LoadState(state);
        
        Debug.Log("SaveLoadSystem: Loaded save file successfully.");
    }
    
// Make the save file easy to read for devs only
#if DEBUG || UNITY_EDITOR
    static JsonSerializerSettings JsonSettings = new() {
        Formatting = Formatting.Indented,
    };
#endif
    
    private void SaveFile(Dictionary<string, object> state) {
#if DEBUG || UNITY_EDITOR
        string jsonString = JsonConvert.SerializeObject(state, JsonSettings);
#else
        string jsonString = JsonConvert.SerializeObject(state);
#endif
        
        if (string.IsNullOrEmpty(jsonString)) {
            Debug.LogError("SaveLoadSystem: Save file is empty, not saving.");
            return;
        }
        
        string tempPath = SavePath + ".tmp";
        try {
            File.WriteAllText(tempPath, jsonString);
            if (File.Exists(SavePath)) { File.Replace(tempPath, SavePath, PreviousSavePath); }
            else { File.Move(tempPath, SavePath); }
        }
        catch (Exception ex) { Debug.LogError($"Error saving file: {ex.Message}"); }
        finally { if (File.Exists(tempPath)) { File.Delete(tempPath); } }
    }

    private Dictionary<string, object> LoadFile() {
        if (File.Exists(SavePath)) {
            try {
                string jsonString = File.ReadAllText(SavePath);
                Dictionary<string, object> loadedSave = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                if (loadedSave != null) { return loadedSave; }
                throw new Exception("Save file is null!?");
            }
            catch (Exception ex) {
                Debug.LogError($"Error loading save file at {SavePath}: {ex.Message}, trying previous save file...");
            }
        }
        else {
            if (UpgradeSaveFile()) { return LoadFile(); }
            Debug.LogError($"SaveLoadSystem: No save file found at {SavePath}, trying previous save file...");
        }

        if (File.Exists(PreviousSavePath)) {
            try {
                string jsonString = File.ReadAllText(PreviousSavePath);
                Dictionary<string, object> loadedSave = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                if (loadedSave != null) { return loadedSave; }
                throw new Exception("Save file is null!?");
            }
            catch (Exception ex) {
                Debug.LogError($"Error loading previous save file at {PreviousSavePath}: {ex.Message}, creating new save file...");
                return new Dictionary<string, object>();
            }
        }
        Debug.LogError($"SaveLoadSystem: No temporary save file found at {PreviousSavePath}, creating new save file...");
        return new Dictionary<string, object>();
    }

    private bool SaveState(Dictionary<string, object> state) {
        // Descend into state to find the previous kill count
        
        // If this fails it means the last save on the disk is messed up, and we want to overwrite it anyways
        float oldKillCount = 0; // bro wtf why are kills stored in floats
        if (state.TryGetValue(SettingsSaveId, out object settingsState)) {
            if (settingsState is JObject settingsJObject) {
                try { oldKillCount = settingsJObject["Settings"]!["killsAmount"]!.ToObject<float>(); }
                catch (Exception e) { Debug.LogError($"Error parsing settings state: {e.Message}"); }
            }
        }
        
        SaveableEntity[] saveableEntities = FindObjectsOfType<SaveableEntity>(true);
        foreach (SaveableEntity saveable in saveableEntities) {
            object newState = saveable.SaveState();
            
            if (saveable.Id == SettingsSaveId) {
                if (newState is Dictionary<string, object> newStateDict) {
                    if (newStateDict.TryGetValue("Settings", out object settingsStateObj)) {
                        if (settingsStateObj is Settings.SaveData saveData) {
                            if (oldKillCount > saveData.killsAmount) {
                                Debug.Log("SaveLoadSystem: Old kill count is greater than new kill count, not saving.");
                                return false;
                            }
                        } else {
                            Debug.LogError("SaveLoadSystem: Settings state is not a valid SaveData object? not saving...");
                            return false;
                        }
                    }
                }
            }
            
            state[saveable.Id] = newState;
        }
        
        return true;
    }

    private void LoadState(Dictionary<string, object> state) {
        SaveableEntity[] saveableEntities = FindObjectsOfType<SaveableEntity>(true);
        foreach (SaveableEntity saveable in saveableEntities) {
            if (!state.TryGetValue(saveable.Id, out object savedState)) { continue;}
            try {
                saveable.LoadState((JContainer)savedState);
            }
            catch (Exception e) {
                Debug.LogError($"Error loading part of save state for {saveable.Id}: {e.Message} {e.StackTrace}");
            }
        }
    }
    
    public void OnApplicationQuit() { Save(); }
}
