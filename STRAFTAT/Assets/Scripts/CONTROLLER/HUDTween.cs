using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HUDTween : MonoBehaviour
{
    [SerializeField] private Transform hudUp;
    [SerializeField] private Transform hudDown;
    [SerializeField] private float tweenTime = 0.4f;
    [SerializeField] private Ease ease;
    bool trigger;

    ClientInstance clientScript;
    void Start()
    {
        clientScript = GetComponentInParent<PlayerValues>().playerClient;
    }

    // Update is called once per frame
    void Update()
    {
        if (clientScript.nonSteamworksTransport)
        {
            hudUp.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
            hudDown.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
        }

        if(Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftControl) && Application.isEditor)
        {
            hudUp.gameObject.SetActive(!hudUp.gameObject.activeSelf);
            hudDown.gameObject.SetActive(!hudDown.gameObject.activeSelf);
        }
        
        if (trigger) return;
        if (!trigger && PauseManager.Instance.onStartRoundScreen)
        {
            trigger = true;
            hudUp.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
            hudDown.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
        }

        
    }

    public void MoveUp()
    {
        hudUp.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
        hudDown.DOLocalMove(Vector3.zero, tweenTime).SetEase(ease);
    }
}
