using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaylistPreset", menuName = "ComputeryStuff/PlaylistPreset", order = 1)]
public class PlaylistPreset : ScriptableObject {
    public string Name;
    public string[] Maps;
    [Header("This will make the playlist only available if the player owns the DLC, you can include DLC exclusive maps in non-DLC playlists, but they will not show up if the player does not own the DLC.")]
    public bool IsDlcExclusive;
    
    #if UNITY_EDITOR
    [Button("Import Playlist from Clipboard")]
    public void ImportPlaylistFromClipboard() {
        string clipboardText = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(clipboardText)) {
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
            return;
        }
        
        JObject importedPlaylist;
        try { importedPlaylist = JObject.Parse(decompressedPlaylistString); }
        catch (Exception e) {
            Debug.LogError($"MapsManager: Failed to parse clipboard text as JSON: {e.Message}");
            return;
        }
        
        if (!importedPlaylist.TryGetValue("type", out JToken value)) {
            Debug.LogError("MapsManager: Clipboard JSON does not contain 'type' key.");
            return;
        }
        if (value.ToString() != "playlist") {
            Debug.LogError("MapsManager: Clipboard JSON 'type' is not 'playlist'.");
            return;
        }
        if (!importedPlaylist.TryGetValue("name", out JToken nameToken) || !importedPlaylist.TryGetValue("maps", out JToken mapsToken)) {
            Debug.LogError("MapsManager: Clipboard JSON does not contain 'name' or 'maps' keys.");
            return;
        }
        
        JArray mapsArray = mapsToken as JArray;
        if (mapsArray == null || mapsArray.Count == 0) {
            Debug.LogError("MapsManager: 'maps' key is not an array or is empty.");
            return;
        }
        string[] importedMaps = mapsArray.Select(mapName => mapName.ToString()).ToArray();
        Maps = importedMaps;
        Name = nameToken.ToString();
    }
    #endif
}