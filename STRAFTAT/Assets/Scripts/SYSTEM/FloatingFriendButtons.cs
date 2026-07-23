using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FloatingFriendButtons : MonoBehaviour
{
    [SerializeField] private float offsetY = 18;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button inviteButton;
    public string currentEntry;
    public GameObject display;

    PauseManager pauseManager;

    void Start()
    {
        pauseManager = PauseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseManager.inMainMenu && display.activeSelf) display.SetActive(false);

        if (display.activeSelf)
            if (Vector3.Distance(transform.position, Input.mousePosition) > 200 || Input.mouseScrollDelta != Vector2.zero) display.SetActive(false);
            
    }

    public void Spawn(TMP_InputField text)
    {
        //transform.position = Input.mousePosition + Vector3.up * offsetY;
        transform.position = text.transform.position + Vector3.up * offsetY;
        display.SetActive(true);
        currentEntry = text.text;

        joinButton.interactable = SteamLobby.Instance.CanJoinFriend(currentEntry);
        inviteButton.interactable = SteamLobby.Instance.CanInviteFriend(currentEntry);
    }

    public void OpenFriendChat()
    {
        display.SetActive(false);
        SteamLobby.Instance.OpenFriendChat(currentEntry);
    }

    public void JoinFriendLobby()
    {
        display.SetActive(false);
        SteamLobby.Instance.JoinFriendLobby(currentEntry);
    }

    public void InviteFriendToLobby()
    {
        display.SetActive(false);
        SteamLobby.Instance.InviteFriendToLobby(currentEntry);
    }
}
