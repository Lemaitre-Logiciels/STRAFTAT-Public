using TMPro;
using UnityEngine;

public class LobbyIDHandler : MonoBehaviour {
    private string text = "Hidden";
    private bool currentlyShowing = false;
    public TMP_Text lobbyIDText;

    public void Awake() {
        lobbyIDText.text = text;
        UpdateText();
    }
    
    public void ToggleLobbyIDText() {
        currentlyShowing = !currentlyShowing;
        UpdateText();
    }
    
    public void SetLobbyIDText(string lobbyID) {
        text = lobbyID;
        UpdateText();
    }
    
    private void UpdateText() { lobbyIDText.text = currentlyShowing ? text : "Hidden"; }
}
