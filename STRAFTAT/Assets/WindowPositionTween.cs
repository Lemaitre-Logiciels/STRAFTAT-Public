using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

public class WindowPositionTween : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private Ease easeType;
    [SerializeField] private bool options;

    void Update()
    {
        if (options && SceneManager.GetActiveScene().name != "MainMenu")
        {
            //if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Menu")) && transform.localScale == Vector3.one) ChangeWindowState();
        }

        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Menu"))) {
            if (PauseManager.Instance.gamepad) 
            {
                if (panel.activeSelf) PauseManager.Instance.ChangeSelectedItem(optionsButton);
                else PauseManager.Instance.ChangeSelectedItem(resumeButton);
            }
            panel.SetActive(!panel.activeSelf);
        }
        //if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Menu")) && transform.localScale == Vector3.one) ChangeWindowState();
        
    }

    public void ChangeWindowState()
    {
        transform.DOKill();

        if (transform.localScale == Vector3.zero) 
        {
            //transform.DOMove(openPosition.position, tweenDuration).SetEase(easeType);
            transform.localScale = Vector3.one;
        }
        else {
            //transform.DOMove(closedPosition.position, tweenDuration).SetEase(easeType);
            transform.localScale = Vector3.zero;
            //EventSystem.current.SetSelectedGameObject(null);
        }

    }
}
