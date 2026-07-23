using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingName : MonoBehaviour
{
    public string nameToShow;
    [SerializeField] private TextMeshProUGUI text;
    public static FloatingName Instance;
    public GameObject display;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    PauseManager pauseManager;

    void Start()
    {
        pauseManager = PauseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseManager.inMainMenu) nameToShow = "";
        if (nameToShow == "") display.SetActive(false);
        else display.SetActive(true);
        text.text = nameToShow;
        transform.position = Input.mousePosition;
    }
}
