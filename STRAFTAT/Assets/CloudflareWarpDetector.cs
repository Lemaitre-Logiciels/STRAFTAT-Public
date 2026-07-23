using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class CloudflareWarpDetector : MonoBehaviour {
    // https://community.cloudflare.com/t/how-do-i-know-warp-is-running/202227
    // This is some undocumented endpoint that returns info about the connection, including WARP status.
    // Hope it doesnt break!
    private const string TraceUrl = "https://cloudflare.com/cdn-cgi/trace";
    
    private void Start() { StartCoroutine(CheckForCloudflareWarp()); }

    private IEnumerator CheckForCloudflareWarp() {
        using UnityWebRequest webRequest = UnityWebRequest.Get(TraceUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError) {
            Debug.LogError($"Error checking Cloudflare trace endpoint: {webRequest.error}");
            yield break;
        }

        string responseText = webRequest.downloadHandler.text;
        using StringReader reader = new StringReader(responseText);
        
        bool isUsingWarp = false;
        while (reader.ReadLine() is { } line) {
            if (!line.StartsWith("warp=")) { continue; }
            isUsingWarp = line != "warp=off";
            break;
        }

        if (isUsingWarp) {
            Debug.Log("Cloudflare WARP connection detected.");
            WarnPlayer();
        }
        else { Debug.Log("Standard connection detected. No WARP found."); }
    }

    private void WarnPlayer() {
        PauseManager.Instance.ShowInfoPopup("Cloudflare WARP Detected! Cloudflare WARP seemingly breaks steams relayed connections, which causes multiplayer to not work at all. If you are using WARP, please disable it while playing the game. You can re-enable it after you are done playing.");
    }
}