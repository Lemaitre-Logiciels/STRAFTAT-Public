using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapSelection : MonoBehaviour
{
    public static MapSelection Instance;
    public SceneMotor sceneMotor;
    private SelectSceneInstance[] sceneInstances = Array.Empty<SelectSceneInstance>();

    [SerializeField] private string[] sceneNames;
    [SerializeField] private GameObject sceneInstance;

    [Space]
    [SerializeField] private GameObject loadPlaylistPanel;
    bool haveInitiated;

    void Awake() { Instance = this; }
    void Start() { if (sceneInstances.Length != MapsManager.Instance.unlockedMaps.Length) { PopulateMaps(); } }

    void LateUpdate() { if (sceneInstances.Length != MapsManager.Instance.unlockedMaps.Length) { PopulateMaps(); } }

    public void PopulateMaps() {
        for (int i=0; i < transform.childCount; i++) { Destroy(transform.GetChild(i).gameObject); }
        
        sceneNames = new string[MapsManager.Instance.unlockedMaps.Length];
        for (int i = 0; i < MapsManager.Instance.unlockedMaps.Length; i++) {
            int map = MapsManager.Instance.unlockedMaps[i];
            sceneNames[i] = MapsManager.Instance.allMaps[map].mapName;
        }
        Array.Sort(sceneNames);

        sceneInstances = new SelectSceneInstance[sceneNames.Length];

        for (int i = 0; i < sceneNames.Length; i++) {
            var ins = Instantiate(sceneInstance, transform.position, Quaternion.identity, transform);
            sceneInstances[i] = ins.GetComponent<SelectSceneInstance>();
            sceneInstances[i].sceneName = sceneNames[i];
            sceneInstances[i].UpdateUI();
            sceneInstances[i].mapSelectionScript = this;
        }
    }


    public void InitiateMaps()
    {
        SelectAll();
        
        MapsManager.Instance.LoadActivePlaylist();
    }

    public void LoadScenes(string[] tempscenes) {
        loadPlaylistPanel.SetActive(false);
        
        foreach (SelectSceneInstance selectSceneInstance in sceneInstances) {
            bool found = tempscenes.Any(sceneName => selectSceneInstance.sceneName == sceneName);
            selectSceneInstance.SetState(found);
        }

        UpdateScenes();
    }

    public void UpdateScenes() {
        sceneMotor = SceneMotor.Instance;
        
        sceneMotor.PlayListMaps.Clear();
        for (int i = 0; i < sceneInstances.Length; i++) {
            if (sceneInstances[i].selected) { sceneMotor.PlayListMaps.Add(sceneInstances[i].sceneName); }
        }
        
        List<string> shuffledMaps = new List<string>(sceneMotor.PlayListMaps);
        sceneMotor.Shuffle(shuffledMaps);
        
        sceneMotor.PlayListMapsQueue.Clear();
        foreach (string map in shuffledMaps) { sceneMotor.PlayListMapsQueue.Enqueue(map); }
    }

    public void SelectAll()
    {
        foreach (var ins in sceneInstances)
        {
            ins.SetState(true);
        }
        UpdateScenes();
    }

    public void DeselectAll()
    {
        foreach (var ins in sceneInstances)
        {
            ins.SetState(false);
        }
        UpdateScenes();
    }

    public void AddPlaylistFromSelection()
    {
        List<SelectSceneInstance> selectedMaps = new List<SelectSceneInstance>();


        foreach (var map in sceneInstances)
        {
            if (map.selected) selectedMaps.Add(map);
        }

        string[] tempscenes = new string[selectedMaps.Count];
        for (int i = 0; i < tempscenes.Length; i++)
        {
            tempscenes[i] = selectedMaps[i].sceneName;
        }
        MatchLogs.Instance.WriteLocalLog($"Created Playlist n°{MapsManager.Instance.Playlists.Count} from selection");
        MapsManager.Instance.AddPlaylist(tempscenes, $"Playlist n°{MapsManager.Instance.Playlists.Count}");
    }
}
