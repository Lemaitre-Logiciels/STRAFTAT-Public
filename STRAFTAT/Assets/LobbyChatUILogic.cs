using System;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Goodgulf.Graphics;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeathenEngineering.DEMO
{
    [RequireComponent(typeof(LobbyManager))]
    public class LobbyChatUILogic : MonoBehaviour
    {
        public static LobbyChatUILogic Instance;

        [SerializeField]
        private int maxMessages = 25;
        [SerializeField]
        private GameObject chatPanel;
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private Transform messageRoot;
        [SerializeField]
        private GameObject myChatTemplate;
        [SerializeField]
        private GameObject theirChatTemplate;

        private LobbyManager lobbyManager;
        private readonly List<IChatMessage> chatMessages = new List<IChatMessage>();
        private bool forceShowMessages;

        private Queue<float> recentMessageTimes = new Queue<float>();
        private int spamThreshold = 10;
        private float spamWindow = 5f; // seconds
        private bool isLockedOut = false;
        private float lockoutEndTime = 0f;
        private float lockoutDuration = 15f; // seconds
        private string spamWarningMessage = "<color=#A00>[WARNING]</color> <color=grey>Stop spamming. Wait {0} seconds before trying again.</color>";

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ClearChat()
        {
            foreach (IChatMessage msg in chatMessages)
            {
                if (msg.GameObject != null) Destroy(msg.GameObject);
            }
            chatMessages.Clear();
        }

        private void Start()
        {
            lobbyManager = GetComponent<LobbyManager>();

            lobbyManager.evtChatMsgReceived.AddListener(HandleChatMessage);
            inputField.onSubmit.AddListener(_ => OnSendChatMessage());
        }

        private void OnDisable() {
            lobbyManager.evtChatMsgReceived.RemoveListener(HandleChatMessage);
            inputField.onSubmit.RemoveListener(_ => OnSendChatMessage());
        }

        private void HandleChatMessage(LobbyChatMsg message) {
            Debug.Log($"Received chat message from {message.sender.Name} ({message.sender.SteamId}): {message.Message}");
            
            if (message.lobby != lobbyManager.Lobby) { return; }
            
            ulong senderSteamId = message.sender.SteamId;
            bool isValidSender = false; //= message.sender == UserData.Me || ClientInstance.playerInstances.Values.Any(id => id.PlayerSteamID == senderSteamId);
            foreach (ClientInstance player in ClientInstance.playerInstances.Values) {
                if (player.PlayerSteamID == senderSteamId) {
                    if (player.textMuted) { return; }
                    isValidSender = true;
                    break;
                }
            }
            if (!isValidSender) { return; }
            
            GameObject messageTemplate = message.sender == UserData.Me ? myChatTemplate : theirChatTemplate;
            GameObject chatMessageGameObject = Instantiate(messageTemplate, messageRoot);
            chatMessageGameObject.transform.SetAsLastSibling();
            IChatMessage chatMessage = chatMessageGameObject.GetComponent<IChatMessage>();
            chatMessage.Initialize(message);

            // If chat is currently forced visible, new messages must inherit that state.
            if (forceShowMessages)
            {
                MatchChatLine newLine = chatMessageGameObject.GetComponent<MatchChatLine>();
                if (newLine != null) { newLine.ForceShow(); }
            }

            if (chatMessages.Count > 0) {
                IChatMessage previousMessage = chatMessages[^1];
                if (previousMessage.GameObject.activeInHierarchy && previousMessage.User == chatMessage.User) {
                    chatMessage.IsExpanded = false; // This hides the username and avatar.
                }
            }
            chatMessages.Add(chatMessage);
            
            // Clear messages past the max limit.
            while (chatMessages.Count > maxMessages) {
                Destroy(chatMessages[0].GameObject);
                chatMessages.RemoveAt(0);
            }

            StartCoroutine(ForceScrollDown()); // I really hate this coroutine
        }

        public bool inQuickMatch;
        public QuickMatchLobbyControl QuickMatchScript;

        private void OnSendChatMessage() {
            string text = inputField.text.Trim();
            if (string.IsNullOrEmpty(text)) { return; }
            
            float now = Time.time;
            if (isLockedOut) {
                if (now >= lockoutEndTime) {
                    isLockedOut = false;
                    recentMessageTimes.Clear();
                } else {
                    FakeChatMessage(string.Format(spamWarningMessage, Mathf.CeilToInt(lockoutEndTime - now)));
                    return;
                }
            }
            // Remove old message times
            while (recentMessageTimes.Count > 0 && now - recentMessageTimes.Peek() > spamWindow) { recentMessageTimes.Dequeue(); }
            
            recentMessageTimes.Enqueue(now);
            if (recentMessageTimes.Count >= spamThreshold) {
                isLockedOut = true;
                lockoutEndTime = now + lockoutDuration;
                FakeChatMessage(string.Format(spamWarningMessage, Mathf.CeilToInt(lockoutEndTime - now)));
                return;
            }
                
            if (inQuickMatch) { if (QuickMatchScript != null) lobbyManager.Lobby = QuickMatchScript.Lobby; }
            lobbyManager.Lobby.SendChatMessage(FilterSystem.FilterString(text));
            inputField.text = string.Empty;
        }
        
        public void FakeChatMessage(string msg) {
            HandleChatMessage (new LobbyChatMsg {
                sender = UserData.Me,
                type = EChatEntryType.k_EChatEntryTypeChatMsg,
                lobby = lobbyManager.Lobby,
                data = System.Text.Encoding.UTF8.GetBytes(msg),
            });
        }

        public void SendChatMessage(string msg)
        {
            lobbyManager.Lobby.SendChatMessage(FilterSystem.FilterString(msg));
        }

        IEnumerator SelectInputField()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            inputField.Select();
        }
        /// <summary>
        /// Called when a new chat message is added to the UI to force the system to scroll to the bottom
        /// </summary>
        /// <returns></returns>
        IEnumerator ForceScrollDown()
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            scrollView.verticalNormalizedPosition = 0f;
        }

        public void EnableForceShowMessages() {
            forceShowMessages = true;
            foreach (IChatMessage chatMsg in chatMessages) {
                MatchChatLine line = chatMsg.GameObject.GetComponent<MatchChatLine>();
                if (line != null) { line.ForceShow(); }
            }
        }

        public void DisableForceShowMessages() {
            forceShowMessages = false;
            foreach (IChatMessage chatMsg in chatMessages) {
                MatchChatLine line = chatMsg.GameObject.GetComponent<MatchChatLine>();
                if (line != null) { line.DisableForceShow(); }
            }
        }
    }
}