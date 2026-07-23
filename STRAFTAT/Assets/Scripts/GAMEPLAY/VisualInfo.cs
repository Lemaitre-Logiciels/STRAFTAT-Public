using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet;
using FishNet.Object;

public class VisualInfo : NetworkBehaviour
{
    public TextMeshProUGUI name;

    [Header("Tweaks")]
    public Transform lookAt;
    public Vector3 offset;

    [Header("Logic")]
    public Camera cam;
    public GameObject container;

    void Update()
    {
        if (cam == null || IsOwner) {
            container.SetActive(false);
            return;
        }
        else 
            container.SetActive(true);

        Vector3 pos = cam.WorldToScreenPoint(lookAt.position + offset);

        if (transform.position != pos)
            transform.position = pos;
    }
}
