using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Goodgulf.Graphics;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchLogs : NetworkBehaviour
{
    public static MatchLogs Instance { get; private set; }

    private GameObject ChatBox;
    
    [BoxGroup("Output")] 
    public GameObject chatLinePrefab;
    [BoxGroup("Output")] 
    public Transform parentForChatLines;
    [BoxGroup("Output")] 
    public float duration;
    [BoxGroup("Output")] 
    public float fadeDuration;
    public ClientInstance localPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        parentForChatLines = GameObject.Find("LogsPosition").transform;
        //ChatBox = GameObject.Find("LogsBox");

        //ChatBox.transform.localScale = Vector3.one;
        //ChatBox.SetActive(false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    
    public void WriteLog(string text)
    {
        RpcSendChatLine(text);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RpcSendChatLine(string line)
    {
        RpcSendChatLineToAllObservers(line);
    }

    string previousLine;

    [ObserversRpc]
    private void RpcSendChatLineToAllObservers(string line) {
        Debug.Log("Match Log: " + line);
        
        line = ClientInstance.ReplaceAllPlayerNameTags(line);
        
        line = FilterSystem.FilterString(line);
        
        // Collect all existing chat lines
        int childrenCount = parentForChatLines.childCount;
        
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < childrenCount; i++)
        {
            GameObject child = parentForChatLines.GetChild(i).gameObject;
            TMP_Text childText = child.GetComponentInChildren<TMP_Text>();
            if (childText != null)
            {
                children.Add(child);
            }
        }
        // Move existing lines up and delete EOL chat lines
        for (int i=children.Count - 1; i >= 0;i--)
        {
            GameObject child = children[i];
            MatchChatLine mLine = child.GetComponent<MatchChatLine>();
            if (mLine.deleteMe)
            {
                children.RemoveAt(i);
                Destroy(child);
            }
            else
            {
                // This is a chatline so move it up
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 35);
            }
        }
        
        // Create the new chat line from the prefab chatLinePrefab
        GameObject chatLine = Instantiate(chatLinePrefab, parentForChatLines);
        TMP_Text tmpText = chatLine.GetComponentInChildren<TMP_Text>();
        tmpText.text = line;
        MatchChatLine newMatchChatLine = chatLine.GetComponent<MatchChatLine>();
        newMatchChatLine.duration = duration;
        newMatchChatLine.fadeDuration = fadeDuration;
        newMatchChatLine.StartDuration();

        previousLine = line;
    }

    public void WriteLocalLog(string line)
    {
        // Collect all existing chat lines
        int childrenCount = parentForChatLines.childCount;
        
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < childrenCount; i++)
        {
            GameObject child = parentForChatLines.GetChild(i).gameObject;
            TMP_Text childText = child.GetComponentInChildren<TMP_Text>();
            if (childText != null)
            {
                children.Add(child);
            }
        }
        // Move existing lines up and delete EOL chat lines
        for (int i=children.Count - 1; i >= 0;i--)
        {
            GameObject child = children[i];
            MatchChatLine mLine = child.GetComponent<MatchChatLine>();
            if (mLine.deleteMe)
            {
                children.RemoveAt(i);
                Destroy(child);
            }
            else
            {
                // This is a chatline so move it up
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 35);
            }
        }
        
        // Create the new chat line from the prefab chatLinePrefab
        GameObject chatLine = Instantiate(chatLinePrefab, parentForChatLines);
        TMP_Text tmpText = chatLine.GetComponentInChildren<TMP_Text>();
        tmpText.text = line;
        MatchChatLine newMatchChatLine = chatLine.GetComponent<MatchChatLine>();
        newMatchChatLine.duration = duration;
        newMatchChatLine.fadeDuration = fadeDuration;
        newMatchChatLine.StartDuration();

        previousLine = line;
    }

    void OnDisable()
    {
        //ChatBox.transform.localScale = Vector3.zero;
        //ChatBox.SetActive(true);
    }
    
}



