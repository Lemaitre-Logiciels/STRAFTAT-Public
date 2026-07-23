using System;
using System.Collections;
using System.Collections.Generic;
using Goodgulf.Graphics;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchLogsOffline : MonoBehaviour
{
    public static MatchLogsOffline Instance { get; private set; }

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
    }

    string previousLine;

    public void WriteLog(string line)
    {
        Debug.Log("Match Log: " + line);
        
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
        newMatchChatLine.destroyWhenDone = true;
        newMatchChatLine.StartDuration();

        previousLine = line;
    }

    void OnDisable()
    {
        //ChatBox.transform.localScale = Vector3.zero;
        //ChatBox.SetActive(true);
    }
    
}



