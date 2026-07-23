using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using System.IO;

public class LoadListInstance : MonoBehaviour
{
    public List<string> maps = new List<string>();
    public string name;
    public int index;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private Button button;

    void Start() {
        text.text = name;
        for (int i = maps.Count - 1; i >= 0; i--) {
            string map = maps[i];
            if (!MapsManager.Instance.allMapsDict.TryGetValue(map, out Map mapData) || (!SteamLobby.ownDlc0 && mapData.isDlcExclusive)) {
                maps.RemoveAt(i);
            }
        }
        maps.Sort();
    }

    public void LoadPlaylist()
    {
        MapSelection.Instance.LoadScenes(maps.ToArray());
        PauseManager.Instance.WriteOfflineLog("You loaded " + name);
    }

}
