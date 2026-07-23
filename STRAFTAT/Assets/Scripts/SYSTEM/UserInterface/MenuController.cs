using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MenuController : MonoBehaviour
{
    [SerializeField] private PostProcessVolume blurCamera;
    [SerializeField] private DepthOfField depth;
    [SerializeField] private GameObject mapsMenu;
    public GameObject hatsMenu;
    public GameObject playMenu;
    [SerializeField] private GameObject statsMenu;
    [SerializeField] private GameObject progressMenu;
    public GameObject startMenu;
    [SerializeField] private GameObject buttonBar;
    [SerializeField] private GameObject hud;
    [SerializeField] private AudioClip openingClip;

    [Space]
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject startScreenButton;

    public static MenuController Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (blurCamera.profile.TryGetSettings(out DepthOfField lens))
        {
            depth = lens;
        }
    }

    void Start()
    {
        depth.enabled.value  = false;
    }

    void Update()
    {
        /*if (Input.anyKey && startMenu.activeSelf) 
        {
            OpenGame();
        }*/
    }

    public void OpenGame()
    {
        
        if (startMenu.activeSelf) SoundManager.Instance.PlaySound(openingClip);
        ActivateMenu(playMenu);
    }
    
    public void ActivateMenu(GameObject menu)
    {
        SoundManager.Instance.SetHighpass = false;
        SoundManager.Instance.SetLowpass = false;
        depth.enabled.value  = true;
        mapsMenu.SetActive(false);
        hatsMenu.SetActive(false);
        playMenu.SetActive(false);
        statsMenu.SetActive(false);
        progressMenu.SetActive(false);
        startMenu.SetActive(false);
        buttonBar.SetActive(true);
        hud.SetActive(true);

        menu.SetActive(true);

        if (menu == playMenu) 
        {
            backButton.SetActive(false);
            startScreenButton.SetActive(true);
        }
        else{
            backButton.SetActive(true);
            startScreenButton.SetActive(false);
        }

        if (menu == mapsMenu)
        {
            MapsManager.Instance.OpenFirstPlaylist();
        }
        
        //music filters
        if (menu == mapsMenu) SoundManager.Instance.SetHighpass = true;
        if (menu == hatsMenu) SoundManager.Instance.SetLowpass = true;
    }

    [Space]
    [SerializeField] private GameObject PlayButton;

    public void OpenStartMenu()
    {
        mapsMenu.SetActive(false);
        hatsMenu.SetActive(false);
        playMenu.SetActive(false);
        statsMenu.SetActive(false);
        progressMenu.SetActive(false);
        startMenu.SetActive(true);
        buttonBar.SetActive(false);
        hud.SetActive(false);
        depth.enabled.value  = false;

        PauseManager.Instance.ChangeSelectedItem(PlayButton);
    }
}
