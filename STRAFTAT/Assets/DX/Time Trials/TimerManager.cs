using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using HeathenEngineering.DEMO;
using Newtonsoft.Json.Linq;

public class TimerManager : MonoBehaviour, ISaveable
{
    public bool enabled = true;
    public Material invisibleMat;
    TextMeshProUGUI timeUI;
    TextMeshProUGUI timeUIPrevAndPB;
    TextMeshProUGUI courseName;
    AudioSource beep;
    AudioSource checkpointSound;
    AudioSource pbSound;

    Canvas canvas;
    Array triggers;

    [HideInInspector] public FirstPersonController player;
    [HideInInspector] public Transform currentStartPoint;


    void Start()
    {
        timeUI = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        timeUIPrevAndPB = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        courseName = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        courseName.text = "";
        timeUI.text = FormatTime(time);

        beep = transform.GetChild(3).GetComponent<AudioSource>();
        checkpointSound = transform.GetChild(4).GetComponent<AudioSource>();
        pbSound = transform.GetChild(5).GetComponent<AudioSource>();

        canvas = GetComponent<Canvas>();
        canvas.enabled = false;

        triggers = FindObjectsOfType<TimerTrigger>();
        transform.localScale = Vector3.one;

        SaveLoadSystem.Instance.Load();

        CheckEnabled();
    }

    float time = 0.0f;
    float previousTime = -1.0f;
    string previousCourse = "";
    string mapAndCourse = "";
    public bool timerStarted = false;

    Dictionary<string, float> courseTimes = new Dictionary<string, float>
    {
    };

    public void Enable(bool f) {
        enabled = f;
        CheckEnabled();
    }

    void CheckEnabled() {
        if (enabled) {
            foreach (TimerTrigger trigger in triggers)
                trigger.transform.gameObject.SetActive(true);
        }
        else {
            foreach (TimerTrigger trigger in triggers)
                trigger.transform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (currentStartPoint != null && Input.GetKey(KeyCode.LeftControl)) {
            if (Input.GetKeyDown(KeyCode.R)) {
                player.Teleport(currentStartPoint.position, currentStartPoint.eulerAngles.y, false, currentStartPoint, 0, 0, false);
            }
        }

        if (!timerStarted)
            return;

        timeUI.text = FormatTime(time);

        time += Time.deltaTime;
        
    }

    public void TStart(string course)
    {
        CancelInvoke(nameof(HideCanvas));
        canvas.enabled = true;

        foreach (TimerTrigger trigger in triggers)
        {
            if (trigger.transform.parent.gameObject.name == course)
                trigger.ResetMat();
            else
                trigger.HideMat();
        }

        if (course != previousCourse)
        {
            previousTime = -1.0f;
            previousCourse = course;
        }

        prevCheckpoint = 0;
        canFinish = false;

        mapAndCourse = SceneManager.GetActiveScene().name + "_" + course;

        StartCoroutine(BeepTimer(1));

        courseName.text = "\"" + course + "\"";
        time = 0.0f;
        timerStarted = true;
        UpdatePrevAndPB();
    }


    bool canFinish = false;
    int prevCheckpoint = 0;

    public void TCheckpoint(object[] info)
    {
        if (!timerStarted)
            return;

        string course = (string)info[0];
        int checkpointNumber = (int)info[1];
        int checkpointCount = (int)info[2];

        if (checkpointNumber != prevCheckpoint + 1)
        {
            return;
        }

        foreach (TimerTrigger trigger in triggers)
        {
            trigger.SetMatActive(course, checkpointNumber + 1);
        }

        prevCheckpoint = checkpointNumber;

        //course == courseName.text && 
        canFinish = checkpointNumber == checkpointCount;

        checkpointSound.Play();

        Debug.Log("Checkpoint" + checkpointNumber + "/" + checkpointCount);
    }

    public void TEnd()
    {
        if (!canFinish)
            return;

        if (!timerStarted)
            return;

        timerStarted = false;

        LogTime();

        bool pb = previousTime > 0.0f ? time < previousTime : false;

        if (pb)
            pbSound.Play();


        previousTime = time;

        UpdatePrevAndPB();
        SaveLoadSystem.Instance.Save();

        foreach (TimerTrigger trigger in triggers)
        {
            trigger.HideMat();
        }
        
        Invoke(nameof(HideCanvas), 5.0f);
        StartCoroutine(BlinkTimer());
        StartCoroutine(BeepTimer(4));


        string announcement = "";

        if (pb)
            announcement = " set a PB on " + courseName.text + " with a time of " + FormatTime(time);
        else
            announcement = " completed " + courseName.text + " with a time of " + FormatTime(time);

        AnnounceFinish(announcement);

    }

    void AnnounceFinish(string a)
    {
        GameObject.Find("LobbyController").GetComponent<LobbyChatUILogic>().SendChatMessage(a);
    }

    void UpdatePrevAndPB()
    {
        float pb = -1.0f;

        if (courseTimes.ContainsKey(mapAndCourse))
            pb = courseTimes[mapAndCourse];

        if (pb < 0.0f)
            timeUIPrevAndPB.text = "\nPersonal Best: -";
        else
            timeUIPrevAndPB.text = "\nPersonal Best: " + FormatTime(pb);

        if (previousTime < 0.0f)
            timeUIPrevAndPB.text += "\nPrevious Time: -";
        else
            timeUIPrevAndPB.text += "\nPrevious Time: " + FormatTime(previousTime);
    }

    public void LogTime()
    {
        if (!courseTimes.ContainsKey(mapAndCourse))
            courseTimes.Add(mapAndCourse, 0.0f);

        if (time < courseTimes[mapAndCourse] || Mathf.Approximately(courseTimes[mapAndCourse], 0.0f))
            courseTimes[mapAndCourse] = time;


        Debug.Log("Course Times:");
        foreach (KeyValuePair<string, float> kvp in courseTimes)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    public void TShowUI(string course)
    {
        mapAndCourse = SceneManager.GetActiveScene().name + "_" + course;

        CancelInvoke(nameof(HideCanvas));
        canvas.enabled = true;

        if (course != previousCourse)
        {
            previousTime = -1.0f;
            previousCourse = course;
        }

        foreach (TimerTrigger trigger in triggers)
        {
            if (trigger.transform.parent.gameObject.name == course)
                trigger.ResetMat();
            else
                trigger.HideMat();
        }

        courseName.text = "\"" + course + "\"";

        timeUI.text = FormatTime(0f);

        UpdatePrevAndPB();
    }

    public void THideUI(string course)
    {
        if (timerStarted) return;

        foreach (TimerTrigger trigger in triggers)
        {
            trigger.HideMat();
        }

        CancelInvoke(nameof(HideCanvas));
        canvas.enabled = false;

    }

    private IEnumerator BlinkTimer()
    {
        for (int i = 0; i < 5; i++)
        {
            timeUI.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            timeUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.35f);
        }
    }

    private IEnumerator BeepTimer(int nb)
    {
        for (int i = 0; i < nb; i++)
        {
            beep.Play();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void HideCanvas()
    {
        canvas.enabled = false;
    }

    public static string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        int milliseconds = Mathf.FloorToInt((totalSeconds * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }


    public object SaveState()
    {
        return new SaveData()
        {
            courseTimes = this.courseTimes
            
        };
    }
    public void LoadState(JContainer state)
    {
        SaveData saveData = state.ToObject<SaveData>();

        courseTimes = saveData.courseTimes;
    }

    [Serializable]
    private struct SaveData
    {
        public Dictionary<string, float> courseTimes;
    }
}
