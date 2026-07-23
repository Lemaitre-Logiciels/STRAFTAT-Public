using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class ComputeryAnnouncements : MonoBehaviour, IPointerClickHandler {
    [Header("Configuration")]
    [Tooltip("The raw URL of the text file on GitHub")]
    public string targetUrl = "https://raw.githubusercontent.com/C0mputery/StraftatLeaderboardWhitelist/refs/heads/main/ComputeryAnnouncements.txt";

    [Header("UI References")]
    [Tooltip("The TextMeshPro UI component to display the text")]
    public TextMeshProUGUI displayTextComponent;
    
    [Header("Canvas Reference")]
    [Tooltip("Ttrsvegrhbdrtnjyhjnyt)")]
    public Canvas parentCanvas;
    
    private void Start() {
        displayTextComponent.text = "fetching announcement...";
        StartCoroutine(FetchTextFromGitHub());
    }

    private IEnumerator FetchTextFromGitHub() {
        using UnityWebRequest webRequest = UnityWebRequest.Get(targetUrl);
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError($"Error fetching text: {webRequest.error}");
            displayTextComponent.text = "Failed to fetch announcement.";
        }
        else {
            string downloadedText = webRequest.downloadHandler.text;
            displayTextComponent.text = downloadedText;
        }
    }
    
    public void OnPointerClick(PointerEventData pointerEventData) {
        if (displayTextComponent == null) { return; }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(displayTextComponent, Input.mousePosition, parentCanvas.worldCamera);
        if (linkIndex == -1) { return; }
        TMP_LinkInfo clickedLinkInfo = displayTextComponent.textInfo.linkInfo[linkIndex];
        string urlToOpen = clickedLinkInfo.GetLinkID();
        Debug.Log($"Opening Link: {urlToOpen}");
        Application.OpenURL(urlToOpen);
    }
}
