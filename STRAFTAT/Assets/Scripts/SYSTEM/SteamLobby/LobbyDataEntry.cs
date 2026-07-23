using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class LobbyDataEntry : MonoBehaviour
{
    //Data
    public CSteamID lobbyID;
    public string lobbyName;
    public string ownDlc0;
    public string gamemode;
    public int playerCount;
    public int maxPlayers;
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI gamemodeText;
    [SerializeField] private Image background; 
    [SerializeField] private Color bgColor; 
    [SerializeField] private Color textColor; 
    [SerializeField] private GameObject DlcStar; 
    [SerializeField] private GameObject DlcFrame; 

    public void RenderLobby()
    {
        DlcStar.SetActive(false);
        DlcFrame.SetActive(false);

        if (lobbyID == new CSteamID(SteamLobby.Instance.CurrentLobbyID)) {
            Destroy(gameObject);
        }

        if (lobbyName == "private") Destroy(gameObject);

        lobbyNameText.text = lobbyName == "" ? "Empty" : lobbyName;

        if (ownDlc0 == "True") { // SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "ownDlc0", ownDlc0); // add this line of code into SteamLobby
            background.color = bgColor;
            lobbyNameText.color = textColor;
            DlcStar.SetActive(true);
            DlcFrame.SetActive(true);
        }

        playerCountText.text = playerCount + "/" + maxPlayers;
        gamemodeText.text = gamemode;
    }

    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobbyAuth(lobbyID);
    }


}
