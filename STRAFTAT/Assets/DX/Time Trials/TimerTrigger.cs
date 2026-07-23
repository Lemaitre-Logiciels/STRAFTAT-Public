using FishNet.Managing.Timing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        Start,
        Checkpoint,
        End,
        ShowUI
    }

    [SerializeField]
    private TriggerType type = TriggerType.Start;

    TimerManager timerManager;

    string courseName;

    Renderer renderer;
    Material matDefault;
    public Material checkpointMatActive;

    private void Start()
    {
        timerManager = FindObjectOfType<TimerManager>();
        courseName = transform.parent.name;
        renderer = GetComponent<Renderer>();
        matDefault = renderer.material;
        HideMat();
    }

    public void SetMatActive(string course, int checkpointNumber)
    {
        if (courseName != course)
            return;
        if (type != TriggerType.Checkpoint)
            return;

        if (checkpointNumber == transform.GetSiblingIndex() - 1)
            renderer.material = checkpointMatActive;
        else
            ResetMat();
    }

    void SetMatActiveFirstCheckpoint(string c)
    {
        if (courseName != c)
            return;
        if (type != TriggerType.Checkpoint)
            return;
        renderer.material = checkpointMatActive;
    }

    void SetMatVisible(string c)
    {
        if (courseName != c)
            return;
        if (type != TriggerType.Start)
            return;
        ResetMat();
    }

    void SetMatInvisible(string c)
    {
        if (courseName != c)
            return;
        if (type != TriggerType.Start)
            return;
        HideMat();
    }

    public void ResetMat()
    {
        renderer.material = matDefault;
    }

    public void HideMat()
    {
        if (type == TriggerType.Start) {
            ResetMat();
            return;
        }
        renderer.material = timerManager.invisibleMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (type == TriggerType.Start)
            {
                for (int i=0; i < transform.parent.childCount; i++) transform.parent.GetChild(i).SendMessage("SetMatVisible", courseName);
                timerManager.SendMessage("TStart", courseName, SendMessageOptions.DontRequireReceiver);
                transform.parent.GetChild(transform.GetSiblingIndex() + 1).SendMessage("SetMatActiveFirstCheckpoint", courseName);
                timerManager.player = other.GetComponent<FirstPersonController>();
                timerManager.currentStartPoint = this.transform;
            }
            if (type == TriggerType.Checkpoint)
                timerManager.SendMessage("TCheckpoint", new object[] { courseName, transform.GetSiblingIndex() - 1, transform.parent.childCount - 3} , SendMessageOptions.DontRequireReceiver);
            if (type == TriggerType.End) {
                for (int i=0; i < transform.parent.childCount; i++) transform.parent.GetChild(i).SendMessage("SetMatInvisible", courseName);
                timerManager.SendMessage("TEnd", SendMessageOptions.DontRequireReceiver);
            }
            if (type == TriggerType.ShowUI && !timerManager.timerStarted)
                timerManager.SendMessage("TShowUI", courseName, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (type == TriggerType.ShowUI)
                timerManager.SendMessage("THideUI", courseName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
