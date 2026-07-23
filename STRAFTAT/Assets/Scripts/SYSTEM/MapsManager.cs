using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public class Map {
    public string mapName;
    public int index;
    
    public bool isDlcExclusive;
    public bool isAltMap;
    
    public bool isSelected;
    
    public bool isUnlocked;
    public MapInstance mapInstance;
}

public class Playlist {
    public string Name;
    public string[] Maps;
    public bool IsPreset;
    public Playlist(string name, string[] maps, bool isPreset = false) {
        Name = name;
        Maps = maps;
    }
}

public class MapsManager : MonoBehaviour, ISaveable {
    public static MapsManager Instance;
    
    public Map[] allMaps;
    public Dictionary<string, Map> allMapsDict = new Dictionary<string, Map>();
    public int[] unlockedMaps = Array.Empty<int>();
    public List<int> selectedMaps = new List<int>();
    
    [Space] 
    public List<Playlist> Playlists = new List<Playlist>();
    
    [Space]
    [SerializeField] private GameObject playlistInstance;
    [FormerlySerializedAs("playlistsViewport")] [SerializeField] private Transform playlistParent;
    
    [Space]
    [SerializeField] private GameObject mapInstance;
    [FormerlySerializedAs("mapsViewport")] [SerializeField] private Transform standardMapParent;
    [FormerlySerializedAs("dlcMapsViewport")] [SerializeField] private Transform dlcMapParent;
    [FormerlySerializedAs("altMapsViewport")] [SerializeField] private Transform altMapParent;
    [FormerlySerializedAs("mapsAddedViewport")] [SerializeField] private Transform selectedMapParent;
    
    [Space]
    [FormerlySerializedAs("loadListInstance")] [SerializeField] private GameObject loadPlayListInstance;
    [FormerlySerializedAs("loadListsViewport")] [SerializeField] private Transform playListLoadButtonParent;
    
    [Space]
    [FormerlySerializedAs("mapScreen")] [SerializeField] private RawImage mapPreviewImage;
    [SerializeField] private TextMeshProUGUI mapText;
    
    [Space]
    [SerializeField] private ScrollRect scrollRect;
    public TextMeshProUGUI currentPlaylistText;
    
    [Space]
    [SerializeField] private MapSelection mapSelectionScript;


    private static PlaylistPreset[] Presets;
    
    void Awake() {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); }
        
        if (Presets == null) { Presets = Resources.LoadAll<PlaylistPreset>("PlaylistPresets"); }
    }

    ProgressManager ProgressManager;

    void Start() {
        ProgressManager = ProgressManager.Instance;
        InitMaps();
        
        SaveLoadSystem.Instance.Load();

        ChangePicture(null, "");
        
        if (Playlists.Count == 0) { AddPresetPlaylists(); }
        
        UpdatePlaylists(false);
    }

    public void InitMaps() {
        List<string> tempLockedMaps = new List<string>();
        List<string> tempDLCExclusive = new List<string>();
        foreach (ProgressInstance progressInstance in ProgressManager.instances) {
            if (progressInstance.dlcExlusive) { tempDLCExclusive.AddRange(progressInstance.maps); }
            if (!progressInstance.unlocked) { tempLockedMaps.AddRange(progressInstance.maps); }
        }
        
        allMaps = new Map[SceneManager.sceneCountInBuildSettings-6];
        
        for (int i = 0; i < allMaps.Length; i++) {
            string mapName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i + 6));
            bool mapNameEndsWithAlt = mapName.ToLower().EndsWith("_alt");
            Map map = new() {
                index = i,
                mapName = mapName,
                isDlcExclusive = tempDLCExclusive.Contains(mapName) || mapNameEndsWithAlt,
                isAltMap = mapNameEndsWithAlt,
                isSelected = false,
                isUnlocked = !tempLockedMaps.Contains(mapName),
                mapInstance = null
            };
            
            if (!SteamLobby.ownDlc0 && map.isDlcExclusive) { map.isUnlocked = false; }
            
            allMaps[i] = map;
            allMapsDict.Add(map.mapName, map);
            
            GameObject button = Instantiate(mapInstance, standardMapParent.position, Quaternion.identity, standardMapParent);
            map.mapInstance = button.GetComponent<MapInstance>();
            map.mapInstance.name = map.mapName;
            map.mapInstance.selected = map.isSelected;
            
            button.SetActive(map.isUnlocked);
        }
        
        SortMapsFromMapInstanceName();
        
        List<int> tempUnlockedMapsIndex = new List<int>();
        foreach (Map map in allMaps) { if (map.isUnlocked) { tempUnlockedMapsIndex.Add(map.index); } }
        unlockedMaps = tempUnlockedMapsIndex.ToArray();
    }


    public void UpdateUnlockedMaps() {
        List<string> tempLockedMaps = new List<string>();
        foreach (ProgressInstance progressInstance in ProgressManager.instances) {
            if (!SteamLobby.ownDlc0 && progressInstance.dlcExlusive) { tempLockedMaps.AddRange(progressInstance.maps); }
            if (!progressInstance.unlocked) { tempLockedMaps.AddRange(progressInstance.maps); }
        }

        List<int> tempUnlockedMapsIndex = new List<int>();
        for (int index = 0; index < allMaps.Length; index++) {
            Map map = allMaps[index];
            map.isUnlocked = !tempLockedMaps.Contains(map.mapName);
            map.mapInstance.gameObject.SetActive(map.isUnlocked);
            if (map.isUnlocked) { tempUnlockedMapsIndex.Add(map.index); }
        }
        unlockedMaps = tempUnlockedMapsIndex.ToArray();
    }

    public void OpenFirstPlaylist() {
        if (playlistParent.childCount > 0 && selectedMapParent.childCount == 0) {
            playlistParent.GetComponentsInChildren<PlaylistInstance>()[0].OpenPlaylist();
        }
    }

    public void AddPlaylist(string[] tempPlaylist, string n) {
        Playlists.Add(new Playlist(n, tempPlaylist));
        UpdatePlaylists(true);
    }
    
    public void DupePlaylist(int i) {
        Playlist dupePlaylist = Playlists[i];
        Playlist tempPlaylist = new Playlist($"{dupePlaylist.Name} - copy", dupePlaylist.Maps);
        Playlists.Add(tempPlaylist);
        UpdatePlaylists(true);
    }
    
    public int selectedPlaylistIndex;
    public int activePlaylistIndex = -1;
    [SerializeField] private TextMeshProUGUI activePlaylistText;

    public void ResetActivePlaylist() {
        activePlaylistIndex = -1;
        SetActivePlaylist(activePlaylistIndex);
    }

    public void SetActivePlaylistFromSelectedItem() {
        if (selectedPlaylistIndex >= Playlists.Count || selectedPlaylistIndex < 0) {
            PauseManager.Instance.WriteOfflineLog("Please open a playlist");
            return;
        }

        activePlaylistIndex = selectedPlaylistIndex;
        SetActivePlaylist(activePlaylistIndex);
        SaveLoadSystem.Instance.Save();
    }

    public void SetActivePlaylist(int index) {
        if (index >= Playlists.Count) { index = -1; }

        if (index < 0) { activePlaylistText.text = "all maps"; }
        else { activePlaylistText.text = Playlists[index].Name; }
        
        if (SceneMotor.Instance) LoadActivePlaylist();
    }

    public void LoadActivePlaylist() {
        if (activePlaylistIndex < 0 || activePlaylistIndex >= Playlists.Count) {
            return;
        }
        MapSelection.Instance.LoadScenes(Playlists[activePlaylistIndex].Maps);
    }
    
    public void AddPlaylistWithoutUpdate(string[] tempPlaylist, string n) {
        Playlist playlist = new Playlist(n, tempPlaylist);
        Playlists.Add(playlist);
    }

    public void RemovePlaylist(int i) {
        Playlists.RemoveAt(i);
        UpdatePlaylists(false);
    }

    public void CreatePlaylist() {
        Playlist newPlaylist = new Playlist("Playlist n°" + (Playlists.Count + 1), Array.Empty<string>());
        Playlists.Add(newPlaylist);
        UpdatePlaylists(true);
    }

    public void UpdateLoadLists() {
        int childCount = playListLoadButtonParent.childCount;
        for (int i = 0; i < childCount; i++) {
            Destroy(playListLoadButtonParent.GetChild(i).gameObject);
        }
        
        for (int i=0; i < Playlists.Count; i++) {
            GameObject ins = Instantiate(loadPlayListInstance, playListLoadButtonParent.position, Quaternion.identity, playListLoadButtonParent);
            LoadListInstance insScript = ins.GetComponent<LoadListInstance>();
            insScript.maps = Playlists[i].Maps.ToList();
            insScript.name = Playlists[i].Name;
            insScript.index = i;
        }
    }

    public void UpdatePlaylists(bool hasCreatedNewPlaylist) {
        int childCount = playlistParent.childCount;
        for (int i = 0; i < childCount; i++) {
            Destroy(playlistParent.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < Playlists.Count; i++) {
            GameObject ins = Instantiate(playlistInstance, playlistParent.position, Quaternion.identity, playlistParent);
            PlaylistInstance insScript = ins.GetComponent<PlaylistInstance>();
            insScript.name = Playlists[i].Name;
            insScript.index = i;
            
            if (hasCreatedNewPlaylist && i == Playlists.Count - 1) {
                scrollRect.verticalNormalizedPosition = 0f;
                scrollRect.verticalNormalizedPosition = 0;
                insScript.GetComponent<PlaylistInstance>().button.Select();
                insScript.GetComponent<PlaylistInstance>().OpenPlaylist();
            }
        }

        SaveLoadSystem.Instance.Save();
    }

    public void PopulateMapsFromPlayList(int playListIndex) {
        selectedMaps.Clear();
        foreach (Map map in allMaps) {
            map.isSelected = false;
            map.mapInstance.selected = false;
            map.mapInstance.UpdateUI(); // It'll show up as selected in the UI if this is not called
        }
        
        string[] playlist = Playlists[playListIndex].Maps;
        foreach (string mapName in playlist) {
            if (!allMapsDict.TryGetValue(mapName, out Map map)) { continue; }
            
            if (!SteamLobby.ownDlc0 && map.isDlcExclusive) { continue; }
            if (!map.isUnlocked) { continue; }
            
            map.isSelected = true;
            map.mapInstance.selected = true;
            map.mapInstance.UpdateUI();
            
            selectedMaps.Add(map.index);
        }

        SortMapsFromMapInstanceName();
    }

    public void ChangeMapsState(string mapName) {
        Map map = allMapsDict[mapName];
        map.isSelected = !map.isSelected;
        map.mapInstance.selected = map.isSelected;
        if (map.isSelected) { selectedMaps.Add(map.index); }
        else { selectedMaps.Remove(map.index); }
        SortMapsFromMapInstanceName();
        
        Playlists[selectedPlaylistIndex].Maps = selectedMaps.Select(i => allMaps[i].mapName).ToArray();
        SaveLoadSystem.Instance.Save();
    }

    public void ChangePicture(Texture2D sprite, string txt) {
        if (sprite == null) {
            mapPreviewImage.enabled = false;
            mapText.text = "";
            return;
        }
        
        mapPreviewImage.enabled = true;
        mapPreviewImage.texture = sprite;
        mapText.text = txt;
    }

    [FormerlySerializedAs("inTestMap")] [HideInInspector] public bool inExplorationMap;

    private bool MenuTrigger = false;
    void Update() {
        if (!PauseManager.Instance.inMainMenu) { MenuTrigger = true; }
        if (PauseManager.Instance.inMainMenu && MenuTrigger && !inExplorationMap) {
            inExplorationMap = false;
            MenuTrigger = false;
            ReturnToMenu();
        }
    }

    void ReturnToMenu() {
        UpdatePlaylists(false);
    }
    
    [SerializeField] private TMP_InputField unselectedMapsSearchInput;
    [SerializeField] private TMP_InputField selectedMapsSearchText;
    
    public void SortMapsFromMapInstanceName() {
        // Standard maps in standardMapParent
        List<MapInstance> unselectedStandardMaps = new List<MapInstance>();
        List<MapInstance> unselectedAltMaps = new List<MapInstance>();
        List<MapInstance> unselectedDlcMaps = new List<MapInstance>();
        List<MapInstance> selectedMaps = new List<MapInstance>();
        
        List<Map> sortedAllMaps = allMaps.OrderBy(map => map.mapName).ToList();
        
        string selectedMapsSearchTextLower = selectedMapsSearchText.text.ToLower();
        string unselectedMapsSearchTextLower = unselectedMapsSearchInput.text.ToLower();
        foreach (Map map in sortedAllMaps) {
            if (map.isSelected) {
                selectedMaps.Add(map.mapInstance);

                if (!map.isUnlocked) { continue; }
                
                bool showedUpInSearch = selectedMapsSearchTextLower == "" || map.mapName.ToLower().Contains(selectedMapsSearchTextLower);
                map.mapInstance.gameObject.SetActive(showedUpInSearch);
            } else {
                if (map.isAltMap) { unselectedAltMaps.Add(map.mapInstance); }
                else if (map.isDlcExclusive) { unselectedDlcMaps.Add(map.mapInstance); }
                else { unselectedStandardMaps.Add(map.mapInstance); }
                
                if (!map.isUnlocked) { continue; }
                
                bool showedUpInSearch = unselectedMapsSearchTextLower == "" || map.mapName.ToLower().Contains(unselectedMapsSearchTextLower);
                map.mapInstance.gameObject.SetActive(showedUpInSearch);
            }
        }

        for (int i = 0; i < unselectedStandardMaps.Count; i++) {
            MapInstance map = unselectedStandardMaps[i];
            map.transform.SetParent(standardMapParent);
            map.transform.SetSiblingIndex(i);
        }
        
        for (int i = 0; i < unselectedAltMaps.Count; i++) {
            MapInstance map = unselectedAltMaps[i];
            map.transform.SetParent(altMapParent);
            map.transform.SetSiblingIndex(i);
        }
        
        for (int i = 0; i < unselectedDlcMaps.Count; i++) {
            MapInstance map = unselectedDlcMaps[i];
            map.transform.SetParent(dlcMapParent);
            map.transform.SetSiblingIndex(i);
        }
        
        for (int i = 0; i < selectedMaps.Count; i++) {
            MapInstance map = selectedMaps[i];
            map.transform.SetParent(selectedMapParent);
            map.transform.SetSiblingIndex(i);
        }
    }
    
    // Export play list to clipboard
    public void ExportSelectedPlaylistToClipboard() {
        Playlist selectedPlaylist = Playlists[selectedPlaylistIndex];
        string playlistName = selectedPlaylist.Name;
        string[] playlist = selectedPlaylist.Maps;

        JObject playlistData = new JObject { { "name", playlistName }, { "maps", new JArray(playlist) }, { "type", "playlist" } };
        string playlistString = playlistData.ToString(Newtonsoft.Json.Formatting.None);

        byte[] playlistBytes = System.Text.Encoding.UTF8.GetBytes(playlistString);
        string compressedBase64String;
        using (MemoryStream outputStream = new MemoryStream()) {
            using (GZipStream gZipStream = new GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal)) {
                gZipStream.Write(playlistBytes, 0, playlistBytes.Length);
            }
            compressedBase64String = Convert.ToBase64String(outputStream.ToArray());
        }
        
        GUIUtility.systemCopyBuffer = compressedBase64String;
        PauseManager.Instance.WriteOfflineLog($"Exported {playlistName} to clipboard");
    }
    public void ImportPlaylistFromClipboard() {
        string clipboardText = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(clipboardText)) {
            PauseManager.Instance.WriteOfflineLog("Clipboard is empty");
            return;
        }

        string decompressedPlaylistString;
        try {
            byte[] compressedBytes = Convert.FromBase64String(clipboardText);
            using (MemoryStream inputStream = new MemoryStream(compressedBytes))
            using (GZipStream gZipStream = new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress))
            using (StreamReader streamReader = new StreamReader(gZipStream)) {
                decompressedPlaylistString = streamReader.ReadToEnd();
            }
        }
        catch (Exception e) {
            Debug.LogError($"MapsManager: Failed to decompress clipboard text: {e.Message}");
            PauseManager.Instance.WriteOfflineLog("Failed to decompress playlist from the clipboard.");
            return;
        }
        
        JObject importedPlaylist;
        try { importedPlaylist = JObject.Parse(decompressedPlaylistString); }
        catch (Exception e) {
            Debug.LogError($"MapsManager: Failed to parse clipboard text as JSON: {e.Message}");
            PauseManager.Instance.WriteOfflineLog("Failed to parse playlist from the clipboard.");
            return;
        }
        
        if (!importedPlaylist.TryGetValue("type", out JToken value)) {
            Debug.LogError("MapsManager: Clipboard JSON does not contain 'type' key.");
            PauseManager.Instance.WriteOfflineLog("Invalid playlist format in clipboard.");
            return;
        }
        if (value.ToString() != "playlist") {
            Debug.LogError("MapsManager: Clipboard JSON 'type' is not 'playlist'.");
            PauseManager.Instance.WriteOfflineLog("This is not a playlist!");
            return;
        }
        if (!importedPlaylist.TryGetValue("name", out JToken nameToken) || !importedPlaylist.TryGetValue("maps", out JToken mapsToken)) {
            Debug.LogError("MapsManager: Clipboard JSON does not contain 'name' or 'maps' keys.");
            PauseManager.Instance.WriteOfflineLog("Invalid playlist format in clipboard, missing a name or maps.");
            return;
        }
        
        string playlistName = nameToken.ToString();
        JArray mapsArray = mapsToken as JArray;
        if (mapsArray == null || mapsArray.Count == 0) {
            Debug.LogError("MapsManager: 'maps' key is not an array or is empty.");
            PauseManager.Instance.WriteOfflineLog("Invalid playlist format in clipboard.");
            return;
        }
        string[] importedMaps = mapsArray.Select(mapName => mapName.ToString()).ToArray();
        
        AddPlaylist(importedMaps, playlistName);
    }
    
    //Save playlists
    public object SaveState() {
        List<string[]> savedPlaylists = new List<string[]>();
        List<string> savedPlaylistNames = new List<string>();
        foreach (Playlist playlist in Playlists) {
            savedPlaylists.Add(playlist.Maps.ToArray());
            savedPlaylistNames.Add(playlist.Name);
        }
        
        return new SaveData() {
            savedPlaylists = savedPlaylists,
            savedPlaylistNames = savedPlaylistNames,
            savedActivePlaylistIndex = activePlaylistIndex
        };
    }
    public void LoadState(JContainer state)
    {
        SaveData saveData = state.ToObject<SaveData>();

        List<string[]> savedPlaylists = saveData.savedPlaylists;
        List<string> savedPlaylistNames = saveData.savedPlaylistNames;
        activePlaylistIndex = saveData.savedActivePlaylistIndex;
        SetActivePlaylist(activePlaylistIndex);

        if (savedPlaylists.Count != savedPlaylistNames.Count) {
            Debug.LogError($"MapsManager: Save data playlist array length ({savedPlaylists.Count}) does not match playlist names length ({savedPlaylistNames.Count})");
            int min = Mathf.Min(savedPlaylists.Count, savedPlaylistNames.Count);
            List<string[]> tempPlaylists = new List<string[]>(min);
            List<string> tempPlaylistNames = new List<string>(min);
            for (int i=0; i < min; i++) {
                tempPlaylists.Add(savedPlaylists[i]);
                tempPlaylistNames.Add(savedPlaylistNames[i]);
            }
            savedPlaylists = tempPlaylists;
            savedPlaylistNames = tempPlaylistNames;
        }
        
        // populate Playlists
        Playlists.Clear();
        for (int i = 0; i < savedPlaylists.Count; i++) {
            string[] maps = savedPlaylists[i];
            string name = savedPlaylistNames[i];
            Playlists.Add(new Playlist(name, maps));
        }
        AddPresetPlaylists();
    }

    [Serializable]
    private struct SaveData {
        public List<string[]> savedPlaylists;
        public List<string> savedPlaylistNames;
        public int savedActivePlaylistIndex;
    }

    private void AddPresetPlaylists() {
        AddPresetPlaylist(new Playlist("Standard Maps", allMaps.Where(map => !map.isDlcExclusive && !map.isAltMap).Select(map => map.mapName).ToArray(), true));
        if (SteamLobby.ownDlc0) {
            AddPresetPlaylist(new Playlist("DLC Maps", allMaps.Where(map => map.isDlcExclusive && !map.isAltMap).Select(map => map.mapName).ToArray(), true));
            AddPresetPlaylist(new Playlist("Alt Maps", allMaps.Where(map => map.isAltMap).Select(map => map.mapName).ToArray(), true));
        }
        AddPresetPlaylist(new Playlist("All Maps", allMaps.Select(map => map.mapName).ToArray(), true));
        foreach (PlaylistPreset preset in Presets) {
            if (preset.IsDlcExclusive && !SteamLobby.ownDlc0) { continue; }
            AddPresetPlaylist(new Playlist(preset.Name, preset.Maps, true));
        }
    }
    
    private void AddPresetPlaylist(Playlist preset) {
        if (Playlists.Any(p => p.Name == preset.Name)) { return; } // don't overwrite existing playlists with the same name
        Playlists.Add(preset);
    }
}